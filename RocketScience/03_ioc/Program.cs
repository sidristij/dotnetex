using System;
using System.Linq;

namespace IocSample
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var container = new Container("03_iocLibrary.dll"))
            {
                container.Register<IServiceProvider>("IocLibrary.Implementation");
                var serviceProvider = container.Resolve<IServiceProvider>();

                Console.WriteLine("calling method without proxy: {0}", serviceProvider);
                Console.WriteLine("Current domain assemblies: {0}",
                                  string.Join(",  ", AppDomain.CurrentDomain.GetAssemblies().Select(asm => asm.GetName().Name).ToArray()));
            }
        }
    }
}
