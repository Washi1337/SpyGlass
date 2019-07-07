using System;
using System.ComponentModel;
using SpyGlass.Interop;

namespace SpyGlass
{
    public class RemoteThread : IDisposable
    {
        public RemoteThread(IntPtr handle, IntPtr id)
        {
            Handle = handle;
            Id = id;
        }

        ~RemoteThread()
        {
            ReleaseUnmanagedResources();
        }

        public IntPtr Handle
        {
            get;
            private set;
        }

        public IntPtr Id
        {
            get;
        }

        public void Resume()
        {
            if (Kernel32.ResumeThread(Handle) == -1)
                throw new Win32Exception();
        }

        public void Suspend()
        {
            if (Kernel32.SuspendThread(Handle) == -1)
                throw new Win32Exception();
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