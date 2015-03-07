namespace IocSample
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CLR;
    
    public class Container : IDisposable
    {
        private readonly AppDomainRunner appdomain;

        private readonly Dictionary<Type, Object> instances = new Dictionary<Type, object>(); 

        public Container(string assemblyName)
        {
            appdomain = new AppDomainRunner(Path.Combine(System.Environment.CurrentDirectory, assemblyName));
        }

        public void Register<TInterface>(string fullTypeName)
        {
            instances.Add(typeof (TInterface), appdomain.CreateInstance(fullTypeName));
        }

        public TInterface Resolve<TInterface>()
        {
            return (TInterface)(instances[typeof (TInterface)]);
        }

        public void Dispose()
        {
            appdomain.Dispose();
        }
    }
}