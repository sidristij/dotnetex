using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Runtime.CLR
{
	/// <summary>
	/// Description of UnmanagedHeap.
	/// </summary>
	public unsafe class UnmanagedHeap<T> //where T:new()
	{
		private readonly Queue<WeakReference> _freeObjects;
		private readonly List<WeakReference> _allObjects;
		private readonly int _totalSize;
		
		public unsafe UnmanagedHeap(int capacity)
		{
		    _freeObjects = new Queue<WeakReference>(capacity);
			_allObjects = new List<WeakReference>(capacity);
			
			var objectSize = GCEx.SizeOf<T>();
			_totalSize = objectSize * capacity;
			
			var startingPointer = Marshal.AllocHGlobal(_totalSize).ToInt32();
			var mTable = (MethodTableInfo *)typeof(T).TypeHandle.Value.ToInt32();
			
			var ptr = new EntityPtr();			
			// var ctor = typeof(T).GetConstructor(Type.EmptyTypes);
			
			for(int i = 0; i < capacity; i++)
			{
				ptr.Handler =  startingPointer + (objectSize * i);
				ptr.Object.SetMethodTable(mTable);
				var reference = new WeakReference(ptr.Object);
				_freeObjects.Enqueue(reference);
				_allObjects.Add(reference);
			}
		}
		
		public int TotalSize
		{
			get 
            {
				return _totalSize;
			}
		}
		
		public T Allocate()
		{			
			var obj = _freeObjects.Dequeue().Target;
			return (T)obj;
		}
		
		public void Free(T obj)
		{
			_freeObjects.Enqueue(new WeakReference(obj));			
		}	
	}
}
