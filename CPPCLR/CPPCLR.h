// CPPCLR.h

#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Threading;

#pragma unmanaged

class RegHelper_Unmanaged
{
    public:
		void ForkPrepare(CPPCLR::RegHelper^ helper, void *stackStart, int stackSize);
		void InForkedThread();
};

#pragma managed


