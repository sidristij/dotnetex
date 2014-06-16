using System;
using System.Reflection;
using System.Runtime.CLR;

namespace _03_ioc
{
    public class AppDomainRunner : MarshalByRefObject, IDisposable
    {
        private AppDomain appDomain;
        private Assembly assembly;
        private AppDomainRunner remoteRunner;

        private void LoadAssembly(string assemblyPath)
        {
            assembly = Assembly.LoadFile(assemblyPath);
        }
        
        public AppDomainRunner()
        {
            ;
        }

        public AppDomainRunner(string assemblyPath)
        {
            // make appdomain
            appDomain = AppDomain.CreateDomain("PseudoIsolated", null, new AppDomainSetup
                                                                           {
                                                                               ApplicationBase = AppDomain.CurrentDomain.BaseDirectory
                                                                           });

            // create object instance
            remoteRunner = (AppDomainRunner)appDomain.CreateInstanceAndUnwrap(typeof(AppDomainRunner).Assembly.FullName, typeof(AppDomainRunner).FullName);
            remoteRunner.LoadAssembly(assemblyPath);
        }

        public IntPtr CreateInstance(string typename)
        {
            return remoteRunner.CreateInstanceImpl(typename);
        }

        private IntPtr CreateInstanceImpl(string typename)
        {
            return EntityPtr.ToPointer(assembly.CreateInstance(typename));
        }

        public void Dispose()
        {
            assembly = null;
            remoteRunner = null;
            AppDomain.Unload(appDomain);
        }
    }
}