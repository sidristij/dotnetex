using System;
using System.Diagnostics;
using System.Threading;
using AdvancedThreading;

namespace ForkingThreadSample
{
    public static class Program
    {
        static readonly object _sync = new object();

        static void Main(string[] args)
        {
            Console.WriteLine("Press [Enter] to start");
            Console.ReadKey();

            Console.WriteLine("Splitting to new thread:");
            MakeFork(false);

            Console.WriteLine("Splitting to thread pool:");
            MakeFork(true);
            
            Console.WriteLine("Fork called successfully");
            Console.ReadKey();
        }

        static void MakeFork(bool inThreadpool)
        {
            var cdevent = new CountdownEvent(2);
            var sameLocalVariable = 123;
            var stopwatch = Stopwatch.StartNew();

            // Splitting current thread flow to two threads
            var forked = Fork.CloneThread(inThreadpool);

            lock (_sync)
            {
                Console.WriteLine("in {0} thread: {1}, local value: {2}, time to enter = {3} ms",
                    forked ? "forked" : "parent",
                    Thread.CurrentThread.ManagedThreadId,
                    sameLocalVariable, 
                    stopwatch.ElapsedMilliseconds);
                cdevent.Signal();
            }

            // Here forked thread's life will be stopped
        }
    }
}
