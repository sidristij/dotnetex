using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Runtime.CLR
{
	public delegate void CtorDelegate(IntPtr obj);
	
	internal static class Stub
	{
		public static void Construct(object obj, int value)
		{
			Console.WriteLine("Construct1");
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
	
	
	public interface IUnmanagedHeapBase
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
		private readonly TPoolItem[] _freeObjects;
		private readonly TPoolItem[] _allObjects;
		private readonly int _totalSize;
		private int _freeSize;
		private readonly ConstructorInfo _ctor;
		
		public unsafe UnmanagedHeap(int capacity)
		{                                
			_allObjects = new TPoolItem[capacity];
			_freeSize = capacity;
			
			var objectSize = 100; //GCEx.SizeOf<T>();
			_totalSize = objectSize * capacity;
			
			var startingPointer = Marshal.AllocHGlobal(_totalSize).ToInt32();
			var mTable = (MethodTableInfo *)typeof(TPoolItem).TypeHandle.Value.ToInt32();
			
			var ptr = new EntityPtr();
			var pFake = typeof(Stub).GetMethod("Construct", BindingFlags.Static|BindingFlags.Public);
			var pCtor = _ctor = typeof(TPoolItem).GetConstructor(new []{typeof(int)});
		
			MethodUtil.ReplaceMethod(pCtor, pFake, skip: true);
			
			for(int i = 0; i < capacity; i++)
			{
				ptr.Handler =  startingPointer + (objectSize * i);
				ptr.Object.SetMethodTable(mTable);
				
				var reference = (TPoolItem)ptr.Object;
				reference.heap = this;
				
				_allObjects[i] = reference;
			}			
			
			_freeObjects = (TPoolItem[])_allObjects.Clone();
			
			// compile methods
			this.Free(this.Allocate());
			this.Free(this.AllocatePure());
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
			return obj;
		}
		
		public TPoolItem AllocatePure()
		{
            _freeSize--;
			var obj = _freeObjects[_freeSize]; 
			_ctor.Invoke(obj, new object[]{123});			
			return obj;
		}
		
		public void Free(TPoolItem obj)
		{
			_freeObjects[_freeSize] = obj;
			_freeSize++;
		}	
		
		public void Reset()
		{
			_allObjects.CopyTo(_freeObjects, 0);
			_freeSize = _freeObjects.Length;
		}

		object IUnmanagedHeapBase.Allocate()
		{
			return this.Allocate();
		}
		
		void IUnmanagedHeapBase.Free(object obj)
		{
			this.Free((TPoolItem)obj);
		}
	}
}
