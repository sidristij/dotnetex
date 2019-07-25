namespace AllocOnStackSample
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CLR;

    class Program
    {
        /// <summary>
        /// Simple class with overriden method
        /// </summary>
        public class Customer
        {
            public virtual string M1()
            {
                return "M1::base";
            }
        }

        public class SubCustomer : Customer
        {
            public override string M1()
            {
                return "M1";
            }

            public override string ToString()
            {
                return "ToString()";
            }
        }

        static void Main(string[] args)
        {
            var x = new Customer();
            x.SetType<SubCustomer>();
            Console.WriteLine(x.M1());
            Console.ReadKey();
        }
    }
}
