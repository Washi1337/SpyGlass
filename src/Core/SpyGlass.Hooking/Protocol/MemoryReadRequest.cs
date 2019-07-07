using System;
using System.IO;

namespace SpyGlass.Hooking.Protocol
{
    public class MemoryReadRequest : Message
    {
        public MemoryReadRequest(IntPtr address, int length)
        {
            Address = address;
            Length = length;
        }
        
        public IntPtr Address
        {
            get;
            set;
        }

        public int Length
        {
            get;
            set;
        }
        
        public override void ReadFrom(BinaryReader reader)
        {
            Address = (IntPtr) reader.ReadInt64();
            Length = reader.ReadInt32();
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(Address.ToInt64());
            writer.Write(Length);
        }

        public override string ToString()
        {
            return $"MemoryRead(Address: {Address.ToInt64():X8}, Length: {Length})";
        }
    }
}