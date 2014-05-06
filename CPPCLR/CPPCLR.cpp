// This is the main DLL file.

#include "CPPCLR.h"

#pragma managed

namespace CPPCLR {

    public ref class RegHelper
    {
		 RegHelper_Unmanaged* helper;
    public:
		 void ForkPrepare(void *stackStart, int stackSize)
         {
			 helper = new RegHelper_Unmanaged();
			 helper->ForkPrepare(this, stackStart, stackSize);
		 }

		 void MakeThread()
		 {
			 Thread^ thread = gcnew Thread(gcnew ThreadStart(this, &InForkedThread));
             thread->Start();
		 }

		 void InForkedThread()
         {
			 helper->InForkedThread();
		 }
    };
}

