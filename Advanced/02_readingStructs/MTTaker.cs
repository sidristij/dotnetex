using System;
using System.Runtime.InteropServices;

namespace readingStructs
{
    /// <summary>
    /// Getting and setting pointer to class
    /// </summary>
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
            public EntityInfo Value;
        }

        public class PIntPtr
        {
            public IntPtr Value;
        }

        
        [FieldOffset(0)] public RefPtr Reference;
        [FieldOffset(0)] public PIntPtr Pointer;

        public EntityInfoPtr(EntityInfo methodTable)
        {
            Reference = new RefPtr { Value = methodTable };
        }

        public EntityInfoPtr(IntPtr methodTable)
        {
            Pointer = new PIntPtr { Value = methodTable };
        }
    }

}
