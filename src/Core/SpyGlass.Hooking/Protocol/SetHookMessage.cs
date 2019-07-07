using System;
using System.Collections.Generic;
using System.IO;

namespace SpyGlass.Hooking.Protocol
{
    public class SetHookMessage : IMessage
    {
        public SetHookMessage(IntPtr address, int count, IEnumerable<ushort> fixups)
        {
            Address = address;
            Count = count;
            Fixups = new List<ushort>(fixups);
        }
        
        public IntPtr Address
        {
            get;
            set;
        }

        public int Count
        {
            get;
            set;
        }
        
        public IList<ushort> Fixups
        {
            get;
        } = new List<ushort>();
        
        public void ReadFrom(BinaryReader reader)
        {
            Address = new IntPtr(reader.ReadInt64());
            Count = reader.ReadInt32();
            
            Fixups.Clear();
            
            int fixupsCount = reader.ReadInt16();
            for (int i = 0; i < fixupsCount; i++)
                Fixups.Add(reader.ReadUInt16());
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Address.ToInt64());
            writer.Write(Count);
            writer.Write((ushort) Fixups.Count);
            foreach (ushort offset in Fixups)
                writer.Write(offset);
        }

        public override string ToString()
        {
            return $"SetHook(Address: {Address}, Count: {Count})";
        }
    }
}