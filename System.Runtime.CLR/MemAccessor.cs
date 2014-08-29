namespace System.Runtime.CLR
{
    using InteropServices;

    public static class MemAccessor
    {
        /// <summary>
        /// Gets managed heap address (only last ephemeral segment)
        /// </summary>
        public static void GetManagedHeap(out IntPtr heapsOffset, out IntPtr lastHeapByte)
        {
            var offset = EntityPtr.ToPointer(new object());

            var memoryBasicInformation = new WinApi.MEMORY_BASIC_INFORMATION();

            unsafe
            {
                WinApi.VirtualQuery(offset, ref memoryBasicInformation, (IntPtr)Marshal.SizeOf(memoryBasicInformation));
                heapsOffset = (IntPtr)memoryBasicInformation.AllocationBase;
                lastHeapByte = (IntPtr)((long)offset + (long)memoryBasicInformation.RegionSize);
            }
        }
    }
}
