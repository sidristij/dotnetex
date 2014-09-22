namespace AdvancedThreadingSample
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using AdvancedThreading;
    
    /// <summary>
    /// This sample shows how to clone existing thread flow to another.
    /// Note: Method, which calls Clone() should not contain ref/out parameters, because they makes stack references in stack, which shouldn't be fixed. 
    /// </summary>
    public static class Program
    {
        static readonly object Sync = new object();
        static readonly CountdownEvent joined = new CountdownEvent(2);

        static void Main()
        {
            Console.WriteLine("Press [Enter] to start");
            Console.ReadKey();

            Console.WriteLine("Splitting to thread pool:");

            var i = 0;
            for (; i < 20; i++)
            {
                joined.Reset(2);
                MakeFork();
                joined.Wait();
            }

            Console.WriteLine("Fork called successfully {0} times", i);
            Console.ReadKey();
        }

        static void MakeFork()
        {
            var cdevent = new CountdownEvent(2);
            var sameLocalVariable = 123;
            var stopwatch = Stopwatch.StartNew();

            // Splitting current thread flow to two threads
            var forked = Fork.CloneThread();

            lock (Sync)
            {
                Console.WriteLine("in {0} thread: {1}, local value: {2}, time to enter = {3} ms",
                    forked ? "forked" : "parent",
                    Thread.CurrentThread.ManagedThreadId,
                    sameLocalVariable, 
                    stopwatch.ElapsedMilliseconds);
                cdevent.Signal();
            }

            // Here forked thread's life will be stopped
            joined.Signal();
        }
    }
}
