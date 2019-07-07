using System;
using System.Collections.Generic;

namespace SpyGlass.Hooking
{
    public class HookEventArgs : EventArgs
    {
        public HookEventArgs(IList<ulong> registers)
        {
            Registers = registers;
        }

        public IList<ulong> Registers
        {
            get;
        }
    }
}