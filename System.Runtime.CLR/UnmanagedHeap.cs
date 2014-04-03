using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Runtime.CLR
{
	public delegate void CtorDelegate(IntPtr obj);
	
	internal static class Stub
	{
		public static void Construct(object obj, int value)
		{
		}	
	}
	
	public class UnmanagedObject<T> : IDisposable where T : UnmanagedObject<T>
	{	
		internal IUnmanagedHeap<T> heap;
		
		#region IDisposable implementation
		void IDisposable.Dispose()
		{
			heap.Free(this);
		}
		#endregion
	}
	
	
	public interface IUnmanagedHeapBase  : IDisposable
	{
		int TotalSize { get; }		
		object Allocate();
		void Free(object obj);
		void Reset();
	}
	
	public interface IUnmanagedHeap<TPoolItem> : IUnmanagedHeapBase where TPoolItem : UnmanagedObject<TPoolItem>
	{
		int TotalSize { get; }		
		TPoolItem Allocate();
		void Free(TPoolItem obj);
		void Reset();
	}
	
	/// <summary>
	/// Description of UnmanagedHeap.
	/// </summary>
	public unsafe class UnmanagedHeap<TPoolItem> : IUnmanagedHeap<TPoolItem> where TPoolItem : UnmanagedObject<TPoolItem>
	{
        private readonly IntPtr *_freeObjects;
		private readonly IntPtr *_allObjects;
		private readonly int _totalSize, _capacity;
		private int _freeSize;
	    private readonly void *_startingPointer;
		private readonly ConstructorInfo _ctor;
		
		public unsafe UnmanagedHeap(int capacity)
		{
			_freeSize = capacity;
			
            // Getting type size and total pool size
			var objectSize = GCEx.SizeOf<TPoolItem>();
		    _capacity = capacity;
			_totalSize = objectSize * capacity + capacity * IntPtr.Size * 2;
			
			_startingPointer = Marshal.AllocHGlobal(_totalSize).ToPointer();
            var mTable = (MethodTableInfo*)typeof(TPoolItem).TypeHandle.Value.ToInt32();
            _freeObjects = (IntPtr*)_startingPointer;
            _allObjects = (IntPtr*)((long)_startingPointer + IntPtr.Size * capacity);
            _startingPointer = (void*)((long)_startingPointer + 2 * IntPtr.Size * capacity); 
			
			var pFake = typeof(Stub).GetMethod("Construct", BindingFlags.Static|BindingFlags.Public);
			var pCtor = _ctor = typeof(TPoolItem).GetConstructor(new []{typeof(int)});
		
			MethodUtil.ReplaceMethod(pCtor, pFake, skip: true);
			
			for(int i = 0; i < capacity; i++)
			{
				var handler =  (IntPtr *)((long)_startingPointer + (objectSize * i));
			    handler[1] = (IntPtr)mTable;
			    var obj = EntityPtr.ToInstance<object>((IntPtr)handler);
               
				var reference = (TPoolItem)obj;
				reference.heap = this;

                _allObjects[i] = (IntPtr)(handler + 1);
			}			

            Reset();
		}
		
		public int TotalSize
		{
			get {
				return _totalSize;
			}
		}
				
		public TPoolItem Allocate()
		{
			_freeSize--;
			var obj = _freeObjects[_freeSize];
			Stub.Construct(obj, 123);			
			return EntityPtr.ToInstanceWithOffset<TPoolItem>(obj);
		}
		
		public TPoolItem AllocatePure()
		{
            _freeSize--;
            var obj = EntityPtr.ToInstanceWithOffset<TPoolItem>(_freeObjects[_freeSize]);
			_ctor.Invoke(obj, new object[]{123});			
			return obj;
		}
		
		public void Free(TPoolItem obj)
		{
			_freeObjects[_freeSize] = EntityPtr.ToPointerWithOffset(obj);
			_freeSize++;
		}	
		
		public void Reset()
		{
            UnmanagedPoolWinApiHelper.memcpy((IntPtr)_freeObjects, (IntPtr)_allObjects, _capacity * IntPtr.Size);
			_freeSize = _capacity;
		}

		object IUnmanagedHeapBase.Allocate()
		{
			return this.Allocate();
		}
		
		void IUnmanagedHeapBase.Free(object obj)
		{
			this.Free((TPoolItem)obj);
		}

        public void Dispose()
        {
            Marshal.FreeHGlobal((IntPtr)_startingPointer);
        }
    }

    internal static class UnmanagedPoolWinApiHelper
    {
        [DllImport("msvcrt.dll", SetLastError = false)]
        public static extern IntPtr memcpy(IntPtr dest, IntPtr src, int count);
    }
}
