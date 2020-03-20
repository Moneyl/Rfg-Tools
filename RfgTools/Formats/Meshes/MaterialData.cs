using System.IO;

namespace RfgTools.Formats.Meshes
{
    public class MaterialData
    {
        public uint ShaderHandle;
        public uint NameChecksum;
        public uint MaterialFlags; //Todo: Find meaning of values for this
        public ushort NumTextures;
        public byte NumConstants;
        public byte MaxConstants;
        public uint TextureOffset; //Seems to be a ptr set at runtime
        public uint ConstantNameChecksumsOffset; //Seems to be a ptr set at runtime
        public uint ConstantBlockOffset; //Seems to be a ptr set at runtime

        public void Read(BinaryReader data)
        {
            ShaderHandle = data.ReadUInt32();
            NameChecksum = data.ReadUInt32();
            MaterialFlags = data.ReadUInt32();
            NumTextures = data.ReadUInt16();
            NumConstants = data.ReadByte();
            MaxConstants = data.ReadByte();
            TextureOffset = data.ReadUInt32();
            ConstantNameChecksumsOffset = data.ReadUInt32();
            ConstantBlockOffset = data.ReadUInt32();
        }
    }
}
