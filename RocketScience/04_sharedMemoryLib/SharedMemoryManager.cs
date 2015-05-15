using System;
using System.ComponentModel;
using System.Runtime.CLR;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;

/// <summary>
/// Class for sending objects through shared memory using a mutex
/// to synchronize access to the shared memory
/// </summary>
public class SharedMemoryManager<TransferItemType> : IDisposable
{
    #region Consts
    const int INVALID_HANDLE_VALUE = -1;
    const int FILE_MAP_WRITE = 0x0002;
    /// <summary>
    /// Define from Win32 API.
    /// </summary>
    const int ERROR_ALREADY_EXISTS = 183;
    #endregion

    #region Private members
    IntPtr handleFileMapping = IntPtr.Zero;
    IntPtr ptrToMemory = IntPtr.Zero;
    bool disposed = false;
    Semaphore semaphoreSend, semaphoreRecieve;
    readonly int memRegionSize = 0;
    readonly string memoryRegionName;
    readonly int sharedMemoryBaseSize = 0;
    #endregion

    #region Construction / Cleanup
    public SharedMemoryManager(string name, int sharedMemoryBaseSize)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException("name");

        if (sharedMemoryBaseSize <= 0)
            throw new ArgumentOutOfRangeException("sharedMemoryBaseSize",
                "Shared Memory Base Size must be a value greater than zero");

        // Set name of the region.
        memoryRegionName = name;
        // Save base size.
        this.sharedMemoryBaseSize = sharedMemoryBaseSize;
        // Set up the memory region size.
        memRegionSize = this.sharedMemoryBaseSize + sizeof(int);
        // Set up the shared memory section.
        SetupSharedMemory();
    }

    private void SetupSharedMemory()
    {
        // Grab some storage from the page file.
        handleFileMapping =
            PInvoke.CreateFileMapping((IntPtr)INVALID_HANDLE_VALUE,
                IntPtr.Zero, // security., // IntPtr.Zero,
                PInvoke.PageProtection.ReadWrite,
                0,
                memRegionSize,
                memoryRegionName);

        if (handleFileMapping == IntPtr.Zero)
        {
            throw new Win32Exception(
                "Could not create file mapping");
        }

        // Check the error status.
        var retVal = Marshal.GetLastWin32Error();
        switch (retVal)
        {
            case ERROR_ALREADY_EXISTS:
                // We opened one that already existed.
                // Make the mutex not the initial owner
                // of the mutex since we are connecting
                // to an existing one.
                semaphoreSend = Semaphore.OpenExisting(string.Format("{0}mtx{1}send", typeof(TransferItemType), memoryRegionName));
                semaphoreRecieve = Semaphore.OpenExisting(string.Format("{0}mtx{1}recieve", typeof(TransferItemType), memoryRegionName));
                break;
            case 0:
                // We opened a new one.
                // Make the mutex the initial owner.
                semaphoreSend = new Semaphore(100, 100, string.Format("{0}mtx{1}send", typeof(TransferItemType), memoryRegionName));
                semaphoreRecieve = new Semaphore(0, 100, string.Format("{0}mtx{1}recieve", typeof(TransferItemType), memoryRegionName));
                break;
            default:
                throw new Win32Exception(retVal, "Error creating file mapping");
        }

        // Map the shared memory.
        ptrToMemory = PInvoke.MapViewOfFile(handleFileMapping,
            FILE_MAP_WRITE,
            0, 0, IntPtr.Zero);

        if (ptrToMemory == IntPtr.Zero)
        {
            retVal = Marshal.GetLastWin32Error();
            throw new Win32Exception(retVal, "Could not map file view");
        }

        retVal = Marshal.GetLastWin32Error();
        if (retVal != 0 && retVal != ERROR_ALREADY_EXISTS)
        {
            // Something else went wrong.
            throw new Win32Exception(retVal, "Error mapping file view");
        }

        WinApi.memset(ptrToMemory, 0, memRegionSize);
    }

    ~SharedMemoryManager()
    {
        // Make sure we close.
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        // Check to see if Dispose has already been called.
        if (!disposed)
        {
            CloseSharedMemory();
        }
        disposed = true;
    }

    private void CloseSharedMemory()
    {
        if (ptrToMemory != IntPtr.Zero)
        {
            // Close map for shared memory.
            PInvoke.UnmapViewOfFile(ptrToMemory);
            ptrToMemory = IntPtr.Zero;
        }
        if (handleFileMapping != IntPtr.Zero)
        {
            // Close handle.
            PInvoke.CloseHandle(handleFileMapping);
            handleFileMapping = IntPtr.Zero;
        }
    }
    public void Close()
    {
        CloseSharedMemory();
    }
    #endregion

    #region Properties
    public int SharedMemoryBaseSize { get { return sharedMemoryBaseSize; } }

    #endregion

    #region Public Methods
    /// <summary>
    /// Send a serializable object through the shared memory
    /// and wait for it to be picked up.
    /// </summary>
    /// <param name="transferObject"> </param>
    public TransferItemType ShareObject(TransferItemType transferObject)
    {
        try
        {
            var ptr = EntityPtr.ToPointerWithOffset(transferObject);
                
            // Write out how long this object is.
            var typesize = transferObject.SizeOf();

            // Write out the bytes.
            WinApi.memcpy((IntPtr)((int)ptrToMemory), ptr, typesize);

            return EntityPtr.ToInstanceWithOffset<TransferItemType>((IntPtr) ((int) ptrToMemory));
        }
        finally
        {
            // Signal the other process using the mutex to tell it
            // to do receive processing.
            semaphoreRecieve.Release();
        }
    }

    /// <summary>
    /// Wait for an object to hit the shared memory and then deserialize it.
    /// </summary>
    /// <returns>object passed</returns>
    public TransferItemType ReceiveObject()
    {
        // Wait on the mutex for an object to be queued by the sender.
        semaphoreRecieve.WaitOne();

        // Read out the bytes for the object.
        return EntityPtr.ToInstanceWithOffset<TransferItemType>((IntPtr)((int)ptrToMemory));
    }
    #endregion
}