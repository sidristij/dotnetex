using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Permissions;
using readingStructs;

namespace sandbox
{
    public class Customer
    {
        public override string ToString()
        {
            return "Customer class";
        }
    }

    public class Client
    {
        public override string ToString()
        {
            return "Client class";
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Data
    {
        [FieldOffset(0)]
        public IntPtr data;

        [FieldOffset(0)]
        public GCHandle Handle;
    }

    
    public class Sandbox : MarshalByRefObject
    {
        public void foo()
        {
          //  MethodTableInfo firsTableInfo, secondTableInfo;
          //  EntityInfoPtr first_entityptr, second_entityptr;
            var customer = new Customer();
            var dd = new Data();

            //var s = PtrConverter.Convert(new object());
            /*taking address from Customer*
            {
                var safeptr1 = SafePtr.Create(customer);
                first_entityptr = new EntityInfoPtr(safeptr1.IntPtr - IntPtr.Size);
                var first_mtintptr = first_entityptr.Reference.mt.MtPointer;
                firsTableInfo = new MethodTablePtr(first_mtintptr - IntPtr.Size).Reference.mt;
                Console.WriteLine("Contents: {0}, {1}", firsTableInfo.Size, firsTableInfo.MethodsCount);
            }

            /*taking address from Client*
            {
                var safeptr2 = SafePtr.Create(new Client());
                second_entityptr = new EntityInfoPtr(safeptr2.IntPtr - IntPtr.Size);
                var second_mtintptr = second_entityptr.Reference.mt.MtPointer;
                secondTableInfo = new MethodTablePtr(second_mtintptr - IntPtr.Size).Reference.mt;
                Console.WriteLine("Contents: {0}, {1}", secondTableInfo.Size, secondTableInfo.MethodsCount);
            }

            Console.WriteLine("Customer before mt rewritting: {0}", customer);

            // changing methods table address
            first_entityptr.Reference.mt.MtPointer = second_entityptr.Reference.mt.MtPointer;

            Console.WriteLine("Customer after  mt rewritting: {0}", customer);
             * */
        }

        public static void Go()
        {
            var permissions = new PermissionSet(PermissionState.None);
            permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution | SecurityPermissionFlag.UnmanagedCode));

            var dom = AppDomain.CreateDomain("foo", null, new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory
            }, permissions);

            var p = (Sandbox)dom.CreateInstanceAndUnwrap(typeof(Sandbox).Assembly.FullName, typeof(Sandbox).FullName);


            var s = PtrConverter.Convert(new object());

            p.foo();
        }
    }

    class Program
    {
        static unsafe void Main(string[] args)
        {
            Sandbox.Go();
        }
    }
}
