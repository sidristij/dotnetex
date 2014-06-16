// This is the main DLL file.

#pragma managed(push, off)

#include "CPPCLR.h"

#pragma managed(pop)

using namespace System;
using namespace System::Runtime::CompilerServices;
using namespace System::Runtime::InteropServices;
using namespace System::Threading;

namespace AdvancedThreading 
{

    static public ref class Fork
    {
        // for unmanaged work with stack and so on
        static AdvancedThreading_Unmanaged* helper;
        static ManualResetEvent^ resetEvent;
        static bool scheduleInThreadPool;

    public:
        
        [MethodImpl(MethodImplOptions::NoInlining)]
        static bool CloneThread([DefaultParameterValue(false)] bool threadpool)
        {
            scheduleInThreadPool = threadpool;
            resetEvent = gcnew ManualResetEvent(false);
            helper = new AdvancedThreading_Unmanaged();
            bool forked = helper->ForkImpl();
            if(!forked)
            {
                resetEvent->WaitOne();
            } else
            {
                resetEvent->Set();
            }
            return forked;
        }

    internal:

        static void MakeThread()
        {
            if(scheduleInThreadPool)
            {
                ThreadPool::QueueUserWorkItem(gcnew WaitCallback(&InForkedThreadPool));
            }
            else
            {
                Thread^ thread = gcnew Thread(gcnew ThreadStart(&InForkedThread));
                thread->Start();
            }
        }
         
        static void InForkedThreadPool(Object^ state)
        {
            helper->InForkedThread();
        }
         
        static void InForkedThread()
        {
            helper->InForkedThread();
        }
    };
}

//
//  Special wapper to enable managed method call from unmanaged method
//
extern "C" __declspec(dllexport)
void __stdcall MakeManagedThread() 
{
    AdvancedThreading::Fork::MakeThread();
}


