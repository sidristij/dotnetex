// CPPCLR.h

#pragma once
class StackInfo;

//
//  This is C++ stuff. In our XXI centry it needs to have h files included to each project with all external types -)
//
public class AdvancedThreading_Unmanaged
{	
public:
	int ForkImpl();
	void InForkedThread(StackInfo * stackCopy);
};