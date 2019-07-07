using System;

namespace SpyGlass.Hooking
{
    public class HookEventArgs : EventArgs
    {
        public HookEventArgs(IntPtr address)
        {
            Address = address;
        }

        public IntPtr Address
        {
            get;
        }
    }
}