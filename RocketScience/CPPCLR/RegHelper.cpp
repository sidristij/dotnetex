#pragma unmanaged

#include <windows.h>
#include "CPPCLR.h"

extern "C" __declspec(dllexport)
    void __stdcall MakeManagedThread(AdvancedThreading_Unmanaged *helper, StackInfo * info);


#define CHECKREF(REG) if((info->REG >= stackStart) && (info->REG <= stackEnd)) info->REG += delta;

void AdvancedThreading_Unmanaged::FixReferences(StackInfo *info, int delta)
{
    int stackStart = info->origStackStart;
    int stackEnd = stackStart + info->origStackSize;
    
    // We need to investigate, what registers are used in what cases for all .Net versions and fix only registers we need.
    CHECKREF(EAX);
    CHECKREF(EBX);
    CHECKREF(ECX);
    CHECKREF(EDX);

    CHECKREF(ESI);
    CHECKREF(EDI);
}

int AdvancedThreading_Unmanaged::ForkImpl()
{
    int threadpool = 0;
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
        __asm
		{
			mov EBP, copy.EBP
            mov ESP, EBP

			// return 1 emulation
			pop EBP
			mov EAX, 1
			ret
		}
        return 1;  // can be changed to jmp by compiler
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
    info->origStackStart = (int)stackData->BaseAddress;
    info->origStackSize = (int)stackData->RegionSize;
    info->frame = arr;
    info->size = (localsEnd - localsStart);

    // call managed new Thread().Start() to make fork
    MakeManagedThread(this, info); 

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

	// Save original registers to restore
	_asm  
	{
		push EAX
		push EBX
		push ECX
		push EDX

		push ESI
		push EDI
	}

    // safe copy w-out changing registers
    for(int i = 0; i < sizeof(StackInfo); i++)
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

    // Fix stack pointers in registers (TODO: may change cached integers, which values are in stack address space range)
    FixReferences(stackCopy, delta_to_target);

    // restore registers, push 1 for Fork() and jmp
    _asm {        
		push copy.EAX
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
		pop EAX
                
        push stackCopy
        jmp fword ptr CS_EIP
    }

Label0:
    
	// Restore original registers
	_asm  
	{
		pop EDI
		pop ESI

		pop EDX
		pop ECX
		pop EBX
		pop EAX
	}

    return;
 }

