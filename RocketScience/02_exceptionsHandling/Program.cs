namespace ExceptionsHandlingSample
{
    using System;
    using System.Diagnostics;
    
    class Program
    {
        static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();
            
            for (int i = 0; i < 100; i++)
                WithoutException();

            var withoutPerf = sw.ElapsedTicks;

            Console.WriteLine("Without exception: {0}", withoutPerf);

            sw.Restart();

            for (int i = 0; i < 100; i++)
                WithExceptionHandler();

            var withPerf = sw.ElapsedTicks;

            Console.WriteLine("With exception handler: {0}", withoutPerf);

            sw.Restart();

            for (int i = 0; i < 100; i++)
                WithExceptionHandlerAndThorwing();

            var throwingPerf = sw.ElapsedTicks;

            Console.WriteLine("With exception handler and throw: {0}", throwingPerf);
            Console.WriteLine("Writting bool TryDoSomething(out result) is {0} times faster", throwingPerf / withoutPerf);


            Console.ReadKey();
        }

        #region Simple Sample

        static void WithoutException()
        {
            WithoutExceptionImpl();
        }

        static void WithoutExceptionImpl()
        {
            ;
        }

        static void WithExceptionHandler()
        {
            try
            {
                WithExceptionHandlerImpl();
            }
            catch (ArgumentOutOfRangeException exception)
            {
            }
            catch (NullReferenceException exception)
            {
            }
            catch (Exception exception)
            {
                ;
            }
        }

        static void WithExceptionHandlerImpl()
        {
            ;
        }

        static void WithExceptionHandlerAndThorwing()
        {
            try
            {
                WithExceptionHandlerAndThrowingImpl();
            }
            catch (ArgumentOutOfRangeException exception)
            {
            }
            catch (NullReferenceException exception)
            {
            }
            catch (Exception exception)
            {
                throw;
            }
        }

        static ArgumentOutOfRangeException exception = new ArgumentOutOfRangeException();

        static void WithExceptionHandlerAndThrowingImpl()
        {
            throw exception;
        }

        #endregion
    }
}
