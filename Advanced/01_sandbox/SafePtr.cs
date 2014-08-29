namespace SafePtrSample
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    public struct SafePtr
    {
        /// <summary>
        /// Type for reference taking
        /// </summary>
        public class ReferenceType
        {
            public object Reference;
        }
        
        /// <summary>
        /// Type for address taking
        /// </summary>
        public class IntPtrWrapper
        {
            public IntPtr IntPtr;
        }

        [FieldOffset(0)]
        private ReferenceType Obj;

        [FieldOffset(0)]
        private IntPtrWrapper Pointer;

        /// <summary>
        /// Creates object addressable by wrapping it
        /// </summary>
        /// <param name="obj">object reference to get its address</param>
        /// <returns></returns>
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