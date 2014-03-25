using System;
using System.Runtime.CLR;

namespace _02_sizeof
{
    class Program
    {
        static void Main()
        {
            // Printing sizes by type, not by value
            Console.WriteLine("\nByType:");
            PrintSize<object>();
            PrintSize<byte>();
            PrintSize<short>();
            PrintSize<int>();
            PrintSize<string>();
            Console.ReadKey();

            // Reference types sizes (obj and strings)
            Console.WriteLine("\nByValue (with boxing if valuetype:");
            PrintSize(new object());
            PrintSize("Hello!");
            PrintSize("Hello!!");
            PrintSize("Hello!!!");
            PrintSize("Hello!!!!");

            // Boxing for elementary types and structs
            PrintSize((object)(byte)123);
            PrintSize((object)(sbyte)123);
            PrintSize((object)'1');
            PrintSize((object)(short)123);
            PrintSize((object)(ushort)123);
            PrintSize((object)123);
            PrintSize((object)(uint)123);
            PrintSize((object)(long)123);
            PrintSize((object)(ulong)123);
            PrintSize((object)123f);
            PrintSize((object)123.0);
            PrintSize((object)(decimal)123f);
            PrintSize((object)new EntityInfo());
            PrintSize((object)new MethodTableInfo());
            Console.ReadKey();

            // Structs
            Console.WriteLine("\nStructs:");
            PrintSize<EntityInfo>();
            PrintSize<MethodTableInfo>();
            Console.ReadKey();

            // Arrays
            Console.WriteLine("\nArrays:");
            PrintSize(new byte[20]);
            PrintSize(new ulong[20]);
            PrintSize(new object[20]);
            PrintSize(new string[20]);
            Console.ReadKey();
        }

        private static void PrintSize<T>()
        {
            Console.WriteLine("sizeof({0}): {1} bytes", typeof(T).Name, GCEx.SizeOf<T>());
        }

        private static void PrintSize<T>(T val)
        {
            Console.WriteLine("sizeof({0}): {1} bytes ({2})", typeof(T).Name, GCEx.SizeOf(val), val);
        }
    }
}
