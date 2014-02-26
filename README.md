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
