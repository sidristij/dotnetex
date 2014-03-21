using System.Threading;

namespace System.Runtime.CLR
{
    public class GCCritical : IDisposable
    {
        private readonly ThreadPriority _priority;

        private GCCritical()
        {
            // We dont need to be more important then finalization thread
            _priority = Thread.CurrentThread.Priority;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
        }

        public static GCCritical Start()
        {
            return new GCCritical();
        }

        public void Dispose()
        {
            Thread.CurrentThread.Priority = _priority;
        }
    }
}
