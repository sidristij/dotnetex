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
        
        [MethodImpl(MethodImplOptions::NoInlining | MethodImplOptions::NoOptimization | MethodImplOptions::PreserveSig)]
        static bool CloneThread()
        {
            ManualResetEvent^ resetEvent = gcnew ManualResetEvent(false);
            AdvancedThreading_Unmanaged *helper = new AdvancedThreading_Unmanaged();
            int somevalue;

            // additionally we pass current stack top address to calculate # of frames to save
            helper->stacktop = (int)(int *)&somevalue;
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

        [MethodImpl(MethodImplOptions::NoInlining | MethodImplOptions::NoOptimization | MethodImplOptions::PreserveSig)]
        static void MakeThread(AdvancedThreading_Unmanaged *helper, StackInfo *stackCopy)
        {
			ForkData^ data = gcnew ForkData();
			data->helper = helper;
			data->info = stackCopy;

            ThreadPool::QueueUserWorkItem(gcnew WaitCallback(&InForkedThread), data);            
        }
         
		[MethodImpl(MethodImplOptions::NoInlining | MethodImplOptions::NoOptimization | MethodImplOptions::PreserveSig)]
		static void InForkedThread(Object^ state)
        {
			ForkData^ data = (ForkData^) state;
			data->helper->InForkedThread(data->info);
        }

    private:

        static int calldeep;
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


