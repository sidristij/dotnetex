using System;
using System.Runtime.CLR;
using System.Runtime.InteropServices;

namespace _04_virtualMemory
{
    static class Program
    {
        private const String Kernel32 = "kernel32.dll";
        private static readonly IntPtr StringsTable;

        static Program()
        {
            StringsTable = typeof(string).TypeHandle.Value;
        }

        static void Main()
        {
            var offset = IntPtr.Zero;

            IntPtr heapsOffset, lastHeapByte;
            GetManagedHeap(offset, out heapsOffset, out lastHeapByte);

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
            
            for (long strMtPointer = (int)heapsOffset, end = (int)lastHeapByte; strMtPointer < (end - IntPtr.Size); strMtPointer++)
            {
                if (!IsString(strMtPointer)) continue;

                var str = EntityPtr.ToInstance<string>(new IntPtr(strMtPointer - 4));
                Console.WriteLine(str);
                count++;
            }
            Console.WriteLine("Total count: {0}", count);
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

        /// <summary>
        /// Gets managed heap address
        /// </summary>
        private static unsafe void GetManagedHeap(IntPtr offset, out IntPtr heapsOffset, out IntPtr lastHeapByte)
        {
            var somePtr = EntityPtr.ToPointer(new object());
            var meminfo = new MEMORY_BASIC_INFORMATION();

            VirtualQuery(somePtr, ref meminfo, (IntPtr) Marshal.SizeOf(meminfo));

            Console.WriteLine( 
                "base addr: 0x{0:X8} size: 0x{1:x8} type: {2:x8} alloc base: {3:x8} state: {4:x8} prot: {5:x2} alloc prot: {6:x8}",
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
