using System.IO;
using System.Text;

namespace SpyGlass.Hooking.Protocol
{
    public class ProcAddressRequest : Message
    {
        public ProcAddressRequest(string moduleName, string procedureName)
        {
            ModuleName = moduleName;
            ProcedureName = procedureName;
        }
        
        public string ModuleName
        {
            get;
            set;
        }

        public string ProcedureName
        {
            get;
            set;
        }

        public override void ReadFrom(BinaryReader reader)
        {
            ushort moduleNameLength = reader.ReadUInt16();
            ushort procedureNameLength = reader.ReadUInt16();
            ModuleName = Encoding.ASCII.GetString(reader.ReadBytes(moduleNameLength));
            ProcedureName = Encoding.ASCII.GetString(reader.ReadBytes(procedureNameLength));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((ushort) ModuleName.Length);
            writer.Write((ushort) ProcedureName.Length);
            writer.Write(Encoding.ASCII.GetBytes(ModuleName));
            writer.Write(Encoding.ASCII.GetBytes(ProcedureName));
        }

        public override string ToString()
        {
            return $"ProcAddress(Module: {ModuleName}, Procedure: {ProcedureName})";
        }
    }
}