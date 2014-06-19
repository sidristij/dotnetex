using System;
using System.Runtime.CLR;
using System.Runtime.InteropServices;
using System.Security;

namespace _04_virtualMemory
{
    static class Program
    {
        private static readonly IntPtr StringsTable;
        private const String Kernel32 = "kernel32.dll";

        static Program()
        {
            StringsTable = typeof(string).TypeHandle.Value;
        }

        static void Main(string[] args)
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
            
            for (long strMtPointer = heapsOffset.ToInt64(), end = lastHeapByte.ToInt64(); strMtPointer < end; strMtPointer++)
            {
                if (IsString(strMtPointer))
                {
                    var str = EntityPtr.ToInstance<string>(new IntPtr(strMtPointer - 4));
                    Console.WriteLine(str);
                    count++;
                }
            }

            Console.WriteLine("Total count: {0}", count);
        }

        private static unsafe bool IsString(long strMtPointer)
        {
            int count;
            if (GCEx.PointsToAllocated((IntPtr)strMtPointer) && *(IntPtr*) strMtPointer == StringsTable)
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
        private static void GetManagedHeap(out IntPtr heapsOffset, out IntPtr lastHeapByte)
        {
            var offset = EntityPtr.ToPointer(new object());

            var memoryBasicInformation = new MEMORY_BASIC_INFORMATION();

            unsafe
            {
                VirtualQuery(offset, ref memoryBasicInformation, (IntPtr) Marshal.SizeOf(memoryBasicInformation));

                heapsOffset = (IntPtr)memoryBasicInformation.AllocationBase;
                lastHeapByte = (IntPtr) ((long) offset + (long) memoryBasicInformation.RegionSize);
            }
        }


        // ReSharper disable All

        [DllImport(Kernel32, SetLastError = true)]
        unsafe internal static extern IntPtr VirtualQuery(
            IntPtr address,
            ref MEMORY_BASIC_INFORMATION buffer,
            IntPtr sizeOfBuffer
        );

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct MEMORY_BASIC_INFORMATION
        {
            public void* BaseAddress;
            public void* AllocationBase;
            public uint AllocationProtect;
            public UIntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
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
 
        [DllImport(Kernel32, SetLastError = true)]
        [SecurityCritical]
        internal static extern void GetSystemInfo(ref SYSTEM_INFO lpSystemInfo);
    }

    // ReSharper restore All
}
