using System;
using readingStructs;

namespace sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var safeptr1 = SafePtr.Create(new object());  // safeptr = ref to "Hello", IntPtr -> SyncBlockIndex.

            var entityptr = new EntityInfoPtr(safeptr1.IntPtr - IntPtr.Size);
            var mtintptr = entityptr.Reference.mt.MtPointer;

            var mtptr = new MethodTablePtr(mtintptr - IntPtr.Size).Reference.mt;

            Console.WriteLine("Contents: {0}, {1}", mtptr.Size, mtptr.MethodsCount);
        }
    }
}
