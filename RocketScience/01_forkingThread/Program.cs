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
            M1();

            Console.WriteLine("Fork called successfully");
            Console.ReadKey();
        }

        static void M1()
        {
            InForkedFlow(Fork.CloneThread());
        }

        static void InForkedFlow(bool isChildThread)
        {
            try
            {
                var sameLocalVariable = 123;
                var cdevent = new CountdownEvent(2);
                if (isChildThread)
                {
                    lock (_sync)
                    {
                        Console.ReadKey();
                        Console.WriteLine("in forked thread: {0}, tid: {1} ", sameLocalVariable, Thread.CurrentThread.ManagedThreadId);
                        cdevent.Signal();
                        throw new Exception("Hello from try block");
                    }
                }
                else
                {
                    lock (_sync)
                    {
                        Console.ReadKey();
                        Console.WriteLine("in parent thread: {0}, tid: {1} ", sameLocalVariable, Thread.CurrentThread.ManagedThreadId);
                        cdevent.Signal();
                        throw new Exception("Hello from try block");
                    }
                }
            }
            catch (Exception exception)
            {
                lock (_sync)
                {
                    Console.WriteLine("Catch called successfully: {0}", exception.Message);
                }
            }
        }
    }
}
