using System;
using System.Collections.Generic;
using System.IO;

namespace SpyGlass.Hooking.Protocol
{
    public class CallbackMessage : Message
    {
        public long Id
        {
            get;
            set;
        }

        public IList<ulong> Registers
        {
            get;
        } = new List<ulong>();
        
        public override void ReadFrom(BinaryReader reader)
        {
            Id = reader.ReadInt64();
            int count = reader.ReadInt32();
            
            Registers.Clear();

            for (int i = 0; i < count; i++)
                Registers.Add(reader.ReadUInt64());
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(Registers.Count);
            foreach (var register in Registers)
                writer.Write(register);
        }

        public override string ToString()
        {
            return $"Callback(Id: {Id}, Registers: {Registers.Count})";
        }
    }
}