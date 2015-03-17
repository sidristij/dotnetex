namespace _10_directCastFeatures
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CLR;

    class A
    {
        public virtual string SomeMethod()
        {
            return "A::M1";
        }
    }

    class B : A
    {
        public override string SomeMethod()
        {
            return "B::M1";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ShowSimpleCastExample();

            CastingBetweenIncompatibleTypes();

            Console.ReadKey();
        }

        private static void ShowSimpleCastExample()
        {
            // for example, this object comes from external library
            var obj = new A();

            // rewrite VMT address
            obj.SetType<B>();

            // Call SomeMethod with changed logic
            Console.WriteLine("Calling SomeMethod from object with rewritten VMT address:\r\n{0} (initially was A::M1)\r\n\n", obj.SomeMethod());
        }

        private static void CastingBetweenIncompatibleTypes()
        {
            // Create List of derived class instances
            var obj = new List<B>();

            // Interpret it as List of base class instances
            var changed = EntityPtr.CastRef<List<A>>(obj);

            // call method
            changed.Add(new B());

            // call another method
            Console.WriteLine("We cannot cast from List<B> to List<A> using C#, but can using CastRef<List<A>>(...):\r\n\"{0}\" is added using List<A> which is actually List<B>", changed[0]);
        }
    }
}
