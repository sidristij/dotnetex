using System;
using System.Threading;
using AdvancedThreading;

namespace ForkingThreadSample
{
    public static class Program
    {
        static readonly object _sync = new object();

        static void Main(string[] args)
        {
            var sameLocalVariable = 123;

            if (Fork.CloneThread())
            {
                lock (_sync)
                {
                    Console.WriteLine("in parent thread: {0}, tid: {1} ", sameLocalVariable, Thread.CurrentThread.ManagedThreadId);
                    Console.ReadKey();
                }
            }
            else
            {
                lock (_sync)
                {
                    Console.WriteLine("in forked thread: {0}, tid: {1} ", sameLocalVariable, Thread.CurrentThread.ManagedThreadId);
                    Console.ReadKey();
                }
            }
        }
    }
}
