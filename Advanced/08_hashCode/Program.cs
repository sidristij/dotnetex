using System;
using System.Runtime.CLR;

namespace _08_hashCode
{
    class Program
    {
        static void Main(string[] args)
        {
            var obj1 = new object();
            var obj2 = new object();
            var obj3 = new object();

            var ref1 = EntityPtr.ToPointer(obj1).ToInt32();
            var ref2 = EntityPtr.ToPointer(obj2).ToInt32();
            var ref3 = EntityPtr.ToPointer(obj3).ToInt32();

            var hash1 = obj1.GetHashCode() >> 3;
            var hash2 = obj2.GetHashCode() >> 3;
            var hash3 = obj3.GetHashCode() >> 3;

            Console.WriteLine("{0}, {1}, {2}", hash1 -ref1, hash2-ref2, hash3 - ref3);
        }
    }
}
