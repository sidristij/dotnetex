/*
 * Created by SharpDevelop.
 * User: SSidristy
 * Date: 03.03.2014
 * Time: 15:57
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Runtime.CLR
{
	/// <summary>
	/// Description of UnmanagedHeap.
	/// </summary>
	public unsafe class UnmanagedHeap<T> //where T:new()
	{
		private Queue<WeakReference> freeObjects;
		private List<WeakReference> allObjects;
		private ConstructorInfo ctor;
		private int totalSize;
		
		public unsafe UnmanagedHeap(int capacity)
		{
			freeObjects = new Queue<WeakReference>(capacity);
			allObjects = new List<WeakReference>(capacity);
			
			int objectSize = GCEx.SizeOf<T>();
			totalSize = objectSize * capacity;
			
			var startingPointer = Marshal.AllocHGlobal(totalSize).ToInt32();
			var mTable = (MethodTableInfo *)typeof(T).TypeHandle.Value.ToInt32();
			
			var ptr = new EntityPtr();			
			var ctor = typeof(T).GetConstructor(Type.EmptyTypes);
			
			for(int i = 0; i < capacity; i++)
			{
				ptr.Handler =  startingPointer + (objectSize * i);
				ptr.Object.SetMethodTable(mTable);
				var reference = new WeakReference(ptr.Object);
				freeObjects.Enqueue(reference);
				allObjects.Add(reference);
			}
		}
		
		public int TotalSize
		{
			get {
				return totalSize;
			}
		}
		
		public T Allocate()
		{			
			var obj = freeObjects.Dequeue().Target;
			if(ctor != null)
			{
				;//ctor.Invoke(obj, new object[0]);
			}
			return (T)obj;
		}
		
		public void Free(T obj)
		{
			freeObjects.Enqueue(new WeakReference(obj));			
		}	
	}
}
