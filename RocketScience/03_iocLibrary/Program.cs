namespace IocLibrary
{
    using System;
    
    public class Implementation : IServiceProvider
    {
        public override string ToString()
        {
            return "Awesome";
        }

        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}
