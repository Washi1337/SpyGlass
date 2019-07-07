using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpyGlass.Hooking.Protocol
{
    public class ContinueMessage : Message
    {
        public ContinueMessage(long id)
        {
            Id = id;
        }
        
        public long Id
        {
            get;
            set;
        }

        public Dictionary<int, ulong> RegisterChanges
        {
            get;
        } = new Dictionary<int, ulong>();
        
        public override void ReadFrom(BinaryReader reader)
        {
            Id = reader.ReadInt64();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                RegisterChanges.Add(reader.ReadInt32(), reader.ReadUInt64());
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(RegisterChanges.Count);
            foreach (var entry in RegisterChanges)
            {
                writer.Write(entry.Key);
                writer.Write(entry.Value);
            }
        }

        public override string ToString()
        {
            string changes = string.Join(", ", 
                RegisterChanges.Select(e => e.Key + ": " + e.Value.ToString("X8")));
            return $"Continue({Id}, {{{changes}}})";
        }
    }
}