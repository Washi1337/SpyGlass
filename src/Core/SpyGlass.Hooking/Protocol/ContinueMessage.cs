using System.IO;

namespace SpyGlass.Hooking.Protocol
{
    public class ContinueMessage : IMessage
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
        
        public void ReadFrom(BinaryReader reader)
        {
            Id = reader.ReadInt64();
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Id);
        }
    }
}