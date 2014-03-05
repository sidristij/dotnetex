using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Runtime.CLR
{
	public static class GCEx
	{
		static readonly int MajorNetVersion;
		static readonly int StringTypeHandler;
		
		static GCEx()
		{
			MajorNetVersion = Environment.Version.Major;
			StringTypeHandler = typeof(string).TypeHandle.Value.ToInt32();
		}
		
		public static unsafe EntityInfo *GetEntityInfo(this object obj)
		{
			return (EntityInfo*)EntityPtr.ToHandler(obj);
		}
		
		/// <summary>
		/// Gets private GC object's fields SyncBlockIndex and EEClass struct pointer
		/// </summary>
		public static unsafe int GetSyncBlockIndex(this object obj)
		{
			var contents = (EntityInfo*)EntityPtr.ToHandler(obj);
			return contents->SyncBlockIndex;
		}
				
		/// <summary>
		/// Sets private GC object's field SyncBlockIndex, which is actually index in private GC table.
		/// </summary>
		/// <param name="obj">Object with SyncBlockIndex to be changed</param>
		/// <param name="syncBlockIndex">New value of SyncBlockIndex</param>
		public static unsafe void SetSyncBlockIndex(this object obj, int syncBlockIndex)
		{
			var contents = (EntityInfo*)(EntityPtr.ToHandler(obj));
			contents->SyncBlockIndex = syncBlockIndex;
		}
					
		/// <summary>
		/// Sets private GC object's field EEClass, which is actually describes current class pointer
		/// </summary>
		/// <param name="obj">Object with SyncBlockIndex to be changed</param>
		/// <param name="methodTable">New value of MathodTable pointer</param>
		public static unsafe void SetMethodTable(this object obj, MethodTableInfo *methodTable)
		{
			var contents = (EntityInfo*)(EntityPtr.ToHandler(obj));
			contents->MethodTable = methodTable;
		}
		
		public static unsafe Int32 SizeOf(this object obj)
		{
			return SizeOf((EntityInfo *)EntityPtr.ToHandler(obj));
		}
		
		/// <summary>
		/// Gets type size in unmanaged memory (directly in SOH/LOH) by type
		/// </summary>
		public static unsafe int SizeOf(EntityInfo *entity)
		{
			if((int)entity->MethodTable == 0)
				throw new ArgumentException("entity have nil in MethodTable (??)");
			
			if((entity->MethodTable->Flags & MethodTableFlags.Array) != 0)
			{
				var arrayinfo = (ArrayInfo *)entity;
				return arrayinfo->SizeOf();
			} 
			else
			if(((int)entity->MethodTable) == StringTypeHandler)
			{
				// TODO: on 4th nedds to be tested
				if(MajorNetVersion >= 4)
				{
					var length = *(int *)((int)entity + 8);
					var str = EntityPtr.ToInstance<string>((int)entity);
					return 4 * ((14 + 2 * length + 3) / 4);
				} 
				else
				{
					 // on 1.0 -> 3.5 string have additional RealLength field
					var length = *(int *)((int)entity + 12);
					var str = EntityPtr.ToInstance<string>((int)entity);
					return 4 * ((16 + 2 * length + 3) / 4);
				}
			} else
			{
				return entity->MethodTable->Size;
			}
		}
	
		/// <summary>
		/// Gets type size in unmanaged memory (directly in SOH/LOH) by type
		/// </summary>
		public static unsafe Int32 SizeOf<T>()
		{
			return SizeOf((EntityInfo *)(typeof(T).TypeHandle.Value.ToInt32() - 4));
		}
		
		/// <summary>
		/// Allocates memory in unmanaged memory area and fills it 
		/// with MethodTable pointer to initialize managed class instance
		/// </summary>
		/// <returns></returns>
		public static T AllocInUnmanaged<T>() where T : new()
		{
			unsafe
			{
				var pointer = Marshal.AllocHGlobal(SizeOf<T>());
				var mTable = (MethodTableInfo *)typeof(T).TypeHandle.Value.ToInt32();
			    var pp = new EntityPtr(pointer);

			    pp.Object.SetMethodTable(mTable);
			
				return (T)pp.Object;
			}
		}
		
		private static unsafe bool TryGetNextInSOH(object current, out object nextObject, out int size)
		{			
			nextObject = null;
			
			try
			{
				var pointer = new EntityPtr { Object = current };
				
				size = SizeOf(pointer.Object);
				pointer.Handler += size;
				
				if(*(int *)(pointer.Handler + 4) == 0)
					return false;
				
				current = pointer.Object;				
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
			int size = 0;
			int cursize = SizeOf(starting);			
			
			do
			{
				size += cursize;
				
				yield return current;
				
				if(current == last)
				{
					yield break;
				}		
				 
			} while(TryGetNextInSOH(current, out current, out cursize));			
		}
	}
}
