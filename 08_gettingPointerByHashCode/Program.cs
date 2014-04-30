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
            helper.Read(ref esp);
            helper.Write(esp);

            var @event = new ManualResetEvent(false);

            MEMORY_BASIC_INFORMATION stackData = new MEMORY_BASIC_INFORMATION();
            char* lastStackPtr = stackalloc char[1];
            VirtualQuery(new IntPtr(lastStackPtr), ref stackData, (IntPtr)sizeof(MEMORY_BASIC_INFORMATION));
            Console.WriteLine("{0}, {1}", (int)(IntPtr)stackData.BaseAddress, (int)stackData.RegionSize);

            var stackSize = (int) stackData.RegionSize - (esp - (int) stackData.BaseAddress);

            new Thread(() =>
            {
                MEMORY_BASIC_INFORMATION stackData2 = new MEMORY_BASIC_INFORMATION();
                char* lastStackPtr2 = stackalloc char[1];
                VirtualQuery(new IntPtr(lastStackPtr2), ref stackData2, (IntPtr)sizeof(MEMORY_BASIC_INFORMATION));
                Console.WriteLine("{0}, {1}", (int)(IntPtr)stackData2.BaseAddress, (int)stackData2.RegionSize);

                helper.MemcpyAndSet((int)stackData2.BaseAddress + (int)stackData2.RegionSize - stackSize,
                       (int)stackData.BaseAddress + (int)stackData.RegionSize - stackSize,
                       stackSize, (int)stackData2.BaseAddress + (int)stackData2.RegionSize - stackSize);
                /*
                helper.
                memcpy();
                */
                @event.Set();
            }).Start();

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
