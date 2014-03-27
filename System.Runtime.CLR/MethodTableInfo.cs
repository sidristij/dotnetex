using System.Runtime.InteropServices;

namespace System.Runtime.CLR
{
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct EntityInfo
	{
		[FieldOffset(0)]
		public int SyncBlockIndex;

		[FieldOffset(4)]
		public MethodTableInfo *MethodTable;				
	}
	
	[StructLayout(LayoutKind.Explicit)]
	public struct RefTypeInfo
	{
		[FieldOffset(0)]
		public EntityInfo BasicInfo;
		
		[FieldOffset(8)]
		public byte fieldsStart;
	}
	
	/// <summary>
	/// Description of GCEnumerator.
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct MethodTableInfo
	{
		#region Basic Type Info
		
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
		public MethodTableInfo *ParentTable;
		
		#endregion
		
		[FieldOffset(20)]
		public ObjectTypeInfo *ModuleInfo;		
		
		[FieldOffset(24)]
		public ObjectTypeInfo *EEClass;
	}
	

	
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct ObjectTypeInfo
	{		
		[FieldOffset(0)]
	    public ObjectTypeInfo *ParentClass;
	    
		[FieldOffset(4)]
	    public int mdTypeDefinition;
	    
		[FieldOffset(8)]
	    public int ClassLoader;
	    
		[FieldOffset(12)]
	    public MethodTableInfo *MethodsTable;
	}

    // ReSharper disable InconsistentNaming
    [Flags]
	public enum MethodTableFlags : uint
	{
		Array                       = 0x00010000,

        InternalCorElementTypeExtraInfoMask                         = 0x00060000,
        InternalCorElementTypeExtraInfo_IfNotArrayThenTruePrimitive = 0x00020000,
        InternalCorElementTypeExtraInfo_IfNotArrayThenClass         = 0x00040000,
        InternalCorElementTypeExtraInfo_IfNotArrayThenValueType     = 0x00060000, 

        IfArrayThenSzArray                                          = 0x00020000,
        IfArrayThenSharedByReferenceTypes                           = 0x00040000,

        ContainsPointers            = 0x00080000,
        HasFinalizer                = 0x00100000, // instances require finalization

        IsMarshalable               = 0x00200000, // Is this type marshalable by the pinvoke marshalling layer

        HasRemotingVtsInfo          = 0x00400000, // Optional data present indicating VTS methods and optional fields
        IsFreezingRequired          = 0x00800000, // Static data should be frozen after .cctor

        TransparentProxy            = 0x01000000, // tranparent proxy
        CompiledDomainNeutral       = 0x02000000, // Class was compiled in a domain neutral assembly

        // This one indicates that the fields of the valuetype are 
        // not tightly packed and is used to check whether we can
        // do bit-equality on value types to implement ValueType::Equals.
        // It is not valid for classes, and only matters if ContainsPointer
        // is false.
        //
        NotTightlyPacked            = 0x04000000, 

        HasCriticalFinalizer        = 0x08000000, // finalizer must be run on Appdomain Unload
        UNUSED                      = 0x10000000,
        ThreadContextStatic         = 0x20000000, 

        IsFreezingCompleted         = 0x80000000, // Static data has been frozen

        NonTrivialInterfaceCast     = Array | TransparentProxy,
	}

    [Flags]
    public enum MethodTableFlags2 : uint
    {
        MarshaledByRef = 0x0001, // Class is marshaled by ref (needs a remoting stub)
        NoSecurityProperties = 0x0002, // Class does not have security properties (that is,
        // GetClass()->GetSecurityProperties will return 0).
        HasGenericsStaticsInfo = 0x0004,

        MayNeedRestore = 0x0008, // Class may need restore
        // This flag is set only for NGENed classes

        UNUSED = 0x0010,
        IsZapped = 0x0020, // This could be fetched from m_pLoaderModule if we run out of flags

        IsDynamicStatics = 0x0040, // Static data will be stored in the dynamic table of the module
        FixedAddressVTStatics = 0x0080, // Value type Statics in this class will be pinned

        // These two flags only use three out of four possible combinations.  We should
        // use the other combination to store some useful information about generics.
        GenericsMask = 0x0300,
        GenericsMask_NonGeneric = 0x0000, // no instantiation
        GenericsMask_CanonInst = 0x0100, // an unshared or canonical-shared instantiation,
        
        // apart from the typical inst. e.g. List<int> or List<__Canon>
        GenericsMask_NonCanonInst = 0x0200, // a non-canonical-shared instantiation, e.g. List<string>
        GenericsMask_TypicalInst = 0x0300, // the type instantiated at its formal parameters, e.g. List<T>

        ClassPreInited = 0x0400, // If this flag is set, we dont need to do any class initialization logic at all
        IsAsyncPin = 0x0800,
        ContainsGenericVariables = 0x1000, // we cache this flag to help detect these efficiently and
        
        // to detect this condition when restoring
        IsInterface = 0x2000, // This MT is an interface
        HasDispatchMap = 0x4000, // TRUE:  m_pDispatchMap is valid
        
        // FALSE: m_pImplTable is valid
        HasVariance = 0x8000, // This is an instantiated type some of whose type parameters are co or contra-variant
    }

    // ReSharper restore InconsistentNaming
    
    /*
    public int CCtorSlot;
	    
    public int DefaultCtorSlot;
	    
    public byte NormType;
	    
    public ushort NumInstanceFields; 

    public ushort NumStaticFields; 

    public ushort NumHandleStatics;

    public ushort NumBoxedStatics; 

    public ushort NumGCPointerSeries; 
    /*
     * DWORD m_cbModuleDynamicID; 

DWORD m_cbNonGCStaticFieldBytes; 
DWORD m_dwNumInstanceFieldBytes; 
FieldDesc *m_pFieldDescList; 
DWORD m_dwAttrClass; 
volatile DWORD m_VMFlags; 
SecurityProperties m_SecProps; 
PTR_MethodDescChunk m_pChunks; 
BitMask m_classDependencies; 
DWORD m_dwReliabilityContract;
     */	
}