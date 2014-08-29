namespace SandboxingAppDomainSample
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CLR;
    using System.Security;
    using System.Security.Permissions;
    
    public class AppDomainRunner : MarshalByRefObject
    {
        private void methodInsideAppDomain(IntPtr startingIntPtr)
        {
            foreach (var obj in GCEx.GetObjectsInSOH(EntityPtr.ToInstance<object>(startingIntPtr), mt => mt != 0))
            {
                Console.WriteLine(" - object: {0}, type: {1}, size: {2}", obj.Item, obj.Item.GetType().Name, GCEx.SizeOf(obj.Item));
            }
        }

        public static void Go(IntPtr startingIntPtr)
        {
            var permissions = new PermissionSet(PermissionState.None);
            permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

            // make appdomain
            var dom = AppDomain.CreateDomain("Isolated", null, new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory
            }, permissions);

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
            var obj = new object();
            var objPtr = EntityPtr.ToPointer(obj);

            var list = new List<int>(100);

            AppDomainRunner.Go(objPtr);

            Console.WriteLine("Still alive: {0}", obj);

            Console.ReadKey();
            Console.WriteLine(list.Count);
        }
    }
}
