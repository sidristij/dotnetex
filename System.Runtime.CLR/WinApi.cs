namespace System.Runtime.CLR
{
    using InteropServices;
    using Security;

    public static class WinApi
    {
        private const String Kernel32 = "kernel32.dll";

        // ReSharper disable All

        [DllImport(Kernel32, SetLastError = true)]
        public static extern unsafe bool IsBadReadPtr(IntPtr address, uint ucb);

        [DllImport("msvcrt.dll", SetLastError = false)]
        public static extern IntPtr memcpy(IntPtr dest, IntPtr src, int count);


        [DllImport(Kernel32, SetLastError = true)]
        public unsafe static extern IntPtr VirtualQuery(
            IntPtr address,
            ref MEMORY_BASIC_INFORMATION buffer,
            IntPtr sizeOfBuffer
        );

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct MEMORY_BASIC_INFORMATION
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
        public struct SYSTEM_INFO
        {
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
        public static extern void GetSystemInfo(ref SYSTEM_INFO lpSystemInfo);

        // ReSharper restore All
    }
}
