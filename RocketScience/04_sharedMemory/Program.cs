namespace _04_sharedMemory
{
    using System;
    using _04_sharedMemoryLib;

    class Program
    {
        static void Main(string[] args)
        {
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
        }
    }
}