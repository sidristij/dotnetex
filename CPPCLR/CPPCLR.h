// CPPCLR.h

#pragma once

using namespace System;

#pragma unmanaged

inline void ReadRegisters(int *ESPr)
{
	int tESPr;
	__asm {
		mov tESPr, ESP
	}
	*ESPr = tESPr;
}

inline void WriteRegisters(int ESPr)
{
	ESPr += 8;
	__asm {
		mov ESP, ESPr
	}
}

void MemcpyAndSetInternal(int dest, int src, int num, int ESPr)
{
	ESPr += 8 + 12;
	__asm {
		ESP = ESPr;
	}

	for(int i = 0; i < num; i++)
	{
		*(char *)(dest + i) = *(char *)(src + i); 
	}

}

#pragma managed

namespace CPPCLR {


	public ref class RegHelper
	{
	public:
		void Read(int %esp)
		{
			int espr;
			ReadRegisters(&espr);
			esp = espr;
		}

		void Write(int esp)
		{
			WriteRegisters(esp);
		}

		void MemcpyAndSet(int dest, int src, int num, int esp)
		{
			MemcpyAndSetInternal(dest, src, num, esp);
		}
	};
}
