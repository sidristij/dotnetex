using System;
using System.Runtime.InteropServices;

namespace readingStructs
{
    [StructLayout(LayoutKind.Explicit)]
    public class EntityInfo
    {
        [FieldOffset(0)] public IntPtr MtPointer;
    }

    [StructLayout(LayoutKind.Explicit)]
    public class EntityInfoPtr
    {
        public class RefPtr
        {
            public EntityInfo mt;
        }

        public class PIntPtr
        {
            public IntPtr mt;
        }

        [FieldOffset(0)]
        public RefPtr Reference;
        [FieldOffset(0)]
        public PIntPtr Pointer;

        public EntityInfoPtr(EntityInfo methodTable)
        {
            Reference = new RefPtr { mt = methodTable };
        }

        public EntityInfoPtr(IntPtr methodTable)
        {
            Pointer = new PIntPtr { mt = methodTable };
        }
    }

}
