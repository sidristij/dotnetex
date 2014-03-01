dotnetex
========

Gets size of .Net Framework objects, can change type of object to incompatible and can alloc .Net objects at unmanaged memory area

How to use it:

    var obj = GCEx.AllocInUnmanaged<SomeClass>();
    
Allocates managed object in unmanaged memory area. But right now without calling constructor.

    var size = GCEx.SizeOf<SomeClass>();
    
Gets size of managed object in memory.

    var obj = new object();
    var syncblock = obj.GetSyncBlockIndex();
    
Gets syncblockindex from object.

    var obj1 = new object();
    var obj2 = new object();
    obj1.SetSyncBlockIndex(obj2.GetSyncBlockIndex());
    
Sets syncblockindex of `obj1` to syncblockindex of `obj2`

    class A { ... }
    class B { ... }
    
    var a = new A();
    var b = new B();
    
    a.SetMethodTable(b.GetMethodTable());
    
Makes object of type A as type B.

Following code will enumerate all objects in SOH (Small Objects Heap) between two objects, which passed via params:


    object a = new object();
				
    List<object> objects = new List<object>();
				
    for(int i = 0; i < 100000; i++)
    	objects.Add(new object());
				
    long count = 0, cursize = 0, size = 0;
				
    foreach(var cur in GCEx.GetObjectsInSOH(a))
    {
        cursize = GCEx.SizeOf(cur);
        size += cursize;					
        count++;
    }
    Console.WriteLine(" - sum: {0}, count: {1}", size, count);

`cur` is object and can be casted to its Type
Program outputs:

     - sum: 1331328, count: 100017
     
Of course, some objects, like arrays, which have lost roots can be GC collected before our loop.
