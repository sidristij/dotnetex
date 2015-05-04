namespace UnmanagedPoolPerfSample
{
    using System;
    using System.Runtime.CLR;
    using BenchmarkDotNet.Attributes;

    public class Competition
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

        const int N = 50001, Iter = 71;
        readonly UnmanagedHeap<Customer> heap = new UnmanagedHeap<Customer>(N);

        [Benchmark("Ctor call via reflection (on already allocated memory)")]
        public void Reflection()
        {
            for (int j = 0; j < Iter; j++)
            {
                for (int i = 0; i < N; i++)
                    heap.AllocatePure();
                heap.Reset();
            }
        }

        [Benchmark("Ctor call via method body ptr redirection")]
        public void MethodBodyPtr()
        {
            for (int j = 0; j < Iter; j++)
            {
                for (int i = 0; i < N; i++)
                    heap.Allocate();
                heap.Reset();
            }
        }

        [Benchmark("Pure allocation in managed memory")]
        public void PureAllocation()
        {
            for (int j = 0; j < Iter; j++)
            {
                for (int i = 0; i < N; i++)
                    new Customer(123);
                GC.Collect();
            }
        }
    }
}