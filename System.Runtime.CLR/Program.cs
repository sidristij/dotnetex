using System;
using System.Diagnostics;
using System.Runtime.CLR;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

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

		    Console.ReadKey();
		}
	}
}
