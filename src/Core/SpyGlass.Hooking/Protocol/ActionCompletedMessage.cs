using System.IO;

namespace SpyGlass.Hooking.Protocol
{
    public class ActionCompletedMessage : Message
    {
        public HookErrorCode ErrorCode
        {
            get;
            set;
        }

        public uint Metadata
        {
            get;
            set;
        }
        
        public override void ReadFrom(BinaryReader reader)
        {
            ErrorCode = (HookErrorCode) reader.ReadUInt32();
            Metadata = reader.ReadUInt32();
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((uint) ErrorCode);
            writer.Write((uint) Metadata);
        }

        public override string ToString()
        {
            return $"Completed(code: {ErrorCode}, Metadata: {Metadata})";
        }
    }
}