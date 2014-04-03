using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CLR;
using System.Runtime.InteropServices;
using System.Threading;

namespace _07_catchingPinnedObjects2
{
    internal class Program
    {
        internal const String KERNEL32 = "kernel32.dll";
        internal static IntPtr StringsTable;
        internal static IntPtr MscorlibModule;

        static unsafe Program()
        {
            StringsTable = typeof (string).TypeHandle.Value;
            MscorlibModule = (IntPtr)((MethodTableInfo*)typeof(string).TypeHandle.Value)->ModuleInfo;
        }

        private static unsafe void Main(string[] args)
        {
            var offset = IntPtr.Zero;
            var objects = new Dictionary<Type, int>(7000);

            unsafe
            {
                // "Suspend" other threads
                Thread.CurrentThread.Priority = ThreadPriority.Highest;

                // Get current heap ranges
                IntPtr managedStart, managedEnd;

                // Run GC before we need to have pinned state for all objects
                //GC.Collect();
                //GC.WaitForFullGCComplete();

                Console.ReadKey();
                GetManagedHeap(offset, out managedStart, out managedEnd);

                // for gaps calculations
                var lastRecognized = (long) managedStart;
                var lostMemory = 0L;

                // for each byte in virtual memory block, we trying to find strings
                var stopwatch = Stopwatch.StartNew();
                for (IntPtr* ptr = (IntPtr*)managedStart, end = (IntPtr *)managedEnd; ptr < end; ptr++)
                {
                    if (IsCorrectMethodsTable(*(IntPtr*)ptr))
                    {
                        // checking next object.
                        int size;
                        try
                        {
                            size = GCEx.SizeOf((EntityInfo*) (ptr - 1)) >> 2;
                        }
                        catch (OverflowException)
                        {
                            continue;
                        }

                        if (ptr + size > (long *) managedEnd)
                            continue;

                        {
                            var gap = (ptr - 1) - ((lastRecognized + ((lastRecognized == managedStart.ToInt64()) ? 0 : GCEx.SizeOf((EntityInfo*)lastRecognized))) >> 2);

                            lostMemory += (long)gap;

                            var found = EntityPtr.ToInstance<object>((IntPtr) (ptr - 1));
                            RegisterObject(objects, found);

                            var lastInChain = found;
                            foreach (var item in GCEx.GetObjectsInSOH(found, hmt => IsCorrectMethodsTable((IntPtr)hmt)))
                            {
                                RegisterObject(objects, item.Item);
                                if(!item.IsArrayItem) lastInChain = item.Item;
                            }

                            lastRecognized = (long)EntityPtr.ToPointer(lastInChain);
                            ptr = (IntPtr*)(lastRecognized + GCEx.SizeOf(lastInChain));
                        }
                    }
                }
                var timeToTakeSnapshot = stopwatch.ElapsedMilliseconds;
                var foundSize = lostMemory;
                var total = (long) managedEnd - (long) managedStart;

                foreach (var type in objects.Keys.OrderByDescending(key => objects[key]))
                {
                    Console.WriteLine("{0:00000} : {1}", objects[type], type.FullName);
                }

                Console.WriteLine("TOTAL UNRESOLVED: {0} from {1} ({2}%), objects total: {3}. Time taken: {4}", foundSize, total,
                    ((float) foundSize/total)*100, objects.Values.Sum(), timeToTakeSnapshot);
                Thread.CurrentThread.Priority = ThreadPriority.Normal;
            }
            Console.ReadKey();
        }

        private static unsafe bool IsCorrectMethodsTable(IntPtr mt)
        {
            if (mt == IntPtr.Zero) return false;
            
            if (PointsToAllocated(mt))
                if (PointsToAllocated((IntPtr) ((MethodTableInfo*) mt)->EEClass))
                    if (PointsToAllocated((IntPtr) ((MethodTableInfo*) mt)->EEClass->MethodsTable))
                        return ((IntPtr)((MethodTableInfo*) mt)->EEClass->MethodsTable == mt) || 
                               ((IntPtr) ((MethodTableInfo*) mt)->ModuleInfo == MscorlibModule);
            
            return false;
        }

        private static bool PointsToAllocated(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero) return false;
            return !IsBadReadPtr(ptr, 32);
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

        /// <summary>
        /// Gets managed heap address
        /// </summary>
        private static unsafe void GetManagedHeap(IntPtr offset, out IntPtr heapsOffset, out IntPtr lastHeapByte)
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
        internal static extern unsafe bool IsBadReadPtr(IntPtr address, uint ucb);

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
