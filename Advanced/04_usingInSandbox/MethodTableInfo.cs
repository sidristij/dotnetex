using System;
using System.Runtime.InteropServices;

namespace readingStructs
{
    /// <summary>
    /// Description of GCEnumerator.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public class MethodTableInfo
    {
        [FieldOffset(0)]
        public MethodTableFlags Flags;

        [FieldOffset(4)]
        public int Size;

        [FieldOffset(8)]
        public short AdditionalFlags;

        [FieldOffset(10)]
        public short MethodsCount;

        [FieldOffset(12)]
        public short VirtMethodsCount;

        [FieldOffset(14)]
        public short InterfacesCount;

        [FieldOffset(16)]
        public IntPtr /*MethodTableInfo* */ ParentTable;

        [FieldOffset(20)]
        public IntPtr/* ObjectTypeInfo* */  EEClass;

        [FieldOffset(24)]
        public IntPtr /*ObjectTypeInfo* */  ModuleInfo;
    }

    [StructLayout(LayoutKind.Explicit)]
    public class MethodTablePtr
    {
        public class RefPtr {
            public MethodTableInfo mt;
        }

        public class PIntPtr
        {
            public IntPtr mt;
        }

        [FieldOffset(0)] public RefPtr  Reference;
        [FieldOffset(0)] public PIntPtr Pointer;

        public MethodTablePtr(MethodTableInfo methodTable)
        {
            Reference = new RefPtr { mt = methodTable };
        }

        public MethodTablePtr(IntPtr methodTable)
        {
            Pointer = new PIntPtr { mt = methodTable };
        }
    }

    // ReSharper disable InconsistentNaming
    [Flags]
    public enum MethodTableFlags : uint
    {
        Array = 0x00010000,

        InternalCorElementTypeExtraInfoMask = 0x00060000,
        InternalCorElementTypeExtraInfo_IfNotArrayThenTruePrimitive = 0x00020000,
        InternalCorElementTypeExtraInfo_IfNotArrayThenClass = 0x00040000,
        InternalCorElementTypeExtraInfo_IfNotArrayThenValueType = 0x00060000,
        IfArrayThenSzArray = 0x00020000,
        IfArrayThenSharedByReferenceTypes = 0x00040000,

        ContainsPointers = 0x00080000,
        HasFinalizer = 0x00100000, // instances require finalization

        IsMarshalable = 0x00200000, // Is this type marshalable by the pinvoke marshalling layer

        HasRemotingVtsInfo = 0x00400000, // Optional data present indicating VTS methods and optional fields
        IsFreezingRequired = 0x00800000, // Static data should be frozen after .cctor

        TransparentProxy = 0x01000000, // tranparent proxy
        CompiledDomainNeutral = 0x02000000, // Class was compiled in a domain neutral assembly

        // This one indicates that the fields of the valuetype are 
        // not tightly packed and is used to check whether we can
        // do bit-equality on value types to implement ValueType::Equals.
        // It is not valid for classes, and only matters if ContainsPointer
        // is false.
        //
        NotTightlyPacked = 0x04000000,

        HasCriticalFinalizer = 0x08000000, // finalizer must be run on Appdomain Unload
        UNUSED = 0x10000000,
        ThreadContextStatic = 0x20000000,

        IsFreezingCompleted = 0x80000000, // Static data has been frozen

        NonTrivialInterfaceCast = Array |
                                                TransparentProxy,
    }
    // ReSharper restore InconsistentNaming
}
