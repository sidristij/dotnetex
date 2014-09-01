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

    public:
        
        [MethodImpl(MethodImplOptions::NoInlining)]
        static bool CloneThread()
        {
            ManualResetEvent^ resetEvent = gcnew ManualResetEvent(false);
            AdvancedThreading_Unmanaged *helper = new AdvancedThreading_Unmanaged();

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

		ref class ForkData
		{
		public:
			AdvancedThreading_Unmanaged *helper;
			StackInfo *info;
		};

        static void MakeThread(AdvancedThreading_Unmanaged *helper, StackInfo *stackCopy)
        {
			ForkData^ data = gcnew ForkData();
			data->helper = helper;
			data->info = stackCopy;

            ThreadPool::QueueUserWorkItem(gcnew WaitCallback(&InForkedThread), data);            
        }
         
        static void InForkedThread(Object^ state)
        {
			ForkData^ data = (ForkData^) state;
			data->helper->InForkedThread(data->info);
        }
    };
}

//
//  Special wapper to enable managed method call from unmanaged method
//
extern "C" __declspec(dllexport)
void __stdcall MakeManagedThread(AdvancedThreading_Unmanaged *helper, StackInfo *stackCopy) 
{
    AdvancedThreading::Fork::MakeThread(helper, stackCopy);
}


