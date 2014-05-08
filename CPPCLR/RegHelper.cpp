#pragma unmanaged

#include <windows.h>
#include "CPPCLR.h"

extern "C" __declspec(dllexport)
void __stdcall MakeManagedThread();

public class StackInfo
{
public:
    int ESPr, EBPr, EIPr, stackSize, filled;
    void *stackData;
};

void RegHelper_Unmanaged::ForkPrepare(void *stackStart, int stackSize)
{
    // make stack copy
    void *copy = new byte[stackSize];

    memcpy_s(copy, stackSize, stackStart, stackSize);
    int ESPr, EBPr, EIPr;

    // make registers copy
    _asm {
        mov ESPr, ESP
        mov EBPr, EBP
    }

    StackInfo* info = new StackInfo();
    info->ESPr = ESPr;
    info->EBPr = EBPr;
    info->EIPr = EIPr;
    info->stackSize = stackSize;
    info->stackData = copy;
    info->filled = info->stackSize - ((int)&copy - (int)stackStart);

	stackInfo = info;

	MakeManagedThread();            
}

void RegHelper_Unmanaged::InForkedThread()
{
    StackInfo* info = stackInfo;
    int instack;

    // Get local thread stack info
    MEMORY_BASIC_INFORMATION* stackData = new MEMORY_BASIC_INFORMATION();            
    VirtualQuery(&instack, stackData, sizeof(MEMORY_BASIC_INFORMATION));
           
    // make stack size equal to parent thread stack size            
    int x;
    if(stackData->RegionSize - ((int)&x - (int)stackData->BaseAddress) < info->filled)
    {
        stackInfo = info;
        InForkedThread();
    }
    else
    {
        // copy all parent thread stack data to local stack and fix all pushed registers
        int size = info->stackSize - (info->ESPr - (int)info->stackData);
        memcpy((void *)((int)stackData->BaseAddress + stackData->RegionSize - size), 
                (void *)((int)info->stackData + info->stackSize - size), size);

        // offset between stack Begins
        int delta = ((int)stackData->BaseAddress + stackData->RegionSize) - ((int)info->stackData + info->stackSize);

        // fix EBPs in new stack
        for(int ptr = (int)&instack; ptr < (int)stackData->BaseAddress + stackData->RegionSize; ptr += 4  )
        {
            int value = *(int *)ptr - (int)info->stackData;
                
            if( value >= 0 && value < info->stackSize)
            {
                *(int *)ptr += delta;
            }
        }
    }
}

