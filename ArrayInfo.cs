/*
 * Created by SharpDevelop.
 * User: SSidristy
 * Date: 01.03.2014
 * Time: 10:57
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace CLREx
{
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct ArrayInfo
	{
		[FieldOffset(0)]
		public EntityInfo BasicInfo;
		
		[FieldOffset(8)]
		private int Lengthes;
				
		public bool IsMultidimentional
		{
			get 
			{
				return (BasicInfo.MethodTable->Flags & MethodTableFlags.IfArrayThenSzArray) == 0;
			}
		}
	
		public bool IsValueTypes
		{
			get 
			{
				return (BasicInfo.MethodTable->Flags & MethodTableFlags.IfArrayThenSharedByReferenceTypes) == 0;
			}
		}
	
		public unsafe int Dimensions
		{
			get 
			{
				if(IsMultidimentional)
				{
					unsafe {
						fixed(int *cur = &Lengthes)
						{
							int count = 0;
							while(cur[count] != 0) count++;
							return count;
						}
					}
				}
				else
				{
					return 1;				
				}
			}
		}
		
		public unsafe int GetLength(int dim)
		{
			int max_dim = Dimensions;
			if(max_dim < dim)
				throw new ArgumentOutOfRangeException("dim");
			
			unsafe {
				fixed(int *addr = &Lengthes)
				{
					return addr[dim];
				}
			}
		}
		
		public unsafe int SizeOf()
		{
			int total = 0;
			
			int elementsize = 0;
			unsafe {
				fixed(EntityInfo *entity = &BasicInfo)
				{
					var pp = new GCEx.EntityPtr { Pointer = (int)entity };
					var arr = pp.Object as Array;
					if(IsValueTypes)
					{
						var elementType = arr.GetType().GetElementType();
						var typecode = Type.GetTypeCode(elementType);
						
						switch(typecode)
						{
							case TypeCode.Byte:
							case TypeCode.SByte:
							case TypeCode.Boolean:
								elementsize = 1;
								break;
							case TypeCode.Int16:
							case TypeCode.UInt16:
							case TypeCode.Char:
								elementsize = 2;
								break;
							case TypeCode.Int32:
							case TypeCode.UInt32:
							case TypeCode.Single:
								elementsize = 4;
								break;
							case TypeCode.Int64:
							case TypeCode.UInt64:
							case TypeCode.Double:
								elementsize = 8;
								break;
							case TypeCode.Decimal:
								elementsize = 12;
								break;
							default:
								var info = (MethodTableInfo *)elementType.TypeHandle.Value;
								elementsize = (int)info->Size - sizeof(EntityInfo);
								break;
						}						
					}
					else 
					{
						elementsize = IntPtr.Size;
					}
				}
			}
			// Header
			total += sizeof(EntityInfo);
			total += IsValueTypes ? 0 : 4; // MethodsTable for refTypes
			total += IsMultidimentional ? Dimensions * 8 : 4; 
			
			// Contents
			if(!IsMultidimentional)
			{
				total += (Lengthes)*elementsize;
			}
			else
			{
				var res = 1;
				for(int i = 1, len = Dimensions; i < len; i++)
				{
					res *= GetLength(i);
				}
				
				total += res * elementsize;
			}
			
			// align size to IntPtr
			if((total & 3) != 0) total += 4 - total % 4;
						
			return total;
		}
	}
}
