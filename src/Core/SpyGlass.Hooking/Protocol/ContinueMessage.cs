using System.IO;

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
        
        public override void ReadFrom(BinaryReader reader)
        {
            Id = reader.ReadInt64();
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(Id);
        }

        public override string ToString()
        {
            return $"Continue({Id})";
        }
    }
}