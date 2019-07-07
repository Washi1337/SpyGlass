using System.IO;

namespace SpyGlass.Hooking.Protocol
{
    public abstract class Message
    {
        public int SequenceNumber
        {
            get;
            set;
        }
        
        public abstract void ReadFrom(BinaryReader reader);

        public abstract void WriteTo(BinaryWriter writer);
    }
}