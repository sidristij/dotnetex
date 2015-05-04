namespace UnmanagedPoolPerfSample
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CLR;
    using BenchmarkDotNet;

    public class MainClass
    {
        public static void Main(string[] args)
        {
            var runner = new BenchmarkRunner();
            runner.RunCompetition(new Competition());
        }
    }
}
