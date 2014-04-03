using System;
using System.Runtime.CLR;
using System.Runtime.InteropServices;
using System.Security;

namespace _04_virtualMemory
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

            IntPtr heapsOffset, lastHeapByte;
            GetManagedHeap(offset, out heapsOffset, out lastHeapByte, true);

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
        private static unsafe void EnumerateStrings(IntPtr heapsOffset, IntPtr lastHeapByte)
        {
            var count = 0;
            
            for (long strMtPointer = heapsOffset.ToInt64(), end = lastHeapByte.ToInt64(); strMtPointer < end; strMtPointer++)
            {
                try
                {
                    if (IsString(strMtPointer))
                    {
                        var str = EntityPtr.ToInstance<string>(new IntPtr(strMtPointer - 4));
                        Console.WriteLine(str);
                        count++;
                    }
                }
                catch
                {
                    ;
                }
            }
            Console.WriteLine("Total count: {0}", count);
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
