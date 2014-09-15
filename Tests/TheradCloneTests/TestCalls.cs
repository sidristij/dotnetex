namespace TheradCloneTests
{
    using System;
    using System.Threading;
    using AdvancedThreading;
    using NUnit.Framework;

    [TestFixture]
    public class TestCalls
    {
        object _sync = new object();

        [Test]
        public void TestCurrentContextClone()
        {
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            resetEvent.Reset();
            if (Fork.CloneThread())
            {
                resetEvent.Set();
            }
            else
            {
                resetEvent.WaitOne();
                resetEvent.Dispose();
            }
        }

        [Test]
        public void TestCurrentContextCloneInsideAnother()
        {
            CountdownEvent countdownEvent = new CountdownEvent(2);
            countdownEvent.Reset(2);

            if (Fork.CloneThread())
            {
                Fork.CloneThread();
                countdownEvent.Signal();
            }
            else
            {
                countdownEvent.Wait();
                countdownEvent.Dispose();
            }
        }

        [Test]
        public void TestCurrentContextShouldBeSavedForValueTypes()
        {
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            resetEvent.Reset();
            int data = 123;

            var forked = Fork.CloneThread();

            if(data != 123) throw new Exception("data was not saved");

            if (forked)
            {
                resetEvent.Set();
            }
            else
            {
                resetEvent.WaitOne();
                resetEvent.Dispose();
            }
        }
    }
}
