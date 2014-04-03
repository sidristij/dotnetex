using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Runtime.CLR
{
	public static class GCEx
	{
	    public static readonly int MajorNetVersion;
		static readonly IntPtr StringTypeHandle;
		
		static GCEx()
		{
			MajorNetVersion = Environment.Version.Major;
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
            var x = SizeOf((EntityInfo*)EntityPtr.ToPointer(obj));
		    return x;
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
            if (flags == 0x00270000)
		    {
                return entity->MethodTable->Size;
		    }

            // Array
            if((flags & 0xffff0000) == 0x800a0000)   
		    {
		         var arrayinfo = (ArrayInfo*)entity;
                    return arrayinfo->SizeOf();
            }

            // ???
            if ((flags & 0xffff0000) == 0x01400200)  
            {
                return entity->MethodTable->Size;
            }

            // COM interface: have no size and have no .Net class
            if ((flags & 0xffff0000) == 0x000c0000)
            {
                return SizeOf<object>();
            }
            
            // String
            if ((entity->MethodTable) == StringTypeHandle.ToPointer())
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
            

            return entity->MethodTable->Size;
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

        private static unsafe bool TryGetNextInSOH(object current, Predicate<long> checker, out object nextObject)
		{			
			nextObject = null;
			
			try
			{
			    var offset = (int)EntityPtr.ToPointer(current);
				var size = SizeOf(current);
			    offset += size;

			    var mt = (long)*(IntPtr*) (offset + IntPtr.Size);

                if(!checker(mt))
			        return false;

                //if ((long)*(IntPtr*)(offset + IntPtr.Size) == 0) return false;

			    current = EntityPtr.ToInstance<object>((IntPtr) offset);
				nextObject = current;
				return true;
			} catch
			{
				return false;
			}
		}

	    public class SohEnumeratorItem
	    {
	        public object Item;
	        public bool IsArrayItem;
	    }

        public static IEnumerable<SohEnumeratorItem> GetObjectsInSOH(object starting, Predicate<long> checker)
		{
			return GetObjectsInSOH(starting, new object(), checker);
		}

        public static IEnumerable<SohEnumeratorItem> GetObjectsInSOH(object starting, object last, Predicate<long> checker)
		{
			var current = starting;
            var enumItem = new SohEnumeratorItem();

            while (TryGetNextInSOH(current, checker, out current))
            {
                enumItem.Item = current;
			    yield return enumItem;
                
			    var @array = current as Array;
			    if (@array != null && !@array.GetType().GetElementType().IsValueType)
			    {
			        enumItem.IsArrayItem = true;
			        foreach (var item in @array)
			        {
			            enumItem.Item = item;
			            if (item != null)
			            {
			                yield return enumItem;
			            }
			        }
			        enumItem.IsArrayItem = false;
			    }
                
				if(current == last)
				{
					yield break;
				}		
			}		
		}

        public static bool IsAchievableFrom(object from, object to, Predicate<long> checker)
	    {
            var current = from;
            int cursize;

            do
            {
                if (current == to)
                {
                    return true;
                }

            } while (TryGetNextInSOH(current, checker, out current));

	        return false;
	    }
	}
}
