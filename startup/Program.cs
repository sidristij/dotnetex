using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CLR;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading;

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
		
		public class Customer : UnmanagedObject<Customer>
		{
			int x = 20;
			
			public Customer()
			{			
			}
			
			public Customer(int a)
			{
			}
			
			public Customer(int a, int b)
			{
			}
			
			public override string ToString()
			{
				return "From Customer";
			}
		}
		
		public unsafe static void Main(string[] args)
		{
			var a = new object();
			var heap = new UnmanagedHeap<Customer>(10000);
			var b = new object();

		    using (GCCritical.Start())
		    {
                
		        var handle = GCHandle.Alloc(a, GCHandleType.Weak);
		        // Let CLR finish all startup processes
		        Console.ReadKey();

		        Sandbox.Go();

                Console.ReadKey();

		        int gcc = GC.CollectionCount(0);
                
		        var sw = Stopwatch.StartNew();
		        for (int i = 0; i < 10000; i++)
		        {
		            var obj = heap.AllocatePure();
		        }

		        var pureAllocTicks = sw.ElapsedTicks;
		        Console.WriteLine("Ctor call via reflection (on already allocated memory): {0}", pureAllocTicks);

		        heap.Reset();

		        sw = Stopwatch.StartNew();
		        for (int i = 0; i < 10000; i++)
		        {
		            var obj = heap.Allocate();
		        }

		        var ctorRedirTicks = sw.ElapsedTicks;
		        Console.WriteLine("Ctor call via method body ptr redirection: {0}", ctorRedirTicks);

                sw = Stopwatch.StartNew();
		        for (int i = 0; i < 10000; i++)
		        {
		            var obj = new Customer(123);
                }

                int gcc2 = GC.CollectionCount(0);
		        
		        var newObjTicks = sw.ElapsedTicks;
		        Console.WriteLine("pure allocation in managed memory: {0}", newObjTicks);

		        Console.WriteLine("ctor Redirection / Refl ctor call {0} (higher is faster)",
		            (float) pureAllocTicks/ctorRedirTicks);
		        Console.WriteLine("ctor Redirection / newobj:        {0} (higher is faster)",
		            (float) newObjTicks/ctorRedirTicks);
		        Console.WriteLine("newobj / Refl ctor call:          {0} (higher is faster)",
		            (float) pureAllocTicks/newObjTicks);

                Console.WriteLine("{0} : {1}", gcc, gcc2);

		    }
		    Console.ReadKey();
		}
		
		private static unsafe void EnumerateAllFrom(object starting)
		{
			int count = 0, cursize = 0, size = 0;

			foreach(var cur in GCEx.GetObjectsInSOH(starting))
			{
				cursize = GCEx.SizeOf(cur);
				
				if(cur is UnmanagedHeap<Customer>)
				{
					Console.WriteLine("At [0x{0:X}] Type found: {1}, points to heap of [{2}] size", (int)cur.GetEntityInfo(), cur.GetType().Name, (cur as UnmanagedHeap<Customer>).TotalSize );
				} else {
					Console.WriteLine("At [0x{0:X}] Type found: {1}", (int)cur.GetEntityInfo(), cur.GetType().Name);
				}
				size += cursize;					
				count++;
			}
			
			Console.WriteLine(" - sum: {0}, count: {1}", size, count);
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

	    public class Sandbox : MarshalByRefObject
        {
            private int foo()
            {
                var a = new object();
                var ptr = EntityPtr.ToHandler(a);
                return ptr;
            }

            public static void Go()
            {
                var permissions = new PermissionSet(PermissionState.None);
                permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution | SecurityPermissionFlag.UnmanagedCode));

                var dom = AppDomain.CreateDomain("foo", null, new AppDomainSetup
                {
                    ApplicationBase = AppDomain.CurrentDomain.BaseDirectory
                }, permissions);

                var p = (Sandbox)dom.CreateInstanceAndUnwrap(typeof(Sandbox).Assembly.FullName, typeof(Sandbox).FullName);

                Console.WriteLine(p.foo());
            }
        }
	}
}
