using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CLR;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace _06_catchingPinnedObjects
{
    internal class Gap
    {
        public long Offset;
        public long Size;
    }

    class Program
    {
        internal const String KERNEL32 = "kernel32.dll";
        internal static IntPtr StringsTable;

        static Program()
        {
            StringsTable = typeof(string).TypeHandle.Value;
        }

        static unsafe void Main(string[] args)
        {
            var offset = IntPtr.Zero;
            var gapsList = new List<Gap>(1000);
            var objects = new List<object>(1000);
            unsafe
            {
                // "Suspend" other threads
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                
                // Get current heap ranges
                IntPtr managedStart, managedEnd;
                GetManagedHeap(offset, out managedStart, out managedEnd, true);
                    
                // calculating memory ranges
                var types = MakeTypesMap();

                // Run GC before we need to have pinned state for all objects
                GC.Collect();
                GC.WaitForFullGCComplete();


                // actually, we should calc ranges for each AppDomain and mscorlib as in Shared.
                var highFreqHeapStart = types.Keys.Min(val => (long)val);
                var highFreqHeapEnd   = types.Keys.Max(val => (long)val);

                // for gaps calculations
                var lastObjectPtr = (long)managedStart;

                // for each byte in virtual memory block, we trying to find strings
                for (long strMtPointer = (long)managedStart, end = (long)managedEnd; strMtPointer < end; strMtPointer++)
                {
                    // If not string, not our story
                    if (!IsString(strMtPointer)) continue;

                    // priting gap
                    var gap = strMtPointer - lastObjectPtr;
                    if (gap > 0 && gap > 12)
                    {
                        gapsList.Add(new Gap { Offset = lastObjectPtr - 4, Size = gap });
                    }

                    // If string is found, we can iterate all objects after string.
                    var str = EntityPtr.ToInstance<object>((IntPtr) (strMtPointer - 4));
                    foreach (var @object in GCEx.GetObjectsInSOH(str, (mt) => mt > highFreqHeapStart && mt <= highFreqHeapEnd && types.ContainsKey((IntPtr)mt)))
                    {
                        Console.Write("{0} -- ", @object.GetType().FullName);

                        lastObjectPtr = (long)EntityPtr.ToPointer(@object);
                        objects.Add(@object);

                        // on string we break because we will find this address in main loop
                        if (@object is string)
                        {
                            strMtPointer = lastObjectPtr - 1 - 4;
                            break;
                        }
                    }
                }

                // Gaps:
                foreach (var gap in gapsList)
                {
                    Console.WriteLine("For GAP size: {0}; ", gap.Size);
                    var lastObjPtr = gap.Offset + gap.Size;
                    var gapEndObject = EntityPtr.ToInstance<object>((IntPtr)(lastObjPtr));
                    var resolved = 0;
                    // for each byte in gap, we trying to find other objects
                    for (long backptr = gap.Offset + gap.Size, end = gap.Offset; backptr > end; backptr--)
                    {
                        var mt = *(IntPtr*) backptr;
                        if (types.ContainsKey(mt))
                        {
                            var unsafeObject = EntityPtr.ToInstance<object>((IntPtr)(backptr - 4));
                            
                            if (GCEx.SizeOf(unsafeObject) != (lastObjPtr - backptr + 4))
                                break;
                            ;
                            if (objects.Contains(unsafeObject))
                                break;

                            if (GCEx.IsAchievableFrom(unsafeObject, gapEndObject,
                                l => l > highFreqHeapStart && l <= highFreqHeapEnd && types.ContainsKey((IntPtr) l)))
                            {
                                var size = GCEx.SizeOf(unsafeObject);
                                Console.WriteLine("{0} found, size: {1}", unsafeObject.GetType(), size);
                                resolved += size;
                                gapEndObject = unsafeObject;
                                lastObjPtr = backptr - 4;
                                gap.Size -= size;
                            }
                        }
                    }

                    Console.WriteLine(" == RESOLVED: {0} from {1}, diff = {2}", resolved, gap.Size, gap.Size - resolved);
                }
                var foundSize = gapsList.Sum(g => g.Size);
                var total = (long) managedEnd - (long) managedStart;
                Console.WriteLine("TOTAL UNRESOLVED: {0} from {1} ({2}%)", gapsList.Sum(g => g.Size), (long)managedEnd - (long)managedStart, ((float)foundSize / total) * 100);
                Thread.CurrentThread.Priority = ThreadPriority.Normal;
            }
            Console.ReadKey();
        }

        private static Dictionary<IntPtr, Type> MakeTypesMap()
        {
            var dict = new Dictionary<IntPtr, Type>(10000);
            var queue = new Queue<Type>(1000);

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in asm.GetTypes())
                {
                    queue.Enqueue(type);
                }

                while (queue.Count != 0)
                {
                    var type = queue.Dequeue();
                    var nested_types = type.GetNestedTypes();

                    foreach (var nested in nested_types)
                    {
                        queue.Enqueue(nested);
                    }

                    dict[type.TypeHandle.Value] = type;
                }
            }
            return dict;
        }

        private static unsafe bool IsString(long strMtPointer)
        {
            int count;
            if (*(IntPtr*) strMtPointer == StringsTable)
            {
                var entity = strMtPointer - IntPtr.Size; // move to sync block
                if (GCEx.MajorNetVersion >= 4)
                {
                    var length = *(int*) ((int) entity + 8);
                    if (length < 2048 && *(short*) ((int) entity + 12 + length*2) == 0)
                    {
                        return true;
                    }
                }
                else
                {
                    var length = *(int*) ((int) entity + 12);
                    if (length < 2048 && *(short*) ((int) entity + 14 + length*2) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Gets managed heap address
        /// </summary>
        private static unsafe void GetManagedHeap(IntPtr offset, out IntPtr heapsOffset, out IntPtr lastHeapByte, bool heaponly)
        {
            var somePtr = EntityPtr.ToPointer("sample");
            var memoryBasicInformation = new MEMORY_BASIC_INFORMATION();

            heapsOffset = IntPtr.Zero;
            lastHeapByte = IntPtr.Zero;
            unsafe
            {
                while (VirtualQuery(offset, ref memoryBasicInformation, (IntPtr) Marshal.SizeOf(memoryBasicInformation)) !=
                       IntPtr.Zero)
                {
                    var isManagedHeap = (long) memoryBasicInformation.BaseAddress < (long) somePtr &&
                                        (long) somePtr <
                                        ((long) memoryBasicInformation.BaseAddress + (long) memoryBasicInformation.RegionSize);

                    if (isManagedHeap || !heaponly)
                    {
                        Console.WriteLine(
                            "{7} base addr: 0x{0:X8} size: 0x{1:x8} type: {2:x8} alloc base: {3:x8} state: {4:x8} prot: {5:x2} alloc prot: {6:x8}",
                            (int) memoryBasicInformation.BaseAddress,
                            memoryBasicInformation.RegionSize,
                            memoryBasicInformation.Type,
                            (int) memoryBasicInformation.AllocationBase,
                            memoryBasicInformation.State,
                            memoryBasicInformation.Protect,
                            memoryBasicInformation.AllocationProtect,
                            isManagedHeap ? " ** " : "    ");
                    }

                    if (isManagedHeap)
                    {
                        heapsOffset = offset;
                        lastHeapByte = (IntPtr) ((long) offset + (long) memoryBasicInformation.RegionSize);
                    }

                    offset = (IntPtr) ((long) offset + (long) memoryBasicInformation.RegionSize);
                }
            }
        }


        // ReSharper disable InconsistentNaming

        [DllImport(KERNEL32, SetLastError = true)]
        unsafe internal static extern IntPtr VirtualQuery(
            IntPtr address,
            ref MEMORY_BASIC_INFORMATION buffer,
            IntPtr sizeOfBuffer
        );

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct MEMORY_BASIC_INFORMATION
        {
            internal void* BaseAddress;
            internal void* AllocationBase;
            internal uint AllocationProtect;
            internal UIntPtr RegionSize;
            internal uint State;
            internal uint Protect;
            internal uint Type;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SYSTEM_INFO {
            internal int dwOemId;    // This is a union of a DWORD and a struct containing 2 WORDs.
            internal int dwPageSize;
            internal IntPtr lpMinimumApplicationAddress;
            internal IntPtr lpMaximumApplicationAddress;
            internal IntPtr dwActiveProcessorMask;
            internal int dwNumberOfProcessors;
            internal int dwProcessorType;
            internal int dwAllocationGranularity;
            internal short wProcessorLevel;
            internal short wProcessorRevision;
        }
 
        [DllImport(KERNEL32, SetLastError = true)]
        [SecurityCritical]
        internal static extern void GetSystemInfo(ref SYSTEM_INFO lpSystemInfo);
    
    }
    
    // ReSharper restore InconsistentNaming
}
