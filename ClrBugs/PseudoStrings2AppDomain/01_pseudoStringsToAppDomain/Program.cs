namespace AppDomainsSample
{
    using System;
    using System.Linq.Expressions;
    using System.Runtime.CLR;
    using System.Security;
    using System.Security.Permissions;

    public class AppDomainRunner : MarshalByRefObject
    {
        private void methodInsideAppDomain(string str)
        {
            object tmp = str;
            var act = (Action)tmp;
            act();
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
            Expression<Action> expression = () => Console.WriteLine("Surprise!");

            AppDomainRunner.Go(EntityPtr.CastRef<string>(expression.Compile()));

            Console.ReadKey();
        }
    }
}
