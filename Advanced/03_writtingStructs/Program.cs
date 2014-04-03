using System;
using System.Security.AccessControl;
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

    class Program
    {
        static void Main(string[] args)
        {
            MethodTableInfo firstTableInfo, secondTableInfo;
            EntityInfoPtr first_entityptr, second_entityptr;
            var customer = new Customer();

            /*taking address from Customer*/
            {
                var safeptr1 = SafePtr.Create(customer);
                first_entityptr = new EntityInfoPtr(safeptr1.IntPtr - IntPtr.Size);
                var first_mtintptr = first_entityptr.Reference.mt.MtPointer;
                firstTableInfo = new MethodTablePtr(first_mtintptr - IntPtr.Size).Reference.mt;
                Console.WriteLine("Contents: {0}, {1}", firstTableInfo.Size, firstTableInfo.MethodsCount);
            }

            /*taking address from Client*/
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

            Console.WriteLine("Customer after mt rewritting: {0}", customer);
            Console.ReadKey();
        }
    }
}
