using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CLR;
using System.Text;

namespace _08_unmanagedPoolPerf
{
	public class MainClass		
	{
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
		
		public static void Main(string[] args)
		{
			var heap = new UnmanagedHeap<Customer>(50000);
            
            Console.ReadKey();

		    var sw = Stopwatch.StartNew();
		    for (int i = 0; i < 50000; i++)
		    {
		        var obj = heap.AllocatePure();
		    }
            var pureAllocTicks = sw.ElapsedTicks;
		    
            Console.WriteLine("Ctor call via reflection (on already allocated memory): {0}", pureAllocTicks);
            heap.Reset();

		    sw = Stopwatch.StartNew();
		    for (int i = 0; i < 50000; i++)
		    {
		        var obj = heap.Allocate();
		    }
		    var ctorRedirTicks = sw.ElapsedTicks;

		    Console.WriteLine("Ctor call via method body ptr redirection: {0}", ctorRedirTicks);

            sw = Stopwatch.StartNew();
		    for (int i = 0; i < 50000; i++)
		    {
		        var obj = new Customer(123);
            }
		    var newObjTicks = sw.ElapsedTicks;
    
		    Console.WriteLine("pure allocation in managed memory: {0}", newObjTicks);
		    Console.WriteLine("ctor Redirection / Refl ctor call: {0} (higher is faster)", (float) pureAllocTicks / ctorRedirTicks);
		    Console.WriteLine("ctor Redirection / newobj:         {0} (higher is faster)", (float) newObjTicks / ctorRedirTicks);
		    Console.WriteLine("newobj / Refl ctor call:           {0} (higher is faster)", (float) pureAllocTicks / newObjTicks);

		    Console.ReadKey();
		}
	}
}
