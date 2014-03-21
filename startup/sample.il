// Metadata version: v2.0.50215
.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 2:0:0:0
}
.assembly sample
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
// Image base: 0x02F20000


// =============== CLASS MEMBERS DECLARATION ===================

.class public auto ansi abstract sealed beforefieldinit sandbox.PtrConverter
	extends [mscorlib]System.Object
{
        .permissionset permitonly = {
		[mscorlib]System.Security.Permissions.SecurityPermissionAttribute = {
			property bool SkipVerification = bool(true)
		}
	}
	
	// Fields
	.field private static native int 'field'

	// Methods
	.method public hidebysig static 
		native int Convert (
			object input
		) cil managed 
	{
		// Method begins at RVA 0x2254
		// Code size 7 (0x7)
		.maxstack 1
		.locals init (
			[0] native int CSS
		)

		IL_0000: nop
		IL_0001: ldarg.0
		IL_0006: ret
	} // end of method Converter::Convert

} // end of class sandbox.Converter