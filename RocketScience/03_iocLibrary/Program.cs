using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IocLibrary
{
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
