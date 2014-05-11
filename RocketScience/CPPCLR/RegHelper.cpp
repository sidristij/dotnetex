#pragma unmanaged

#include <windows.h>
#include "CPPCLR.h"

extern "C" __declspec(dllexport)
void __stdcall MakeManagedThread();

public class StackInfo
{
public:
    int ESP;
    int EBP;
    int EIP;
    int stackSize;
    int origStackStart;
    int origStackSize;
    short CS;
    void *stackData;
};

volatile static StackInfo* stackInfo;

int AdvancedThreading_Unmanaged::ForkImpl()
{
    void *stackStart;                     // 1 word
    int stackSize;				          // 1 words
    void *copy;                           // 1 word
    int ESPr, EBPr, EIPr;                 // 3 words
    int short CSr;
    StackInfo* info;                      // 1 word
    MEMORY_BASIC_INFORMATION* stackData;  // 1 word
    int isforked;

    // ESP = EBP + 8 * wordSize = EBP + 256 on 32-bit

    // Save stack registers
    _asm 
    {
        mov EBPr, EBP
        mov ESPr, ESP

        push 0
        mov CSr,  CS
        mov EIPr, offset Label0
    }
Label0:
    _asm
    {
        pop  isforked
    }

    if(isforked)
    {		
        return 1;
    }

    stackStart = (void *) EBPr;

    // Get information about stack pages
    stackData = new MEMORY_BASIC_INFORMATION();            
    VirtualQuery(stackStart, stackData, sizeof(MEMORY_BASIC_INFORMATION));
    
    // Calculating stack end
    stackSize = stackData->RegionSize - ((int)stackStart - (int)stackData->BaseAddress);
    
    // make stack copy
    copy = new byte[stackSize];
    memcpy_s(copy, stackSize, stackStart, stackSize);
    
    // fill StackInfo structure
    info = new StackInfo();
    info->ESP = ESPr;
    info->EBP = EBPr;
    info->CS  = CSr;
    info->EIP = EIPr;
    
    info->origStackStart = (int)stackData->BaseAddress;
    info->origStackSize = (int)stackData->RegionSize;

    info->stackSize = stackSize;
    info->stackData = copy;
    stackInfo = info;

    // call managed new Thread().Start() to make fork
    MakeManagedThread(); 
}

/***
 *
 *  InForkedThread uses variable parameters count feature to 
 *
 */
void AdvancedThreading_Unmanaged::InForkedThread(int isLastCall, int parentCallStackStart, ...)
{
    void *stackStart;                     // 1 word
    int stackEnd;                         // 1 word
    int stackSize;				          // 1 words
    unsigned short CS_EIP[3];

    _asm {
        mov stackStart, EBP
    }
    
    // Get local thread stack info
    MEMORY_BASIC_INFORMATION* stackData = new MEMORY_BASIC_INFORMATION();            
    VirtualQuery(stackStart, stackData, sizeof(MEMORY_BASIC_INFORMATION));

    // Calculate size
    stackSize = stackData->RegionSize - ((int)stackStart - (int)stackData->BaseAddress);
    stackEnd = (int)stackData->BaseAddress + stackData->RegionSize;
    
    if(!isLastCall)
    {
        if(parentCallStackStart == 0)
        {
            InForkedThread(0, (int)stackStart);
        }
        else
        {
            int realStackSize = stackInfo->stackSize;
            int realStackStart = stackEnd - realStackSize;

                int callCost = parentCallStackStart - (int)stackStart;
                int growSize = realStackSize - stackSize;
                int toAdd = (growSize - callCost) >> 2; // size in words
                int stackSize = toAdd << 2;

                // add alignment
                for (int i=0; i < toAdd; i++)
                    __asm push 123

                // call to move EBP/ESP by compiler
                InForkedThread(1, toAdd);

                // remove parameters from stack. Actually will never be called
                __asm add ESP, toAdd
        }
    }
        else 
        {		
            // copy all parent thread stack data to local stack and fix all pushed registers
            int size = stackInfo->stackSize;

            // offset between parent stack copy and current stack;
            int delta = (int)stackStart - (int)stackInfo->EBP;

            // offset between parent stack start and its copy;
            int delta_original = (int)stackInfo->stackData - (int)stackInfo->EBP;

            short CSr = stackInfo->CS;
            int EIPr = stackInfo->EIP;

            // Setup CS:EIP 6-byte structure to make far jmp
            *(int *)CS_EIP = EIPr;
            CS_EIP[2] = CSr;
        
            int ebp_cur;

            // calculate ranges
            int beg = (int)stackInfo->stackData;
            int end = beg + stackInfo->stackSize;
            int baseFrom = (int) stackInfo->origStackStart;
            int baseTo = baseFrom + (int)stackInfo->origStackSize;

            // In stack copy we have many saved EPBs, which where actually one-way linked list.
            // we need to fix copy to make these pointers correct for our thread's stack.
            ebp_cur = beg;
            while(true)
            {
                int val = *(int*)ebp_cur;

                if(baseFrom <= val && val < baseTo)
                {
                    int localOffset = val + delta_original;
                    *(int *)ebp_cur += delta;
                    ebp_cur = localOffset;
                } 
                else 
                    break;
            }
        
            // Replace our stack with fixed parent stack copy
            // We couldn't call memcpy to avoid touching stack
            for(int i = 0; i < size; i++)
            {
                ((byte *)stackStart)[i] = ((byte *)stackInfo->stackData)[i];
            };

            // jmp to Fork method (see below)
            _asm {
                push 1
                jmp fword ptr [CS_EIP]
            }
        }
    }
}

