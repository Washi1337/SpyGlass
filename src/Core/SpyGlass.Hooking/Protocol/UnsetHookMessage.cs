using System;
using System.IO;

namespace SpyGlass.Hooking.Protocol
{
    public class UnsetHookMessage : Message
    {
        public UnsetHookMessage(IntPtr address)
        {
            Address = address;
        }
        
        public IntPtr Address
        {
            get;
            set;
        }
        
        public override void ReadFrom(BinaryReader reader)
        {
            Address = (IntPtr) reader.ReadInt64();
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(Address.ToInt64());
        }
    }
}