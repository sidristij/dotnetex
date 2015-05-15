namespace _04_sharedMemoryReciever
{
    using System;
    using _04_sharedMemoryLib;

    class Program
    {
        static void Main(string[] args)
        {
            var fixedmap = new FixedAddressTypesMap(0x0032e2000);
            fixedmap.GetOrAddType<SharedType>();
            
            using (var reciever = new SharedMemoryManager<SharedType>(typeof(SharedType).FullName, 1024))
            using (var sender = new SharedMemoryManager<string>(typeof(string).FullName, 1024))
            {
                    var obj = reciever.ReceiveObject();
                    var str = string.Format("Recieved: {0}", obj);
                    Console.WriteLine(str);
                    sender.ShareObject(str);
            }
            Console.ReadKey();
        }
    }
}
