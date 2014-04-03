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
        private class RefPtr
        {
            public object Reference;
        }

        private class PIntPtr
        {
            public IntPtr IntPtr;
        }

        [FieldOffset(0)] private RefPtr Obj;

        [FieldOffset(0)] private PIntPtr Pointer;

        public static SafePtr Create(object obj)
        {
            return new SafePtr { Obj = new RefPtr { Reference = obj } };
        }

        public static SafePtr Create(IntPtr rIntPtr)
        {
            return new SafePtr { Pointer = new PIntPtr { IntPtr = rIntPtr } };
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