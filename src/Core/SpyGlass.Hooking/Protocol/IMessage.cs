using System.IO;

namespace SpyGlass.Hooking.Protocol
{
    public interface IMessage
    {
        void ReadFrom(BinaryReader reader);

        void WriteTo(BinaryWriter writer);
    }
}