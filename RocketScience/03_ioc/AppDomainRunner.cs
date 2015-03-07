namespace IocSample
{
    using System;
    using System.Reflection;
    using System.Runtime.CLR;
    
    public class AppDomainRunner : MarshalByRefObject, IDisposable
    {
        private readonly AppDomain appDomain;
        private Assembly assembly;
        private AppDomainRunner remoteRunner;

        public AppDomainRunner()
        {
            
        }

        private void LoadAssembly(string assemblyPath)
        {
            assembly = Assembly.LoadFile(assemblyPath);
        }

        public AppDomainRunner(string assemblyPath)
        {
            // make appdomain
            appDomain = AppDomain.CreateDomain(
                "PseudoIsolated", 
                null, 
                new AppDomainSetup
                {
                    ApplicationBase = AppDomain.CurrentDomain.BaseDirectory
                });

            // create object instance
            remoteRunner = (AppDomainRunner)appDomain.CreateInstanceAndUnwrap(typeof(AppDomainRunner).Assembly.FullName, typeof(AppDomainRunner).FullName);
            remoteRunner.LoadAssembly(assemblyPath);
        }

        public string CreateInstance(string typename)
        {
            return remoteRunner.CreateInstanceImpl(typename);
        }

        private string CreateInstanceImpl(string typename)
        {
            return EntityPtr.CastRef<string>(assembly.CreateInstance(typename));
        }

        public void Dispose()
        {
            assembly = null;
            remoteRunner = null;
            AppDomain.Unload(appDomain);
        }
    }
}