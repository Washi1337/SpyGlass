using System;
using System.Collections.Generic;
using AsmResolver;
using AsmResolver.X86;
using SpyGlass.Hooking;

namespace SpyGlass.Sample.x86
{
    internal class AsmResolverParametersDetector : IHookParametersDetector
    {
        public HookParameters Detect(HookSession session, IntPtr address)
        {
            var fixups = new List<ushort>();
            
            // Longest x86 instruction possible is 15 bytes. We need 5 bytes at least for a call.
            // Therefore, in the worst case scenario, we need to read 4 + 15 bytes worth of instructions.
            
            var reader = new MemoryStreamReader(session.ReadMemory(address, 4 + 15));
            var disassembler = new X86Disassembler(reader, address.ToInt64());

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

            return new HookParameters((int) reader.Position, fixups);
        }
    }
}