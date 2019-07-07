using System;
using System.IO;

namespace SpyGlass.Hooking.Protocol
{
    public class CallbackMessage : IMessage
    {
        public long Id
        {
            get;
            set;
        }

        public IntPtr Address
        {
            get;
            set;
        }
        
        public void ReadFrom(BinaryReader reader)
        {
            Id = reader.ReadInt64();
            Address = new IntPtr(reader.ReadInt64());
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(Address.ToInt64());
        }
    }
}