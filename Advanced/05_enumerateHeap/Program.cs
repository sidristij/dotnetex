using System;
using System.Runtime.CLR;
using System.Runtime.InteropServices;

namespace _05_enumerateHeap
{
    static class Program
    {
        private const String Kernel32 = "kernel32.dll";
        private static IntPtr StringsTable;
        private static IntPtr MscorlibModule;

        static unsafe Program()
        {
            StringsTable = typeof(string).TypeHandle.Value;
            MscorlibModule = (IntPtr)((MethodTableInfo*)typeof(string).TypeHandle.Value)->ModuleInfo;
        }

        static void Main()
        {
            IntPtr heapsOffset, lastHeapByte;
            GetManagedHeap(out heapsOffset, out lastHeapByte);

            // looking up structures
            if (heapsOffset != IntPtr.Zero)
            {
                EnumerateStrings(heapsOffset, lastHeapByte);
            }
            Console.ReadKey();
        }

        /// <summary>
        /// Enumerates all strings in heap
        /// </summary>
        /// <param name="heapsOffset">Heap starting point</param>
        /// <param name="lastHeapByte">Heap last byte</param>
        private static void EnumerateStrings(IntPtr heapsOffset, IntPtr lastHeapByte)
        {
            var count = 0;
            var strcount = 0;
            var firstFound = string.Empty;
            var first = false;
            for (int strMtPointer = (int)heapsOffset, end = (int)lastHeapByte; strMtPointer < (end - IntPtr.Size); strMtPointer++)
            {
                if (IsString(strMtPointer))
                {
                    Console.WriteLine(EntityPtr.ToInstance<string>(new IntPtr(strMtPointer - 4)));
                    if (!first)
                    {
                        firstFound = EntityPtr.ToInstance<string>(new IntPtr(strMtPointer - 4));
                        first = true;
                    }
                    strcount++;
                }
            }

            foreach (var obj in GCEx.GetObjectsInSOH(firstFound, mt => IsCorrectMethodsTable((IntPtr) mt)))
            {
                Console.WriteLine("   {0}: {1}", obj.Item.GetType().Name, obj.Item);
                count++;
            }
            Console.WriteLine("objects count: {0}, strings: {1}", count, strcount);
        }

        private static unsafe bool IsString(long strMtPointer)
        {
            if (*(IntPtr*) strMtPointer != StringsTable) return false;

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
            return false;
        }

        private static unsafe bool IsCorrectMethodsTable(IntPtr mt)
        {
            if (mt == IntPtr.Zero) return false;

            if (PointsToAllocated(mt))
                if (PointsToAllocated((IntPtr)((MethodTableInfo*)mt)->EEClass))
                    if (PointsToAllocated((IntPtr)((MethodTableInfo*)mt)->EEClass->MethodsTable))
                        return ((IntPtr)((MethodTableInfo*)mt)->EEClass->MethodsTable == mt) /*||
                               ((IntPtr)((MethodTableInfo*)mt)->ModuleInfo == MscorlibModule)*/;

            return false;
        }
        
        private static bool PointsToAllocated(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero) return false;
            return !IsBadReadPtr(ptr, 32);
        }

        /// <summary>
        /// Gets managed heap address
        /// </summary>
        private static unsafe void GetManagedHeap(out IntPtr heapsOffset, out IntPtr lastHeapByte)
        {
            var somePtr = EntityPtr.ToPointer("sample");
            var meminfo = new MEMORY_BASIC_INFORMATION();


            VirtualQuery(somePtr, ref meminfo, (IntPtr)Marshal.SizeOf(meminfo));

            Console.WriteLine(
                "Base addr: 0x{0:X8} size: 0x{1:x8} type: {2:x8} alloc base: {3:x8} state: {4:x8} prot: {5:x2} alloc prot: {6:x8}",
                (int) meminfo.BaseAddress,
                (int) meminfo.RegionSize,
                meminfo.Type,
                (int) meminfo.AllocationBase,
                meminfo.State,
                meminfo.Protect,
                meminfo.AllocationProtect);
        
            heapsOffset = (IntPtr) meminfo.BaseAddress;
            lastHeapByte = (IntPtr) ((long) heapsOffset + (long) meminfo.RegionSize);
        }


        // ReSharper disable InconsistentNaming
        [DllImport(Kernel32, SetLastError = true)]
        private static extern IntPtr VirtualQuery(
            IntPtr address,
            ref MEMORY_BASIC_INFORMATION buffer,
            IntPtr sizeOfBuffer
        );

        [DllImport(Kernel32, SetLastError = true)]
        internal static extern bool IsBadReadPtr(IntPtr address, uint ucb);

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct MEMORY_BASIC_INFORMATION
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
