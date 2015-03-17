namespace _04_sharedMemoryLib
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CLR;
    using System.Runtime.InteropServices;

    public class FixedAddressTypesMap
    {
        private readonly IntPtr ptr;
        private IntPtr lastPtr;
        private Dictionary<Type, IntPtr> addresses = new Dictionary<Type, IntPtr>();

        public FixedAddressTypesMap(uint address)
        {
            ptr = WinApi.VirtualAlloc(new IntPtr(address), new IntPtr(2048), 0x1000, 0x40);
            lastPtr = ptr;
        }

        public unsafe SharedTypeHolder<TType> GetOrAddType<TType>()
        {
            if (addresses.ContainsKey(typeof (TType)))
            {
                return new SharedTypeHolder<TType>(addresses[typeof (TType)]);
            }

            var mt = (MethodTableInfo *)(typeof (TType).TypeHandle.Value);
            var structsize = Marshal.SizeOf(typeof (MethodTableInfo));
            var size = structsize;
            var vmt_size = 4 * mt->VirtMethodsCount;
            size += vmt_size;

            var newmt = lastPtr;
            WinApi.memcpy(newmt, (IntPtr)mt, size);
            lastPtr = (IntPtr)((int)lastPtr + structsize);
            var newvmt_addr = (int *)lastPtr;
            lastPtr = (IntPtr)((int)lastPtr + vmt_size);

            var proxiesFrom = (int *)((byte *)mt + structsize);
            var proxiesTo = (ProxyStruct *)lastPtr;

            int index = 0;
            for (index = 0; index < mt->VirtMethodsCount; index++)
            {
                var slot = new ProxyStruct(proxiesFrom[index] - ((int)&proxiesTo[index] + 5));
                proxiesTo[index] = slot;
                newvmt_addr[index] = (int) &proxiesTo[index] ;
            }

            lastPtr = (IntPtr)((int)lastPtr + mt->VirtMethodsCount * 5);
            addresses.Add(typeof(TType), newmt);

            ((MethodTableInfo*) newmt)->ParentTable = mt; // support inheristance

            return new SharedTypeHolder<TType>(newmt);
        }

        [StructLayout(LayoutKind.Explicit)]
        struct ProxyStruct
        {
            public ProxyStruct(int addr)
            {
                opcode = 0xe9;
                this.addr = addr;
                debug1 = 0xcc;
                debug2 = 0xcccc;
            }

            [FieldOffset(0)]
            public byte opcode; // jmp near

            [FieldOffset(1)]
            public int addr;

            [FieldOffset(5)] public byte debug1;

            [FieldOffset(6)] public ushort debug2;
        }
    }

    public class SharedTypeHolder<T>
    {
        private readonly IntPtr methodsTable;

        public SharedTypeHolder(IntPtr methodsTable)
        {
            this.methodsTable = methodsTable;
        }

        public unsafe T AsSharedType(T obj)
        {
            *(int*)EntityPtr.ToPointerWithOffset(obj) = (int)methodsTable; // rewrite mt to new
            return obj;
        }
    }
}
