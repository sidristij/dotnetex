namespace VirtualMemorySample
{
    using System;
    using System.Runtime.CLR;
    
    static class Program
    {
        private static readonly IntPtr StringsTable;
    
        static Program()
        {
            StringsTable = typeof(string).TypeHandle.Value;
        }

        static void Main()
        {
            IntPtr heapsOffset, lastHeapByte;

            Console.WriteLine("This proram enumerates all available strings in last ephemeral segment");

            MemAccessor.GetManagedHeap(out heapsOffset, out lastHeapByte);

            // looking up structures
            if (heapsOffset != IntPtr.Zero)
            {
                EnumerateStrings(heapsOffset, lastHeapByte);
            }
            Console.ReadKey();
        }

        /// <summary>
        /// Enumerates all strings in heap
        /// </summary>
        /// <param name="heapsOffset">Heap starting point</param>
        /// <param name="lastHeapByte">Heap last byte</param>
        private static void EnumerateStrings(IntPtr heapsOffset, IntPtr lastHeapByte)
        {
            var count = 0;
            
            for (long pointer = heapsOffset.ToInt64(), end = lastHeapByte.ToInt64(); pointer < end; pointer++)
            {
                if (IsString(pointer))
                {
                    var str = EntityPtr.ToInstance<string>(new IntPtr(pointer));
                    Console.WriteLine(str);
                    count++;
                }
            }

            Console.WriteLine("Total count: {0}", count);
        }

        /// <summary>
        /// Checks, is given memory address contain data, which can be recognized as .Net string by template:
        /// | sync=0 | mtdtbl=known | length | string with 'length' length | zero |
        /// TODO: test it on x64 and fix formulas
        /// </summary>
        private static unsafe bool IsString(long pointer)
        {
            var mtptr = pointer + IntPtr.Size;

            if (GCEx.PointsToAllocated((IntPtr)pointer) 
                && *(IntPtr*)pointer == IntPtr.Zero
                && *(IntPtr*)mtptr == StringsTable)
            {
                if (GCEx.MajorNetVersion >= 4)
                {
                    var length = *(int*) ((int) pointer + 8);
                    if (length < 2048 && *(short*)((int)pointer + 12 + length * 2) == 0)
                    {
                        return true;
                    }
                }
                else
                {
                    var length = *(int*)((int)pointer + 12);
                    if (length < 2048 && *(short*)((int)pointer + 14 + length * 2) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
