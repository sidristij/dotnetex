using BenchmarkDotNet;

namespace UnmanagedPoolPerfSample
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
            var competitionSwitch = new BenchmarkCompetitionSwitch(new[] {
                typeof(Competition)
            });
            competitionSwitch.Run(new[] { "Competition" });
        }
    }
}
