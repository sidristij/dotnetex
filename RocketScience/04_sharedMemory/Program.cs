namespace _04_sharedMemory
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Runtime.CLR;
    using _04_sharedMemoryLib;

    class Program
    {
        static unsafe void Main(string[] args)
        {
            var fixedmap = new FixedAddressTypesMap(0x0017f000);

            var mt = fixedmap.AddType<SharedType>();

            var testobject = new SharedType();
            var aa = EntityPtr.ToPointer(testobject);
            *(int*) EntityPtr.ToPointerWithOffset(testobject) = (int)mt; // rewrite mt to new
            Console.WriteLine(testobject.ToString());

            using (var sender = new SharedMemoryManager<SharedType>(typeof(SharedType).FullName, 1024))
            using (var reciever = new SharedMemoryManager<string>(typeof(string).FullName, 1024))
            {
                var tosend = new SharedType();
                tosend = new SharedType();
                tosend.SetX(100);

                sender.SendObject(tosend);
                var obj = reciever.ReceiveObject();

                Console.WriteLine("{0}", obj);
                Console.ReadKey();
            }
            Console.WriteLine(aa);
        }
    }
}