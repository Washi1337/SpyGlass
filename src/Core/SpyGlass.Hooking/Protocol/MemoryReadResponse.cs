using System.IO;

namespace SpyGlass.Hooking.Protocol
{
    public class MemoryReadResponse : Message
    {
        public byte[] Data
        {
            get;
            set;
        }
        
        public override void ReadFrom(BinaryReader reader)
        {
            Data = reader.ReadBytes((int) (reader.BaseStream.Length - reader.BaseStream.Position));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(Data);
        }

        public override string ToString()
        {
            return $"MemoryReadResponse(Data: {Data.Length} bytes)";
        }
    }
}