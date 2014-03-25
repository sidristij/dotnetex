using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CLR;
using System.Text;

namespace _06_redirectingMethod
{
    class Program
    {
        public class Pencil
        {
            public virtual void Draw()
            {
                Console.WriteLine("Drawing on paper");
            }
        }

        public class Paper
        {
            public virtual void Clear()
            {
                Console.WriteLine("Clearing paper");
            }
        }

        static void Main(string[] args)
        {
            var paper = new Paper();
            var pencil = new Pencil();

            MethodUtil.ReplaceMethod(typeof(Pencil).GetMethod("Draw"), typeof(Paper).GetMethod("Clear"), true);

            pencil.Draw();
            paper.Clear();

            Console.ReadKey();
        }
    }
}
