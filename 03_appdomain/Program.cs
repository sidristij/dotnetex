using System;
using System.Collections.Generic;
using System.Runtime.CLR;

namespace _03_appdomain
{
    public class AppDomainRunner : MarshalByRefObject
    {
        private void methodInsideAppDomain(IntPtr startingIntPtr)
        {
            foreach (var obj in GCEx.GetObjectsInSOH(EntityPtr.ToInstance<object>(startingIntPtr), mt => true))
            {
                Console.WriteLine(" - object: {0}, type: {1}, size: {2}", obj, obj.GetType().Name, GCEx.SizeOf(obj));
            }
        }

        public static void Go(IntPtr startingIntPtr)
        {
            // make appdomain
            var dom = AppDomain.CreateDomain("PseudoIsolated", null, new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory
            });

            // create object instance
            var p = (AppDomainRunner)dom.CreateInstanceAndUnwrap(typeof(AppDomainRunner).Assembly.FullName, typeof(AppDomainRunner).FullName);

            // enumerate objects from outside area to our appdomain area
            p.methodInsideAppDomain(startingIntPtr);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var obj= new object();
            var list = new List<int>(100);
            var objPtr = EntityPtr.ToPointer(obj);
            Console.ReadKey();

            AppDomainRunner.Go(objPtr);

            Console.WriteLine("Still alive: {0}", obj);
            Console.ReadKey();
        }
    }
}
