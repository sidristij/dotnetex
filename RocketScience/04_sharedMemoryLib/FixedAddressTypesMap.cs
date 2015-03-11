namespace _04_sharedMemoryLib
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CLR;
    using System.Runtime.InteropServices;

    public class FixedAddressTypesMap
    {
        private IntPtr ptr, lastPtr;
        private Dictionary<Type, IntPtr> addresses = new Dictionary<Type, IntPtr>();

        public FixedAddressTypesMap(uint address)
        {
            ptr = WinApi.VirtualAlloc(new IntPtr(address), new IntPtr(2048), 0x1000, 0x40);
            lastPtr = ptr;
        }

        public unsafe IntPtr AddType<TType>()
        {
            var mt = (MethodTableInfo *)(typeof (TType).TypeHandle.Value);
            var structsize = Marshal.SizeOf(typeof (MethodTableInfo));
            var size = structsize;
            var vmt_size = 4 * mt->VirtMethodsCount;
            size += vmt_size;

            var newmt = lastPtr;
            WinApi.memcpy(newmt, (IntPtr)mt, size);
            lastPtr += structsize;
            var newvmt_addr = (int *)lastPtr;
            lastPtr += vmt_size;

            var proxiesFrom = (int *)((byte *)mt + structsize);
            var proxiesTo = (ProxyStruct *)lastPtr;

            int index = 0;
            for (index = 0; index < mt->VirtMethodsCount; index++)
            {
                var slot = new ProxyStruct(proxiesFrom[index] - ((int)&proxiesTo[index] + 5));
                proxiesTo[index] = slot;
                newvmt_addr[index] = (int) &proxiesTo[index] ;
            }

            lastPtr += mt->VirtMethodsCount * 5;
            addresses.Add(typeof(TType), newmt);

            return newmt;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct ProxyStruct
        {
            public ProxyStruct(int addr)
            {
                opcode = 0xe9;
                this.addr = addr;
            }

            [FieldOffset(0)]
            public byte opcode; // jmp near

            [FieldOffset(1)]
            public int addr;
        }
    }
}
