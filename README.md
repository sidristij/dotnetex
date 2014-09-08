dotnetex
========

Gets size of .Net Framework objects, can change type of object to incompatible and can alloc .Net objects at unmanaged memory area
Also contains examples to resolve .Net objects in virtual memory and fork() method.

```csharp
   var size = GCex.SizeOf<Object>();    // prints 12 on 32-bit .Net Framework;
   var size = GCEx.SizeOf(someObject);  // prints size for already existing object;
```
> SizeOf allows to compute size of any .Net type, including reference types, strings, arrays and so on.

```csharp
  var from = new object();
  
  callSomeMethodOrJustCodeBlock();
  
  GCEx.EnumerateSOH(from).Select(obj => GCEx.SizeOf(obj)).Sum()
```
> will compute sum of objects, which are allocated by callSomeMethodOrJustCodeBlock();

```csharp
  static void MakeFork()
  {
      var sameLocalVariable = 123;
      var stopwatch = Stopwatch.StartNew();

      // Splitting current thread flow to two threads
      var forked = Fork.CloneThread();

      lock (Sync)
      {
          Console.WriteLine("in {0} thread: {1}, local value: {2}, time to enter = {3} ms",
              forked ? "forked" : "parent",
              Thread.CurrentThread.ManagedThreadId,
              sameLocalVariable, 
              stopwatch.ElapsedMilliseconds);
      }

      // Here forked thread's life will be stopped
      joined.Signal();
  }
```

Will make new thread, which will start its flow on Fork.CLone() call and will stop on caller's method exit.

```csharp
  var heap = new UnmanagedHeap<Foo>(100);
  var obj = heap.Allocate();
  
  obj.CallMethod();
  
  heap.Free(obj);
```
> Will create objects pool in unmanaged memory. Object will have type 'Foo' and pool's size will be 100. Performance tests for this pool:
```
Ctor call via reflection (on already allocated memory): 49575
Ctor call via method body ptr redirection: 1147
pure allocation in managed memory: 6162
Refl ctor call / ctor Redirection 43,22145 (higher is slower)   <--- 43 times faster than constructorInfo.Invoke()
ctor Redirection / newobj:        0,1861409 (higher is slower)  <--- 5 times faster than new T(int ..)
Refl ctor call / newobj:          8,045278 (higher is slower)   <--- using reflection is just 8 times faster than new T(int ...);
```

# Bugs

Please feel free to add new bugs to our issue tracker (github's tracker)
