// This is the main DLL file.

#pragma managed(push, off)

#include "CPPCLR.h"

#pragma managed(pop)

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Threading;

namespace AdvancedThreading 
{

    static public ref class Fork
    {
         // for unmanaged work with stack and so on
         static AdvancedThreading_Unmanaged* helper;
		 static ManualResetEvent^ resetEvent;

    public:
         
         static bool CloneThread()
         {
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
             Thread^ thread = gcnew Thread(gcnew ThreadStart(&InForkedThread));
             thread->Start();
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


