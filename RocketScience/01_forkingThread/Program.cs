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
            var cdevent = new CountdownEvent(2);

            ThreadPool.QueueUserWorkItem(
            (_) =>
            {
                if (Fork.CloneThread())
                {
                    lock (_sync)
                    {
                        Console.ReadKey();
                        Console.WriteLine("in forked thread: {0}, tid: {1} ", sameLocalVariable, Thread.CurrentThread.ManagedThreadId);
                        cdevent.Signal();
                    }
                }
                else
                {
                    lock (_sync)
                    {
                        Console.ReadKey();
                        Console.WriteLine("in parent thread: {0}, tid: {1} ", sameLocalVariable, Thread.CurrentThread.ManagedThreadId);
                        cdevent.Signal();
                    }
                }
            });

            cdevent.Wait();
            Console.WriteLine("Fork called successfully");
            Console.ReadKey();
        }
    }
}
