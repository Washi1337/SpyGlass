using System;
using System.Collections.Generic;
using AsmResolver.X86;
using SpyGlass.Hooking;

namespace SpyGlass.Sample.x86
{
    internal class AsmResolverParametersDetector : IHookParametersDetector
    {
        public HookParameters Detect(RemoteProcess process, IntPtr address)
        {
            var fixups = new List<ushort>();
            
            var reader = new RemoteProcessMemoryReader(process, address);
            var disassembler = new X86Disassembler(reader);

            while (reader.Position - reader.StartPosition < 5)
            {
                var next = disassembler.ReadNextInstruction();
                if (next.OpCode.Op1 == X86OpCodes.Jmp_Rel1632.Op1
                    || next.OpCode.Op1 == X86OpCodes.Call_Rel1632.Op1)
                {
                    int offset = (int) (reader.Position - address.ToInt64() - 4);
                    fixups.Add((ushort) offset);
                }
            }

            return new HookParameters((int) (reader.Position - address.ToInt64()), fixups);
        }
    }
}