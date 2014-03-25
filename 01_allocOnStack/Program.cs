using System;
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

            // print contents via .ToString()
            Console.WriteLine(customerOnStack);
        }
    }
}
