
using System;
using System.IO;
using System.Text;
using RfgTools.Helpers;

namespace RfgTools.Formats.Meshes
{
    public class MeshHeaderShared
    {
        public uint Signature;
        public uint Version;
        public uint MeshOffset;
        public uint MaterialMapOffset;
        public uint MaterialsOffset;
        public uint NumMaterials;
        public uint TextureNamesOffset;

        public void Read(BinaryReader data, uint expectedSignature)
        {
            Signature = data.ReadUInt32();
            if (Signature != expectedSignature)
                throw new Exception($"Error! Invalid static mesh signature. Expected value is {expectedSignature}. The detected signature is {Signature}");

            Version = data.ReadUInt32();
            if (Version != 5)
                throw new Exception($"Error! Invalid static mesh version. Expected version 5. The detected version is {Version}");

            //Read header
            //Shared mesh header
            MeshOffset = data.ReadUInt32();
            data.Skip(4);
            MaterialMapOffset = data.ReadUInt32();
            data.Skip(4);
            MaterialsOffset = data.ReadUInt32();
            data.Skip(4);
            NumMaterials = data.ReadUInt32();
            data.Skip(4);
            TextureNamesOffset = data.ReadUInt32();
            data.Skip(4);
        }
    }
}
