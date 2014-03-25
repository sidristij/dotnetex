using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CLR;
using System.Text;
using Microsoft.Win32;

namespace _02_sizeof
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            Console.WriteLine("\nByType:");
            PrintSize<object>();
            PrintSize<byte>();
            PrintSize<short>();
            PrintSize<int>();
            PrintSize<string>();
            Console.ReadKey();

            Console.WriteLine("\nByValue (with boxing if valuetype:");
            PrintSize(new object());
            PrintSize("Hello!");
            PrintSize("Hello!!");
            PrintSize("Hello!!!");
            PrintSize("Hello!!!!");

            PrintSize((byte)123);
            PrintSize((sbyte)123);
            PrintSize('1');
            PrintSize((short)123);
            PrintSize((ushort)123);
            PrintSize((int)123);
            PrintSize((uint)123);
            PrintSize((long)123);
            PrintSize((ulong)123);
            PrintSize(123f);
            PrintSize(123.0);
            PrintSize((decimal)123f);

            PrintSize(new EntityInfo());
            PrintSize(new MethodTableInfo());
            Console.ReadKey();
            
            Console.WriteLine("\nByRef:");
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
