using System;
using System.Runtime.CLR;

namespace UnmanagedPoolSample
{
    class Program
    {
        /// <summary>
        /// Now cannot call default .ctor
        /// </summary>
        private class Quote : UnmanagedObject<Quote>
        {
            public Quote(int urr)
            {
                Console.WriteLine("Hello from object .ctor");
            }

            public int GetCurrent()
            {
                return 100;
            }
        }

        static void Main(string[] args)
        {
            using (var pool = new UnmanagedHeap<Quote>(1000))
            {
                using (var quote = pool.Allocate())
                {
                    Console.WriteLine("quote: {0}", quote.GetCurrent());
                }
            }

            Console.ReadKey();
        }
    }
}
