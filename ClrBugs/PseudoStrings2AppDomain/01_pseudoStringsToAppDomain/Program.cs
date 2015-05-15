using System.Collections.Generic;

namespace AppDomainsSample
{
    using System;
    using System.Runtime.CLR;
    using System.Security;
    using System.Security.Permissions;

    public class AppDomainRunner : MarshalByRefObject
    {
        public class A
        {
        }

        private void methodInsideAppDomain(string str)
        {
            object tmp = str;
            var act = EntityPtr.CastRef<List<A>>(tmp);
            Console.WriteLine(act.Count);
        }

        public static void Go(string startingIntPtr)
        {
            // make appdomain
            var permissions = new PermissionSet(PermissionState.None);
            permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            var dom = AppDomain.CreateDomain("PseudoIsolated", null, new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory
            }, permissions);

            // create object instance
            var asmname = typeof(AppDomainRunner).Assembly.FullName;
            var typename = typeof(AppDomainRunner).FullName;
            var instance = (AppDomainRunner)dom.CreateInstanceAndUnwrap(asmname, typename);

            // enumerate objects from outside area to our appdomain area
            instance.methodInsideAppDomain(startingIntPtr);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var lst = new List<AppDomainRunner.A> {new AppDomainRunner.A()};

            AppDomainRunner.Go(EntityPtr.CastRef<string>(lst));

            Console.ReadKey();
        }
    }
}
