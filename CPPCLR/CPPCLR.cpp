// This is the main DLL file.

#pragma managed(push, off)

#include "CPPCLR.h"

#pragma managed(pop)

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Threading;

namespace CPPCLR {

	static public ref class RegHelper
    {
         // for unmanaged work with stack and so on
         static RegHelper_Unmanaged* helper;

    public:
         static void ForkPrepare(void *stackStart, int stackSize)
         {
             helper = new RegHelper_Unmanaged();
             helper->ForkPrepare(stackStart, stackSize);
         }

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

extern "C" __declspec(dllexport)
void __stdcall MakeManagedThread() 
{
	CPPCLR::RegHelper::MakeThread();
}


