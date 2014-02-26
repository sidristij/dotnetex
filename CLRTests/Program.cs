using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace ConsoleTest
{
	public static class GCEx
	{
		public static unsafe ObjContents *GetGCFields(this object obj)
		{
			return (ObjContents*)((new ObjPointer { Object = obj }).Pointer);
		}
		
		/// <summary>
		/// Gets private GC object's fields SyncBlockIndex and EEClass struct pointer
		/// </summary>
		public static unsafe void GetGCFields(this object obj, out UInt32 syncBlockIndex, out UInt32 methodtable)
		{
			var contents = (ObjContents*)((new ObjPointer { Object = obj }).Pointer);
			syncBlockIndex = contents->syncBlockIndex;
			methodtable = contents->methodTable;
		}
		
					
		/// <summary>
		/// Gets private GC object's fields SyncBlockIndex and EEClass struct pointer
		/// </summary>
		public static unsafe UInt32 GetSyncBlockIndex(this object obj)
		{
			var contents = (ObjContents*)((new ObjPointer { Object = obj }).Pointer);
			return contents->syncBlockIndex;
		}
					
		/// <summary>
		/// Gets private GC object's fields SyncBlockIndex and EEClass struct pointer
		/// </summary>
		public static unsafe UInt32 GetMethodTable(this object obj)
		{
			var contents = (ObjContents*)((new ObjPointer { Object = obj }).Pointer);
			return contents->methodTable;
		}
		
		/// <summary>
		/// Sets private GC object's field SyncBlockIndex, which is actually index in private GC table.
		/// </summary>
		/// <param name="obj">Object with SyncBlockIndex to be changed</param>
		/// <param name="syncBlockIndex">New value of SyncBlockIndex</param>
		public static unsafe void SetSyncBlockIndex(this object obj, UInt32 syncBlockIndex)
		{
			var contents = (ObjContents*)((new ObjPointer { Object = obj }).Pointer);
			contents->syncBlockIndex = syncBlockIndex;
		}
					
		/// <summary>
		/// Sets private GC object's field EEClass, which is actually describes current class pointer
		/// </summary>
		/// <param name="obj">Object with SyncBlockIndex to be changed</param>
		/// <param name="syncBlockIndex">New value of SyncBlockIndex</param>
		public static unsafe void SetMethodTable(this object obj, UInt32 eeClass)
		{
			var contents = (ObjContents*)((new ObjPointer { Object = obj }).Pointer);
			contents->methodTable = eeClass;
		}
		
		public static unsafe Int32 SizeOf(Type obj)
		{
			RuntimeTypeHandle th = obj.GetType().TypeHandle;
            return *(*(int**)&th + 1);
		}

		public static unsafe Int32 SizeOf<T>()
		{
			return SizeOf(typeof(T));
		}
		
		/// <summary>
		/// Allocates memory in unmanaged memory area and fills it 
		/// with MethodTable pointer to initialize managed class instance
		/// </summary>
		/// <returns></returns>
		public static T AllocInUnmanaged<T>() where T : new()
		{
			IntPtr pointer = Marshal.AllocHGlobal(SizeOf<T>());
			uint th = (UInt32)typeof(T).TypeHandle.Value.ToInt32();
			var pp = new ObjPointer();
			unsafe
			{				
				pp.Pointer = (UInt32)pointer.ToInt32();
				pp.Object.SetMethodTable((uint)th);
			}
			
			return (T)pp.Object;
		}
	}
	
	[StructLayout(LayoutKind.Explicit)]
	public struct ObjPointer
	{
		[FieldOffset(0)]
		internal object Object;
		
		[FieldOffset(4)]
		private uint _pointer;
		
		internal unsafe uint Pointer
		{
			get {
				fixed(uint *pp = &_pointer)
				{
					return *(uint *)((uint)pp - sizeof(uint)) - sizeof(uint);
				}
			}
			
			set {
				fixed(uint *pp = &_pointer)
				{
					*(uint *)((uint)pp - sizeof(uint)) = value + sizeof(uint);
				}
			}
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct ObjContents
	{
		[FieldOffset(0)]
		public UInt32 syncBlockIndex;

		[FieldOffset(4)]
		public UInt32 methodTable;

		[FieldOffset(8)]
		public byte fieldsStart;
	}

	// To read: http://msdn.microsoft.com/en-us/magazine/cc163791.aspx	
	public class MainClass		
	{
		public class Person
		{
			public int x = 123;

			public override string ToString()
			{
				return "From Person";
			}
		}
		
		public class Customer
		{
			public override string ToString()
			{
				return "From Customer";
			}
		}
		
		public unsafe static void Main(string[] args)
		{				
			unsafe {
				
				for(int i=0; i<1000000; i++)
					new Person();
				
				var obj = GCEx.AllocInUnmanaged<Customer>();
				var str = obj.ToString();
				
				Console.WriteLine("Marking unmanaged object as managed: type[{0}], ToString[{1}]", obj.GetType().Name, obj.ToString());
				
				Console.WriteLine("GC Collected: {0}, gen: {1}", GC.CollectionCount(0), GC.GetGeneration(obj));
				
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				
				Console.WriteLine("GC Collected: {0}, gen: {1}", GC.CollectionCount(0), GC.GetGeneration(obj));
				Console.ReadKey();
			};
			
			
		}
		
		private static void TestSyncBIAndEEClassChanging()
		{
			var first = new Person();
			var second = new Customer();
			
			unsafe
			{				
				Console.WriteLine("Before all:");
				PrintObjectsInfo(first, second);
				lock(first)
				{
					Console.WriteLine("After lock(first):");
					PrintObjectsInfo(first, second);
				
					lock(second)
					{					
						Console.WriteLine("After lock(second):");
						PrintObjectsInfo(first, second);
						
						first.SetMethodTable(second.GetMethodTable());
						
						Console.WriteLine("After changing type of first from 'Person' to 'Customer':");
						PrintObjectsInfo(first, second);

						var secsync = first.GetSyncBlockIndex();
						first.SetSyncBlockIndex(second.GetSyncBlockIndex());
						second.SetSyncBlockIndex(secsync);

						Console.WriteLine("After switching SyncBlockIndexes:");
						PrintObjectsInfo(first, second);
					}
					
					Console.WriteLine("After unlocking second");
					PrintObjectsInfo(first, second);
				}

				Console.WriteLine("After unlocking first");
				PrintObjectsInfo(first, second);
				
				Console.ReadKey();
			}
		}
		
		private static void TestForStrangeMarkMovingBehaviour()
		{	
			var first = new Person();
			var second = new Customer();
			
			Monitor.Enter(first);
			
			second.SetSyncBlockIndex(first.GetSyncBlockIndex());
			
			ThreadPool.QueueUserWorkItem((object o)=>
            {
             	lock(second)
             	{
             		Console.WriteLine("unlocked");
             	}
            });
			                             
			Console.ReadKey();
			
			Monitor.Exit(second);
			
			Console.ReadKey();
		}
		
		private static void TestForEqualSyncBlockIndex()
		{
			object a = new object(), b = new object(), c = new object(), d = new object(), e = new object(), f = (object)5;
			
			lock(a)
			{
				lock(b)
				{
					lock(c)
					{
						lock(d)
						{
							lock(e)
							{
								lock(f)
								{
									Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}"
									                  	, a.GetSyncBlockIndex()
									                    , b.GetSyncBlockIndex()
									                    , c.GetSyncBlockIndex()
									                    , d.GetSyncBlockIndex()
									                    , e.GetSyncBlockIndex()
									                    , f.GetSyncBlockIndex());
								}
							}
						}
					}
				}
			}
			Console.ReadKey();
		}
		
	
		private static void PrintObjectsInfo(object first, object second)
		{
			Console.WriteLine("type of first: {0}, ToString(): {1}, first sync:{2}, second sync:{3}", 
	              first.GetType().Name, 
	              first, 
	              first.GetSyncBlockIndex(), 
	              second.GetSyncBlockIndex());
		}
	}
}
