namespace _04_sharedMemory
{
    using System;
    using _04_sharedMemoryLib;

    class Program
    {
        static void Main(string[] args)
        {
            var fixedmap = new FixedAddressTypesMap(0x0017f000);
            var holder = fixedmap.GetOrAddType<SharedType>();

            var testobject = new SharedType();
            Console.WriteLine(testobject.ToString());

            using (var sender = new SharedMemoryManager<SharedType>(typeof(SharedType).FullName, 1024))
            using (var reciever = new SharedMemoryManager<string>(typeof(string).FullName, 1024))
            {
                var tosend = new SharedType();    
                tosend.SetX(100);

                holder.AsSharedType(tosend);  
                sender.ShareObject(tosend);

                var obj = reciever.ReceiveObject();

                Console.WriteLine("{0}", obj);
                Console.ReadKey();
            }
        }
    }
}