using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace _08_gettingPointerByHashCode
{
    public class Program
    {
        internal const String KERNEL32 = "kernel32.dll";

        static unsafe void Main(string[] args)
        {
            Fork();
            Console.ReadKey();
        }

        private static bool Fork()
        {
            Sandbox();
            /*
            var thread = new Thread(() => ForkInternal(true));
            thread.Start();

            ForkInternal(false);
            return false;
             * */
            return false;
        }

        private static void ForkInternal(bool forNewThread)
        {
            
        }

        private unsafe static void Sandbox()
        {
            var helper = new CPPCLR.RegHelper();
            int esp=0;
            helper.ForkPrepare(ref esp);
            helper.Write(esp);

            var @event = new ManualResetEvent(false);

            // Retrieving stack info
            var stackData = new MEMORY_BASIC_INFORMATION();
            char* lastStackPtr = stackalloc char[1];
            VirtualQuery(new IntPtr(lastStackPtr), ref stackData, (IntPtr)sizeof(MEMORY_BASIC_INFORMATION));

            Console.WriteLine("{0}, {1}", (int)(IntPtr)stackData.BaseAddress, (int)stackData.RegionSize);

            var stackCopy = 


            @event.WaitOne();
        }

        [DllImport(KERNEL32, SetLastError = true)]
        unsafe internal static extern IntPtr VirtualQuery(
            IntPtr address,
            ref MEMORY_BASIC_INFORMATION buffer,
            IntPtr sizeOfBuffer
        );

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct MEMORY_BASIC_INFORMATION
        {
            internal void* BaseAddress;
            internal void* AllocationBase;
            internal uint AllocationProtect;
            internal UIntPtr RegionSize;
            internal uint State;
            internal uint Protect;
            internal uint Type;
        }
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);
    }
}
