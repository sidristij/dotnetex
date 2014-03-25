using System.Runtime.InteropServices;

namespace System.Runtime.CLR
{
	[StructLayout(LayoutKind.Explicit)]
	public struct EntityPtr2
	{
		[FieldOffset(0)]
		public object Object;
		
		[FieldOffset(4)]
		private int _handler;
		
		public EntityPtr2(int handler)
		{
			Object = null; _handler = 0;
			Handler = handler;
		}
		
		public EntityPtr2(IntPtr handler) : this(handler.ToInt32())
		{			
		}
		
		public EntityPtr2(object @object)
		{
			_handler = 0;
			Object = @object;
		}
		
		public unsafe int Handler
		{
			get {
				fixed(int *pp = &_handler)
				{
					return *(int *)((int)pp - sizeof(int)) - sizeof(int);
				}
			}
			
			set {
				fixed(int *pp = &_handler)
				{
					*(int *)((int)pp - sizeof(int)) = value + sizeof(int);
				}
			}
		}
		
		public static int ToHandler(object obj)
		{
			var entity = new EntityPtr2(obj);
			return entity.Handler;
		}
				
		public static T ToInstance<T>(int handler) where T:class
		{
			var entity = new EntityPtr2(handler);
			return (T)entity.Object;
		}
	}
}
