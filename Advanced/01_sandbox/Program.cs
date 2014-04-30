using System;

namespace sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var safeptr1 = SafePtr.Create("Hello");
            var safeptr2 = SafePtr.Create("Hello Hello");
            
            safeptr1.SetPointer(safeptr2);
            Console.WriteLine("Printing just 'Hello', but:");
            Console.WriteLine(safeptr1.Object);
            Console.WriteLine(safeptr1.IntPtr.ToInt32());
            Console.ReadKey();
        }
    }
}
