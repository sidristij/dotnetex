// CPPCLR.h

#pragma once
class StackInfo;

public class RegHelper_Unmanaged
{
private:	
	StackInfo* stackInfo;

public:
	void ForkPrepare(void *stackStart, int stackSize);
	void InForkedThread();
};