// CPPCLR.h

#pragma once

public class StackInfo
{
public:
    int EAX, EBX, ECX, EDX;
    int EDI, ESI;
    int ESP;
    int EBP;
    int EIP;
    short CS;
    void *frame;
    int size;
    int origStackStart, origStackSize;
};

//
//  This is C++ stuff. In our XXI centry it needs to have h files included to each project with all external types -)
//
public class AdvancedThreading_Unmanaged
{	
public:
	int ForkImpl();
	void InForkedThread(StackInfo * stackCopy);
};