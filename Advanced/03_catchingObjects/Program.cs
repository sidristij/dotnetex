namespace CatchingObjectsSample
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CLR;

    internal class Program
    {
        private static readonly IntPtr MscorlibModule;

        static unsafe Program()
        {
            MscorlibModule = (IntPtr) ((MethodTableInfo*) typeof (string).TypeHandle.Value)->ModuleInfo;
        }

        private static unsafe void Main()
        {
            var objects = new Dictionary<Type, int>(7000);

            // Get current heap ranges
            IntPtr managedStart, managedEnd;

            Console.ReadKey();
            MemAccessor.GetManagedHeap(out managedStart, out managedEnd);

            // for each byte in virtual memory block, we trying to find strings
            var stopwatch = Stopwatch.StartNew();
            for (IntPtr* ptr = (IntPtr*) managedStart, end = (IntPtr*) managedEnd; ptr < end; ptr++)
            {
                if (IsCorrectMethodsTable(*ptr))
                {
                    // checking next object.
                    int size;
                    try
                    {
                        size = GCEx.SizeOf((EntityInfo*) (ptr - 1)) >> 2;
                    }
                    catch (OverflowException)
                    {
                        continue;
                    }

                    if (ptr + size > (long*) managedEnd)
                        continue;

                    {
                        var found = EntityPtr.ToInstance<object>((IntPtr) (ptr - 1));
                        RegisterObject(objects, found);

                        var lastInChain = found;
                        foreach (var item in GCEx.GetObjectsInSOH(found, hmt => IsCorrectMethodsTable((IntPtr) hmt)))
                        {
                            RegisterObject(objects, item.Item);
                            if (!item.IsArrayItem) lastInChain = item.Item;
                        }

                        long lastRecognized = (long) EntityPtr.ToPointer(lastInChain);
                        ptr = (IntPtr*) (lastRecognized + lastInChain.SizeOf());
                    }
                }
            }

            var timeToTakeSnapshot = stopwatch.ElapsedMilliseconds;

            foreach (var type in objects.Keys.OrderByDescending(key => objects[key]))
            {
                Console.WriteLine("{0:00000} : {1}", objects[type], type.FullName);
            }

            Console.WriteLine("Objects total: {0}. Time taken: {1}", objects.Values.Sum(), timeToTakeSnapshot);
            Console.ReadKey();
        }

        private static unsafe bool IsCorrectMethodsTable(IntPtr mt)
        {
            if (mt == IntPtr.Zero) return false;

            if (PointsToAllocated(mt))
                if (PointsToAllocated((IntPtr) ((MethodTableInfo*) mt)->EEClass))
                    if (PointsToAllocated((IntPtr) ((MethodTableInfo*) mt)->EEClass->MethodsTable))
                        return ((IntPtr) ((MethodTableInfo*) mt)->EEClass->MethodsTable == mt) ||
                               ((IntPtr) ((MethodTableInfo*) mt)->ModuleInfo == MscorlibModule);

            return false;
        }

        private static bool PointsToAllocated(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero) return false;
            return !WinApi.IsBadReadPtr(ptr, 32);
        }

        private static void RegisterObject(Dictionary<Type, int> dict, object obj)
        {
            var type = obj.GetType();
            if (!dict.ContainsKey(type))
            {
                dict[type] = 0;
            }

            dict[type] = dict[type] + 1;
        }
    }
}
