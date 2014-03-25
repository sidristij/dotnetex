using System;
using System.Linq;
using System.Runtime.CLR;

namespace _01_allocOnStack
{
    class Program
    {
        /// <summary>
        /// Simple class with overrided method
        /// </summary>
        public class Customer
        {
            public int x;
            public int y;

            public override string ToString()
            {
                return string.Format("x: {0}, y: {1}", x, y);
            }
        }

        static unsafe void Main(string[] args)
        {
            // alloc unsafe array on stack
            var data = stackalloc int[10];

            // cast to Customer
            var customerOnStack = EntityPtr.ToInstance<Customer>((IntPtr)data);

            // Setup type on memory
            customerOnStack.SetType<Customer>();

            // init with fields
            customerOnStack.x = 5;
            customerOnStack.y = 10;

            // copy locally;
            var arr = new int[10];
            for (var i = 0; i < 10; i++)
            {
                arr[i] = data[i];
            }

            // print contents via .ToString()
            Console.WriteLine("data: {0}", String.Join(", ", arr));
            Console.WriteLine(customerOnStack);
            Console.ReadKey();
        }
    }
}
