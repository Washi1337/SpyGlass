using System.Collections.Generic;

namespace SpyGlass.Hooking
{
    public class HookParameters
    {
        public HookParameters(int bytesToOverwrite, IEnumerable<ushort> fixups)
        {
            BytesToOverwrite = bytesToOverwrite;
            Fixups = new List<ushort>(fixups);
        }
        
        public int BytesToOverwrite
        {
            get;
        }

        public List<ushort> Fixups
        {
            get;
        }
    }
}