using System.Linq;
using System.Reflection;
using RuntimeHelpers = System.Runtime.CompilerServices.RuntimeHelpers;
using System.Reflection.Emit;

namespace System.Runtime.CLR
{
    public static class MethodUtil
    {
        public static void ReplaceMethod(Delegate from, Delegate to, bool skip = false)
        {
            ReplaceMethod(from.Method, to.Method, skip);
        }

        /// <summary>
        /// Replaces the method.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="dest">The dest.</param>
        /// <param name="skip">Indicates that replacer will skip methods signatures verification</param>
        public static void ReplaceMethod(MethodBase source, MethodBase dest, bool skip = false)
        {
            if (!skip && !MethodSignaturesEqual(source, dest))
            {
                throw new ArgumentException("The method signatures are not the same.", "source");
            }
            ReplaceMethod(GetMethodAddress(source), dest);
        }

        /// <summary>
        /// Replaces the method.
        /// </summary>
        /// <param name="srcAdr">The SRC adr.</param>
        /// <param name="dest">The dest.</param>
        public static void ReplaceMethod(IntPtr srcAdr, MethodBase dest)
        {
            var destAdr = GetMethodAddress(dest);
            unsafe
            {
                if (IntPtr.Size == 8)
                {
                    var d = (ulong*)destAdr.ToPointer();
                    *d = *((ulong*)srcAdr.ToPointer());
                }
                else
                {
                    var d = (uint*)destAdr.ToPointer();
                    *d = *((uint*)srcAdr.ToPointer());
                }
            }
        }

        /// <summary>
        /// Gets the address of the method stub
        /// </summary>
        /// <param name="method">The method handle.</param>
        /// <returns></returns>
        public static IntPtr GetMethodAddress(MethodBase method)
        {
            if ((method is DynamicMethod))
            {
                return GetDynamicMethodAddress(method);
            }

            // Prepare the method so it gets jited
            RuntimeHelpers.PrepareMethod(method.MethodHandle);

            // If 3.5 sp1 or greater than we have a different layout in memory.
            if (IsNet20Sp2OrGreater())
            {
                return GetMethodAddress20SP2(method);
            }
            
            
            unsafe
            {
                // Skip these
                const int skip = 10;

                // Read the method index.
                var location = (UInt64*)(method.MethodHandle.Value.ToPointer());
                var index = (int)(((*location) >> 32) & 0xFF);

                if (IntPtr.Size == 8)
                {
                    // Get the method table
                    var classStart = (ulong*)method.DeclaringType.TypeHandle.Value.ToPointer();
                    var address = classStart + index + skip;
                    return new IntPtr(address);
                }
                else
                {
                    // Get the method table
                    uint* classStart = (uint*)method.DeclaringType.TypeHandle.Value.ToPointer();
                    uint* address = classStart + index + skip;
                    return new IntPtr(address);
                }
            }
        }

        private static IntPtr GetDynamicMethodAddress(MethodBase method)
        {
            unsafe
            {
                var handle = GetDynamicMethodRuntimeHandle(method);
                var ptr = (byte*)handle.Value.ToPointer();
                if (IsNet20Sp2OrGreater())
                {
                    RuntimeHelpers.PrepareMethod(handle);
                    if (IntPtr.Size == 8)
                    {
                        var address = (ulong*)ptr;
                        address = (ulong*)*(address + 5);
                        return new IntPtr(address + 12);
                    }
                    else
                    {
                        var address = (uint*)ptr;
                        address = (uint*)*(address + 5);
                        return new IntPtr(address + 12);
                    }
                }
                
                if (IntPtr.Size == 8)
                {
                    var address = (ulong*)ptr;
                    address += 6;
                    return new IntPtr(address);
                }
                else
                {
                    var address = (uint*)ptr;
                    address += 6;
                    return new IntPtr(address);
                }
            }
        }

        private static RuntimeMethodHandle GetDynamicMethodRuntimeHandle(MethodBase method)
        {
            if (method is DynamicMethod)
            {
                var fieldInfo = typeof(DynamicMethod).GetField("m_method",BindingFlags.NonPublic|BindingFlags.Instance);
                var handle = ((RuntimeMethodHandle)fieldInfo.GetValue(method));
                
                return handle;
            }
            return method.MethodHandle;
        }
        
        private static IntPtr GetMethodAddress20SP2(MethodBase method)
        {
            unsafe
            {
                return new IntPtr(((int*)method.MethodHandle.Value.ToPointer() + 2));
            }
        }
        
        private static bool MethodSignaturesEqual(MethodBase x, MethodBase y)
        {
            if (x.CallingConvention != y.CallingConvention)
            {
                return false;
            }
            Type returnX = GetMethodReturnType(x), returnY = GetMethodReturnType(y);
            if (returnX != returnY)
            {
                return false;
            }
            ParameterInfo[] xParams = x.GetParameters(), yParams = y.GetParameters();
            if (xParams.Length != yParams.Length)
            {
                return false;
            }

            return !xParams.Where((t, i) => t.ParameterType != yParams[i].ParameterType).Any();
        }
        private static Type GetMethodReturnType(MethodBase method)
        {
            var methodInfo = method as MethodInfo;
            if (methodInfo == null)
            {
                // Constructor info.
                throw new ArgumentException("Unsupported MethodBase : " + method.GetType().Name, "method");
            }
            return methodInfo.ReturnType;
        }
        private static bool IsNet20Sp2OrGreater()
        {
                return Environment.Version.Major == FrameworkVersions.Net20SP2.Major &&
                    Environment.Version.MinorRevision >= FrameworkVersions.Net20SP2.MinorRevision;
        }
    }
}
