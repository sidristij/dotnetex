// Metadata version: v2.0.50215
.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 2:0:0:0
}
.assembly EntityPtr

{
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = ( 01 00 08 00 00 00 00 00 ) 
  .hash algorithm 0x00008004
  .ver 1:0:0:0
}


.module sample.exe
// MVID: {A224F460-A049-4A03-9E71-80A36DBBBCD3}
.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY

// =============== CLASS MEMBERS DECLARATION ===================

.class public auto ansi abstract sealed beforefieldinit System.Runtime.CLR.EntityPtr
	extends [mscorlib]System.Object
{               	
	// Methods
	.method public hidebysig static 
		native int ToPointer<TType> (
			!!TType input
		) cil managed 
	{
		// Method begins at RVA 0x2254
		// Code size 7 (0x7)
		.maxstack 2
		.locals init (
			[0] native int CSS
		)

		ldarg.0
		conv.i4
		ldc.i4 4
		sub
		conv.i
		ret
	} // end of method Converter::ToPointer

	// Methods
	.method public hidebysig static 
		!!TType ToInstance<TType> (
			native int input
		) cil managed 
	{
		// Method begins at RVA 0x2254
		// Code size 7 (0x7)
		.maxstack 2
		.locals init (
			[0] native int CSS
		)

		ldarg.0
		conv.i4
		ldc.i4 4
		add
		conv.i
		ret
	} // end of method Converter::ToInstance

} // end of class sandbox.Converter