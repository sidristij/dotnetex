using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

namespace CLREx
{
	public static class GCEx
	{
		[StructLayout(LayoutKind.Explicit)]
		public struct EntityPtr
		{
			[FieldOffset(0)]
			internal object Object;
			
			[FieldOffset(4)]
			private int _pointer;
			
			internal unsafe int Pointer
			{
				get {
					fixed(int *pp = &_pointer)
					{
						return *(int *)((int)pp - sizeof(int)) - sizeof(int);
					}
				}
				
				set {
					fixed(int *pp = &_pointer)
					{
						*(int *)((int)pp - sizeof(int)) = value + sizeof(int);
					}
				}
			}
		}
	
		public static unsafe EntityInfo *GetGCFields(this object obj)
		{
			return (EntityInfo*)((new EntityPtr { Object = obj }).Pointer);
		}
		
		/// <summary>
		/// Gets private GC object's fields SyncBlockIndex and EEClass struct pointer
		/// </summary>
		public static unsafe void GetGCFields(this object obj, out int syncBlockIndex, out MethodTableInfo *methodtable)
		{
			var contents = (EntityInfo*)((new EntityPtr { Object = obj }).Pointer);
			syncBlockIndex = contents->SyncBlockIndex;
			methodtable = contents->MethodTable;
		}
		
					
		/// <summary>
		/// Gets private GC object's fields SyncBlockIndex and EEClass struct pointer
		/// </summary>
		public static unsafe int GetSyncBlockIndex(this object obj)
		{
			var contents = (EntityInfo*)((new EntityPtr { Object = obj }).Pointer);
			return contents->SyncBlockIndex;
		}
					
		/// <summary>
		/// Gets private GC object's fields SyncBlockIndex and EEClass struct pointer
		/// </summary>
		public static unsafe EntityInfo *GetEntityInfo(this object obj)
		{
			return (EntityInfo*)((new EntityPtr { Object = obj }).Pointer);			
		}
		
		/// <summary>
		/// Sets private GC object's field SyncBlockIndex, which is actually index in private GC table.
		/// </summary>
		/// <param name="obj">Object with SyncBlockIndex to be changed</param>
		/// <param name="syncBlockIndex">New value of SyncBlockIndex</param>
		public static unsafe void SetSyncBlockIndex(this object obj, int syncBlockIndex)
		{
			var contents = (EntityInfo*)((new EntityPtr { Object = obj }).Pointer);
			contents->SyncBlockIndex = syncBlockIndex;
		}
					
		/// <summary>
		/// Sets private GC object's field EEClass, which is actually describes current class pointer
		/// </summary>
		/// <param name="obj">Object with SyncBlockIndex to be changed</param>
		/// <param name="syncBlockIndex">New value of SyncBlockIndex</param>
		public static unsafe void SetMethodTable(this object obj, MethodTableInfo *methodTable)
		{
			var contents = (EntityInfo*)((new EntityPtr { Object = obj }).Pointer);
			contents->MethodTable = methodTable;
		}
		
		public static unsafe Int32 SizeOf(this object obj)
		{
			var pointer = new EntityPtr { Object = obj };
			return SizeOf((EntityInfo *)(pointer.Pointer));
		}
		
		/// <summary>
		/// Gets type size in unmanaged memory (directly in SOH/LOH) by type
		/// </summary>
		public static unsafe int SizeOf(EntityInfo *entity)
		{
			if((entity->MethodTable->Flags & MethodTableFlags.Array) != 0)
			{
				var arrayinfo = (ArrayInfo *)entity;
				return arrayinfo->SizeOf();
			} 
			else 
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
				IntPtr pointer = Marshal.AllocHGlobal(SizeOf<T>());
				var mTable = (MethodTableInfo *)typeof(T).TypeHandle.Value.ToInt32();
				var pp = new EntityPtr();
							
				pp.Pointer = pointer.ToInt32();
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
				var entity =  current.GetEntityInfo();				
				var methodTable = entity->MethodTable;
				
				size = SizeOf(pointer.Object);
				pointer.Pointer += size;
				
				current = pointer.Object;				
				nextObject = current;
				return true;
			} catch (COMException ex)
			{
				size = 0;
				return false;
			}
			return false;
		}

		public static IEnumerable GetObjectsInSOH(object starting)
		{
			return GetObjectsInSOH(starting, new object());
		}
		
		public static IEnumerable GetObjectsInSOH(object starting, object last)
		{
			var current = starting;
			var pointer = new EntityPtr { Object = current };
			int size = 0;
			int cursize = GCEx.SizeOf(starting);			
			
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
