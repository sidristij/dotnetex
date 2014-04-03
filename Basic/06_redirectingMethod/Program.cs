using System;
using System.Reflection;
using System.Runtime.CLR;
using System.Runtime.CompilerServices;

namespace _06_redirectingMethod
{
    class Program
    {
        static void Main(string[] args)
        {
            // Run static method replace
            StaticTests();

            // Run instance replace tests
            InstanceTests();

            // Run the dynamic tests
            // DynamicTests();
            Console.ReadKey();
        }

        private static void StaticTests()
        {
            MethodBase[] methods = new MethodBase[]
            {
                typeof(StaticClassA).GetMethod("A",BindingFlags.Static|BindingFlags.Public),
                typeof(StaticClassA).GetMethod("B",BindingFlags.Static|BindingFlags.Public),
                typeof(StaticClassB).GetMethod("A",BindingFlags.Static|BindingFlags.Public),
                typeof(StaticClassB).GetMethod("B",BindingFlags.Static|BindingFlags.Public)
            };

            // Jit TestStaticReplaceJited
            RuntimeHelpers.PrepareMethod(
                typeof(Program).GetMethod("TestStaticReplaceJited", BindingFlags.Static | BindingFlags.NonPublic).MethodHandle);

            // Replace StaticClassA.A() with StaticClassB.A()
            Console.WriteLine("Replacing StaticClassA.A() with StaticClassB.A()");
            MethodUtil.ReplaceMethod(methods[2], methods[0]);

            // Call StaticClassA.A() from a  method that has already been jited
            Console.WriteLine("Call StaticClassA.A() from a  method that has already been jited");
            TestStaticReplaceJited();

            // Call StaticClassA.A() from a  method that has not been jited
            Console.WriteLine("Call StaticClassA.A() from a  method that has not been jited");
            TestStaticReplace();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void TestStaticReplace()
        {
            StaticClassA.A();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void TestStaticReplaceJited()
        {
            StaticClassA.A();
        }

        private static void InstanceTests()
        {
            MethodBase[] methods = new MethodBase[]
            {
                typeof(InstanceClassA).GetMethod("A",BindingFlags.Instance|BindingFlags.Public),
                typeof(InstanceClassA).GetMethod("B",BindingFlags.Instance|BindingFlags.Public),
                typeof(InstanceClassB).GetMethod("A",BindingFlags.Instance|BindingFlags.Public),
                typeof(InstanceClassB).GetMethod("B",BindingFlags.Instance|BindingFlags.Public)
            };



            // Jit TestStaticReplaceJited
            RuntimeHelpers.PrepareMethod(
                typeof(Program).GetMethod("TestInstanceReplaceJited", BindingFlags.Static | BindingFlags.NonPublic).MethodHandle);

            // Replace StaticClassA.A() with StaticClassB.A()
            Console.WriteLine("Replacing InstanceClassA.A() with InstanceClassB.A()");
            MethodUtil.ReplaceMethod(methods[2], methods[0]);

            // Call StaticClassA.A() from a  method that has already been jited
            Console.WriteLine("Call InstanceClassA.A() from a  method that has already been jited");
            TestInstanceReplaceJited();

            // Call StaticClassA.A() from a  method that has not been jited
            Console.WriteLine("Call InstanceClassA.A() from a  method that has not been jited");
            TestInstanceReplace();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void TestInstanceReplace()
        {
            InstanceClassA a = new InstanceClassA();
            a.A();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void TestInstanceReplaceJited()
        {
            InstanceClassA a = new InstanceClassA();
            a.A();
        }
    }
}
