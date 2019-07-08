using System;
using System.IO;

namespace SpyGlass.Hooking.Protocol
{
    public class ProcAddressResponse : Message
    {
        public IntPtr Address
        {
            get;
            set;
        }
        
        public override void ReadFrom(BinaryReader reader)
        {
            Address = (IntPtr) reader.ReadUInt64();
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(Address.ToInt64());
        }

        public override string ToString()
        {
            return $"ProcAddressResponse(Address: {Address.ToInt64():X8})";
        }
    }
}