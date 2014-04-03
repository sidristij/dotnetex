using System;
using System.Runtime.InteropServices;

namespace sandbox
{
    [StructLayout(LayoutKind.Explicit)]
    public struct SafePtr
    {
        public class ReferenceType
        {
            public object Reference;
        }
        
        public class IntPtrWrapper
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
            get { return Pointer.IntPtr; }
            set { Pointer.IntPtr = value; }
        }

        public Object Object
        {
            get { return Obj.Reference; }
            set { Obj.Reference = value; }
        }

        public void SetPointer(SafePtr another)
        {
            Pointer.IntPtr = another.Pointer.IntPtr;
        }
    }
}