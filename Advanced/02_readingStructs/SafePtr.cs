using System;
using System.Runtime.InteropServices;

namespace sandbox
{
    /// <summary>
    /// Allows to take native pointer from managed object reference and to change it
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct SafePtr
    {
        private class ReferenceType
        {
            public object Reference;
        }

        private class IntPtrWrapper
        {
            public IntPtr IntPtr;
        }

        [FieldOffset(0)]
        private ReferenceType Obj;

        [FieldOffset(0)]
        private IntPtrWrapper Pointer;

        public static SafePtr Create(object obj)
        {
            return new SafePtr { Obj = new ReferenceType { Reference = obj } };
        }

        public static SafePtr Create(IntPtr rIntPtr)
        {
            return new SafePtr { Pointer = new IntPtrWrapper { IntPtr = rIntPtr } };
        }

        public IntPtr IntPtr
        {
            get { return Pointer.IntPtr;  }
            set { Pointer.IntPtr = value; }
        }

        public object Object
        {
            get { return Obj.Reference;  }
            set { Obj.Reference = value; }
        }

        public void SetPointer(SafePtr another)
        {
            Pointer.IntPtr = another.Pointer.IntPtr;
        }
    }
}