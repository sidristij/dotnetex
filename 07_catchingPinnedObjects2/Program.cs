using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CLR;
using System.Runtime.InteropServices;
using System.Threading;

namespace _07_catchingPinnedObjects2
{
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
            var objects = new Dictionary<Type, int>(7000);

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
                var highFreqHeapEnd = types.Keys.Max(val => (long)val);

                // for gaps calculations
                var lastRecognized = (long)managedStart;
                var lostMemory = 0L;

                // for each byte in virtual memory block, we trying to find strings
                for (long ptr = (long)managedStart, end = (long)managedEnd; ptr < end; ptr++)
                {
                    var mt = *(IntPtr*) ptr;
                    if (types.ContainsKey(mt))
                    {
                        // checking next object.
                        int size;
                        try
                        {
                            size = GCEx.SizeOf((EntityInfo*) (ptr - IntPtr.Size));
                        }
                        catch (OverflowException)
                        {
                            continue;
                        }
                        if(ptr + size >= (long)managedEnd)
                            continue;

                        var next_mt = *(IntPtr*) (ptr + size);

                        // object found
                        if (types.ContainsKey(next_mt))
                        {
                            var gap = (ptr - 4) - lastRecognized;
                            lostMemory += gap;

                            var found = EntityPtr.ToInstance<object>((IntPtr) (ptr - IntPtr.Size));
                            RegisterObject(objects, found);

                            object lastInChain = found;
                            foreach (var @object in GCEx.GetObjectsInSOH(found, hmt => hmt > highFreqHeapStart && hmt <= highFreqHeapEnd && types.ContainsKey((IntPtr)hmt)))
                            {
                                RegisterObject(objects, @object);
                                lastInChain = @object;
                            }

                            ptr = (long)EntityPtr.ToPointer(lastInChain) + GCEx.SizeOf(lastInChain); // 12 = sizeof(object);
                            lastRecognized = ptr;
                        }
                    }
                }
                
                var foundSize = lostMemory;
                var total = (long)managedEnd - (long)managedStart;

                foreach (var type in objects.Keys.OrderByDescending(key => objects[key]))
                {
                    Console.WriteLine("{0:00000} : {1}", objects[type], type.FullName);
                }

                Console.WriteLine("TOTAL UNRESOLVED: {0} from {1} ({2}%), objects total: {3}", foundSize, total, ((float)foundSize / total) * 100, objects.Values.Sum());
                Thread.CurrentThread.Priority = ThreadPriority.Normal;
            }
            Console.ReadKey();
        }

        private static void RegisterObject(Dictionary<Type, int> dict, object obj)
        {
            var type = obj.GetType();
            if (!dict.ContainsKey(type))
            {
                dict[type] = 0;
            }

            dict[type] = dict[type] + 1;
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
                while (VirtualQuery(offset, ref memoryBasicInformation, (IntPtr)Marshal.SizeOf(memoryBasicInformation)) !=
                       IntPtr.Zero)
                {
                    var isManagedHeap = (long)memoryBasicInformation.BaseAddress < (long)somePtr &&
                                        (long)somePtr <
                                        ((long)memoryBasicInformation.BaseAddress + (long)memoryBasicInformation.RegionSize);

                    if (isManagedHeap || !heaponly)
                    {
                        Console.WriteLine(
                            "{7} base addr: 0x{0:X8} size: 0x{1:x8} type: {2:x8} alloc base: {3:x8} state: {4:x8} prot: {5:x2} alloc prot: {6:x8}",
                            (int)memoryBasicInformation.BaseAddress,
                            memoryBasicInformation.RegionSize,
                            memoryBasicInformation.Type,
                            (int)memoryBasicInformation.AllocationBase,
                            memoryBasicInformation.State,
                            memoryBasicInformation.Protect,
                            memoryBasicInformation.AllocationProtect,
                            isManagedHeap ? " ** " : "    ");
                    }

                    if (isManagedHeap)
                    {
                        heapsOffset = offset;
                        lastHeapByte = (IntPtr)((long)offset + (long)memoryBasicInformation.RegionSize);
                    }

                    offset = (IntPtr)((long)offset + (long)memoryBasicInformation.RegionSize);
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
    }
    // ReSharper restore InconsistentNaming
}
