using System.IO;

namespace SpyGlass.Hooking.Protocol
{
    public class ActionCompletedMessage : IMessage
    {
        public int ErrorCode
        {
            get;
            set;
        }
        
        public void ReadFrom(BinaryReader reader)
        {
            ErrorCode = reader.ReadInt32();
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(ErrorCode);
        }

        public override string ToString()
        {
            return $"Completed (code: {ErrorCode})";
        }
    }
}