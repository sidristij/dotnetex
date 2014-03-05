using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.CLR;
using System.Data;
using System.Linq;

namespace ConsoleTest
{

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
			var a = new object();
				
			var heap = new UnmanagedHeap<Dictionary<Type, int>>(100);
			var list = new List<object>(100);
			
			for(int i = 0; i < 100; i++)
			{
				list.Add(heap.Allocate());
			}
			
			int count = 0, cursize = 0, size = 0;

			foreach(var cur in GCEx.GetObjectsInSOH(a))
			{
				cursize = GCEx.SizeOf(cur);
				
				if(cur is UnmanagedHeap<Dictionary<Type, int>>)
				{
					Console.WriteLine("At [0x{0:X}] Type found: {1}, points to heap of [{2}] size", (int)cur.GetEntityInfo(), cur.GetType().Name, (cur as UnmanagedHeap<Dictionary<Type, int>>).TotalSize );
				} else {
					Console.WriteLine("At [0x{0:X}] Type found: {1}", (int)cur.GetEntityInfo(), cur.GetType().Name);
				}
				size += cursize;					
				count++;
			}
			
			Console.WriteLine(" - sum: {0}, count: {1}", size, count);
			Console.ReadKey();
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
						
						first.SetMethodTable(second.GetEntityInfo()->MethodTable);
						
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
