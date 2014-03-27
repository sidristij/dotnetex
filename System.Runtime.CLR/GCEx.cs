using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.Runtime.CLR
{
	public static class GCEx
	{
	    public static readonly int MajorNetVersion;
		static readonly IntPtr StringTypeHandle;
        static readonly IntPtr ObjectTypeHandle;
		
		static GCEx()
		{
			MajorNetVersion = Environment.Version.Major;
		    ObjectTypeHandle = typeof (object).TypeHandle.Value;
			StringTypeHandle = typeof (string).TypeHandle.Value;
		}
		
		public static unsafe EntityInfo *GetEntityInfo(object obj)
		{
			return (EntityInfo*)EntityPtr.ToPointer(obj);
		}
		
		/// <summary>
		/// Gets private GC object's fields SyncBlockIndex and EEClass struct pointer
		/// </summary>
		public static unsafe int GetSyncBlockIndex(object obj)
		{
            var contents = (EntityInfo*)EntityPtr.ToPointer(obj);
			return contents->SyncBlockIndex;
		}
				
		/// <summary>
		/// Sets private GC object's field SyncBlockIndex, which is actually index in private GC table.
		/// </summary>
		/// <param name="obj">Object with SyncBlockIndex to be changed</param>
		/// <param name="syncBlockIndex">New value of SyncBlockIndex</param>
		public static unsafe void SetSyncBlockIndex(object obj, int syncBlockIndex)
		{
            var contents = (EntityInfo*)(EntityPtr.ToPointer(obj));
			contents->SyncBlockIndex = syncBlockIndex;
		}
					
		/// <summary>
		/// Sets private GC object's field EEClass, which is actually describes current class pointer
		/// </summary>
		/// <param name="obj">Object with SyncBlockIndex to be changed</param>
		/// <param name="methodTable">New value of MathodTable pointer</param>
		public static unsafe void SetMethodTable(object obj, MethodTableInfo *methodTable)
		{
            var contents = (EntityInfo*)(EntityPtr.ToPointer(obj));
			contents->MethodTable = methodTable;
		}
		
		public static unsafe Int32 SizeOf(this object obj)
		{
            return SizeOf((EntityInfo*)EntityPtr.ToPointer(obj));
		}

	    public static unsafe void SetType<TType>(this object obj)
	    {
	        SetMethodTable(obj, (MethodTableInfo *)typeof (TType).TypeHandle.Value.ToPointer());
	    }
		
		/// <summary>
		/// Gets type size in unmanaged memory (directly in SOH/LOH) by type
		/// </summary>
		public static unsafe int SizeOf(EntityInfo *entity)
		{
		    var flags = (uint) entity->MethodTable->Flags;
		    var flags2 = (uint) entity->MethodTable->AdditionalFlags;

            // if boxed elementary type
            if (flags == (int)(MethodTableFlags.Array |
                               MethodTableFlags.IfArrayThenSzArray | 
                               MethodTableFlags.IfArrayThenSharedByReferenceTypes | 
                               MethodTableFlags.IsMarshalable)) // 0x00270000
		    {
                return entity->MethodTable->Size;
		    }
            else 
            if(((uint)flags & 0xffff0000) == 0x800a0000)
		    {
		         var arrayinfo = (ArrayInfo*)entity;
                    return arrayinfo->SizeOf();
		    }
            else
            // At least we can avoid the touch in this case...
            if ((flags & (int)MethodTableFlags.Array) != 0)
            {
                var arrayinfo = (ArrayInfo*)entity;
                return arrayinfo->SizeOf();
            }
            else if ((entity->MethodTable) == StringTypeHandle.ToPointer())
            {
                // TODO: on 4th nedds to be tested
                if (MajorNetVersion >= 4)
                {
                    var length = *(int*)((int)entity + 8);
                    return 4 * ((14 + 2 * length + 3) / 4);
                }
                else
                {
                    // on 1.0 -> 3.5 string have additional RealLength field
                    var length = *(int*)((int)entity + 12);
                    return 4 * ((16 + 2 * length + 3) / 4);
                }
            }
            else if ( (flags2 & (int)MethodTableFlags2.IsInterface) != 0 || 
                      ((flags & (int)MethodTableFlags.InternalCorElementTypeExtraInfoMask) == (int)MethodTableFlags.InternalCorElementTypeExtraInfo_IfNotArrayThenClass))
            {
                return entity->MethodTable->Size;
            }
            else if ((flags & (int)MethodTableFlags.InternalCorElementTypeExtraInfoMask) == (int)MethodTableFlags.InternalCorElementTypeExtraInfo_IfNotArrayThenValueType)
            {
                return entity->MethodTable->Size;
            }
            else
            {
                return entity->MethodTable->Size;
            }

		    return 0;
		}

		/// <summary>
		/// Gets type size in unmanaged memory (directly in SOH/LOH) by type
		/// </summary>
		public static unsafe Int32 SizeOf<T>()
		{
			return ((MethodTableInfo *)(typeof(T).TypeHandle.Value.ToPointer()))->Size;
		}
		
		/// <summary>
		/// Allocates memory in unmanaged memory area and fills it 
		/// with MethodTable pointer to initialize managed class instance
		/// </summary>
		/// <returns></returns>
		public static T AllocInUnmanaged<T>() where T : new()
		{
			var pointer = Marshal.AllocHGlobal(SizeOf<T>());
			var obj = EntityPtr.ToInstance<T>(pointer);
			obj.SetType<T>();
			return obj;
		}
		
		private static unsafe bool TryGetNextInSOH(object current, out object nextObject, out int size)
		{			
			nextObject = null;
			
			try
			{
			    int offset = (int)EntityPtr.ToPointer(current);
				
				size = SizeOf(current);
			    offset += size;
				
				if(*(int *)(offset + 4) == 0)
					return false;

			    current = EntityPtr.ToInstance<object>((IntPtr) offset);
				nextObject = current;
				return true;
			} catch
			{
				size = 0;
				return false;
			}
		}

		public static IEnumerable<object> GetObjectsInSOH(object starting)
		{
			return GetObjectsInSOH(starting, new object());
		}
		
		public static IEnumerable<object> GetObjectsInSOH(object starting, object last)
		{
			var current = starting;
		    int cursize;			
			
			do
			{
			    yield return current;
				
				if(current == last)
				{
					yield break;
				}		
				 
			} while(TryGetNextInSOH(current, out current, out cursize));			
		}
	}
}
