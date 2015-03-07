using System;

namespace _04_sharedMemoryReciever
{
    using _04_sharedMemoryLib;

    class Program
    {
        static void Main(string[] args)
        {
            Console.ReadKey();
            using (var reciever = new SharedMemoryManager<SharedType>(typeof(SharedType).FullName, 1024))
            using (var sender = new SharedMemoryManager<string>(typeof(string).FullName, 1024))
            {
                    var obj = reciever.ReceiveObject();
                    var str = string.Format("Recieved: {0}", obj.GetX());
                    Console.WriteLine(str);
                    sender.SendObject(str);
            }
            Console.ReadKey();
        }
    }
}
