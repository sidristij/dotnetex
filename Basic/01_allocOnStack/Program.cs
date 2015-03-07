using System;
using System.Runtime.CLR;
namespace AllocOnStackSample
{

    class Program
    {
        /// <summary>
        /// Simple class with overrided method
        /// </summary>
        public class Customer
        {
            public virtual void M1()
            {

            }
            public virtual void M2()
            {

            }
            public virtual string M3()
            {
                return "ahahah, prekrati";
            }
        }

        static unsafe void Main(string[] args)
        {
            var obj = new Customer();
            var addr = EntityPtr.ToPointer(obj);

            Console.WriteLine(addr);
            Console.ReadKey();
        }
    }
}
