using System;
using System.IO;

namespace SpyGlass.Hooking.Protocol
{
    public class MemoryEditRequest : Message
    {
        public MemoryEditRequest(IntPtr address, byte[] data)
        {
            Address = address;
            Data = data;
        }
        
        public IntPtr Address
        {
            get;
            set;
        }
        
        public byte[] Data
        {
            get;
            set;
        }
        
        public override void ReadFrom(BinaryReader reader)
        {
            Address = (IntPtr) reader.ReadUInt64();
            Data = reader.ReadBytes((int) (reader.BaseStream.Length - reader.BaseStream.Position));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(Address.ToInt64());
            writer.Write(Data);
        }

        public override string ToString()
        {
            return $"MemoryEdit(Address: {Address.ToInt64():X8}, Data: {Data.Length} bytes)";
        }
    }
}