using System;
using System.ComponentModel;
using SpyGlass.Interop;

namespace SpyGlass
{
    public class RemoteProcess : IDisposable
    {
        public RemoteProcess(int processId)
        {
            Handle = Kernel32.OpenProcess(Kernel32.ProcessAccessFlags.All, false, processId);
            if (Handle == IntPtr.Zero)
                throw new Win32Exception();
        }
        
        public RemoteProcess(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                throw new ArgumentException("Process handle cannot be zero.", nameof(handle));
            Handle = handle;
        }

        ~RemoteProcess()
        {
            ReleaseUnmanagedResources();
        }

        public IntPtr Handle
        {
            get;
            private set;
        }

        public int ReadMemory(IntPtr address, byte[] buffer, int offset, int length)
        {
            var temp = new byte[length];
            if (!Kernel32.ReadProcessMemory(Handle, address, temp, length, out var sizeRead))
                throw new Win32Exception();

            Buffer.BlockCopy(temp, 0, buffer, offset, length);
            return sizeRead.ToInt32();
        }

        public int WriteMemory(IntPtr address, byte[] bytes)
        {
            return WriteMemory(address, bytes, 0, bytes.Length);
        }

        public int WriteMemory(IntPtr address, byte[] bytes, int offset, int length)
        {
            var temp = new byte[length];
            Buffer.BlockCopy(bytes, offset, temp, 0, length);
            if (!Kernel32.WriteProcessMemory(Handle, address, temp, length, out var written))
                throw new Win32Exception();
            return written.ToInt32();
        }

        public IntPtr Allocate(int size)
        {
            return Allocate(size, MemoryProtection.ExecuteReadWrite);
        }

        public IntPtr Allocate(int size, MemoryProtection protection)
        {
            var result = Kernel32.VirtualAllocEx(
                Handle,
                IntPtr.Zero,
                (uint) size,
                AllocationType.Commit | AllocationType.Reserve,
                protection);
            
            if (result == IntPtr.Zero)
                throw new Win32Exception();

            return result;
        }

        public RemoteThread CreateThread(IntPtr entrypoint, IntPtr argument)
        {
            return CreateThread(entrypoint, argument, ThreadCreationFlags.None);
        }
        
        public RemoteThread CreateThread(IntPtr entrypoint, IntPtr argument, ThreadCreationFlags flags)
        {
            var result = Kernel32.CreateRemoteThread(Handle, 
                IntPtr.Zero, 
                0, 
                entrypoint, 
                argument, 
                flags, 
                out var id);

            if (result == IntPtr.Zero)
                throw new Win32Exception();

            return new RemoteThread(result, id);
        }
        
        private void ReleaseUnmanagedResources()
        {
            if (Handle != IntPtr.Zero)
                Kernel32.CloseHandle(Handle);
            Handle = IntPtr.Zero;
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }
    }
}