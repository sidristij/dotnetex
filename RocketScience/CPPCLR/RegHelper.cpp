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
	StackInfo copy;
    StackInfo* info;  

    // Save ALL registers
    _asm 
    {
        mov copy.EAX, EAX
        mov copy.EBX, EBX
        mov copy.ECX, ECX
        mov copy.EDX, EBX
        mov copy.EDI, EDI
        mov copy.ESI, ESI
        mov copy.EBP, EBP
        mov copy.ESP, ESP
        
		// Save CS:EIP for far jmp
        mov copy.CS, CS
        mov copy.EIP, offset Label0

		// Save mark for this method, from what place it was called
        push 0
    }
Label0:
    __asm pop info
        
    if(info != 0)
    {
        copy.EBP = info->EBP;
        __asm mov EBP, copy.EBP
        __asm mov ESP, EBP
        return 1;
    }

    //
    //  We need to copy stack part from our method to user code method including its locals in stack
    //
    int localsStart = copy.EBP;                                // our EBP points to EBP value for parent method
    int localsEnd = *(int *)*(int *)*(int *)*(int *)copy.EBP;  // points to end of user's method's locals /* TODO: not always correct */

    byte *arr = new byte[localsEnd - localsStart];
    memcpy(arr, (void*)localsStart, localsEnd - localsStart);

    
    // Get information about stack pages
    MEMORY_BASIC_INFORMATION *stackData = new MEMORY_BASIC_INFORMATION();            
    VirtualQuery((void *)copy.EBP, stackData, sizeof(MEMORY_BASIC_INFORMATION));

    // fill StackInfo structure
    info = new StackInfo(copy);
    info->ESP = copy.ESP;
    info->EBP = copy.EBP;
    info->EIP = copy.EIP;
    info->CS = copy.CS;

    info->EAX = copy.EAX;
    info->EBX = copy.EBX;
    info->ECX = copy.ECX;
    info->EDX = copy.EDX;
    info->EDI = copy.EDI;
    info->ESI = copy.ESI;
    
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
    StackInfo copy;
    short CS_EIP[3];
    
    // safe copy w-out changing registers
	for(int i=0; i<sizeof(StackInfo);i++)
		((byte *)&copy)[i] = ((byte *)stackCopy)[i];

    // Setup FWORD for far jmp
    *(int*)CS_EIP = copy.EIP;
    CS_EIP[2] = copy.CS;

	// calculate ranges
    int beg = (int)copy.frame;
    int size = copy.size;    
	int end = beg + size;
    int baseFrom = (int) copy.origStackStart;
    int baseTo = baseFrom + (int)copy.origStackSize;
    
    __asm mov copy.ESP, ESP

    // target = EBP[ - locals - EBP - ret - whole stack frames copy]
    int targetToCopy = copy.ESP - 8 - size;

    // offset between parent stack and current stack;
    int delta_to_target = (int)targetToCopy - (int)copy.EBP;

    // offset between parent stack start and its copy;
    int delta_to_copy = (int)copy.frame - (int)copy.EBP;

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
        push copy.EBX
        push copy.ECX
        push copy.EDX
        push copy.ESI
        push copy.EDI
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

