using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SpyGlass.Interop;

namespace SpyGlass
{
    public class RemoteProcess : IDisposable
    {
        public static RemoteProcess Create(string path, string arguments, bool suspended)
        {
            bool retValue;
            
            var pInfo = new PROCESS_INFORMATION();
            var sInfo = new STARTUPINFO();
            var pSec = new SECURITY_ATTRIBUTES();
            var tSec = new SECURITY_ATTRIBUTES();
            
            pSec.nLength = Marshal.SizeOf(pSec);
            tSec.nLength = Marshal.SizeOf(tSec);

            var flags = ProcessCreationFlags.CREATE_NEW_CONSOLE | ProcessCreationFlags.CREATE_NEW_PROCESS_GROUP;
            if (suspended)
                flags |= ProcessCreationFlags.CREATE_SUSPENDED;
            
            retValue = Kernel32.CreateProcess(
                null, path + " " + arguments,
                IntPtr.Zero, IntPtr.Zero,false, flags,
                IntPtr.Zero,null, ref sInfo, out pInfo);

            if (!retValue)
                throw new Win32Exception();
            
            return new RemoteProcess(pInfo.hProcess);
        }
        
        public RemoteProcess(int processId)
        {
            Handle = Kernel32.OpenProcess(ProcessAccessFlags.All, false, processId);
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

        public int Id => Kernel32.GetProcessId(Handle);

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

        public IEnumerable<RemoteThread> GetThreads()
        {
            int processId = Id;
            var snapshot = Kernel32.CreateToolhelp32Snapshot(CreateToolhelp32SnapshotFlags.TH32CS_SNAPTHREAD, (uint) processId);

            THREADENTRY32 threadEntry = new THREADENTRY32
            {
                dwSize = (uint) Marshal.SizeOf(typeof(THREADENTRY32))
            };

            if (!Kernel32.Thread32First(snapshot, ref threadEntry))
                throw new Win32Exception();

            do
            {
                if (threadEntry.th32OwnerProcessID == processId)
                {
                    yield return new RemoteThread(Kernel32.OpenThread(
                        ThreadAccess.SUSPEND_RESUME,
                        false,
                        threadEntry.th32ThreadID), (IntPtr) threadEntry.th32ThreadID);
                }
            } while (Kernel32.Thread32Next(snapshot, ref threadEntry));

            Kernel32.CloseHandle(snapshot);
        }
        
        public void Resume()
        {
            foreach (var remoteThread in GetThreads())
            {
                using (remoteThread)
                {
                    remoteThread.Resume();
                }
            }
        }
        
        public void Suspend()
        {
            foreach (var remoteThread in GetThreads())
            {
                using (remoteThread)
                {
                    remoteThread.Suspend();
                }
            }
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