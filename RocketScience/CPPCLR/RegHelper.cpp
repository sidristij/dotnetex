#pragma unmanaged

#include <windows.h>
#include "CPPCLR.h"

extern "C" __declspec(dllexport)
	void __stdcall MakeManagedThread(StackInfo * info);

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

int AdvancedThreading_Unmanaged::ForkImpl()
{
    int ESPr, EBPr, EIPr;                 // 3 words
    int EAXr, EBXr, ECXr, EDXr;
    int EDIr, ESIr;
    short CSr;
    StackInfo* info;  

    // Save ALL registers
    _asm 
    {
        mov EAXr, EAX
        mov EBXr, EBX
        mov ECXr, ECX
        mov EDXr, EBX
        mov EDIr, EDI
        mov ESIr, ESI
        mov EBPr, EBP
        mov ESPr, ESP
        
    // Save CS:EIP for far jmp
        mov CSr, CS
        mov EIPr, offset Label0

    // Save mark for this method, from what place it was called
        push 0
    }
Label0:
    __asm pop info
        
    if(info != 0)
    {
        EBPr = info->EBP;
        __asm mov EBP, EBPr
        __asm mov ESP, EBP
        return 1;
    }


    //
    //  We need to copy stack part from our method to user code method including its locals in stack
    //
    int localsStart = EBPr;                                // our EBP points to EBP value for parent method
    int localsEnd = *(int *)*(int *)*(int *)*(int *)EBPr;  // points to end of user's method's locals /* TODO: not always correct */

    byte *arr = new byte[localsEnd - localsStart];
    memcpy(arr, (void*)localsStart, localsEnd - localsStart);

    
    // Get information about stack pages
    MEMORY_BASIC_INFORMATION *stackData = new MEMORY_BASIC_INFORMATION();            
    VirtualQuery((void *)EBPr, stackData, sizeof(MEMORY_BASIC_INFORMATION));

    // fill StackInfo structure
    info = new StackInfo();
    info->ESP = ESPr;
    info->EBP = EBPr;
    info->EIP = EIPr;
    info->CS = CSr;

    info->EAX = EAXr;
    info->EBX = EBXr;
    info->ECX = ECXr;
    info->EDX = EDXr;
    info->EDI = EDIr;
    info->ESI = ESIr;
    
    info->origStackStart = (int)stackData->BaseAddress;
    info->origStackSize = (int)stackData->RegionSize;

    info->frame = arr;
    info->size = (localsEnd - localsStart);

    // call managed new Thread().Start() to make fork
    MakeManagedThread(info); 

    return 0;
}

/***
 *
 *  InForkedThread uses variable parameters count feature to 
 *
 */
void AdvancedThreading_Unmanaged::InForkedThread(StackInfo * stackCopy)
{
    int EBPr, ESPr;   			          
    int EAXr, EBXr, ECXr, EDXr;
    int EDIr, ESIr;
    short CS_EIP[3];
    

    void * frame = stackCopy->frame;
    int size = stackCopy->size;

    // Setup FWORD for far jmp
    *(int*)CS_EIP = stackCopy->EIP;
    CS_EIP[2] = stackCopy->CS;

    // localize registers values
    EAXr = stackCopy->EAX;
    EBXr = stackCopy->EBX;
    ECXr = stackCopy->ECX;
    EDXr = stackCopy->EDX;
    EDIr = stackCopy->EDI;
    ESIr = stackCopy->ESI;
    
    // calculate ranges
    int beg = (int)stackCopy->frame;
    int end = beg + stackCopy->size;
    int baseFrom = (int) stackCopy->origStackStart;
    int baseTo = baseFrom + (int)stackCopy->origStackSize;
    
    __asm mov ESPr, ESP

    // target = EBP[ - locals - EBP - ret - whole stack frames copy]
    int targetToCopy = ESPr - 8 - stackCopy->size;

    // offset between parent stack and current stack;
    int delta_to_target = (int)targetToCopy - (int)stackCopy->EBP;

    // offset between parent stack start and its copy;
    int delta_to_copy = (int)stackCopy->frame - (int)stackCopy->EBP;

    // In stack copy we have many saved EPBs, which where actually one-way linked list.
    // we need to fix copy to make these pointers correct for our thread's stack.
    int ebp_cur = beg;
    while(true)
    {
        int val = *(int*)ebp_cur;

        if(baseFrom <= val && val < baseTo)
        {
            int localOffset = val + delta_to_copy;
            *(int *)ebp_cur += delta_to_target;
            ebp_cur = localOffset;
        } 
        else 
            break;
    }

    // for ret
    
    __asm push offset Label0
    __asm push EBP
    
    for(int i = (size >> 2) - 1; i >= 0; i--)
    {
        int val = ((int *)beg)[i];
        __asm push val;
    };
    
    stackCopy->EBP = targetToCopy;

    // restore registers, push 1 for Fork() and jmp
    _asm {        
        push EBXr
        push ECXr
        push EDXr
        push ESIr
        push EDIr
        pop EDI
        pop ESI
        pop EDX
        pop ECX
        pop EBX
                
        push stackCopy
        jmp fword ptr CS_EIP
    }

Label0:

    return;
 }

