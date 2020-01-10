using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using RfgTools.Helpers;

namespace RfgTools.Formats.Meshes
{
    public class StaticMesh
    {
        public uint Signature;
        public uint Version;

        public uint MeshOffset;
        //4 bytes padding from et_sized_native_pointer
        public uint MaterialMapOffset;
        //4 bytes padding from et_sized_native_pointer
        public uint MaterialsOffset;
        //4 bytes padding from et_sized_native_pointer

        public uint NumMaterials;
        //4 bytes padding from alignment

        public uint TextureNamesOffset;
        //4 bytes padding from et_sized_native_pointer

        public uint NumLods;
        //4 bytes padding from alignment

        public uint LodSubmeshIdOffset;
        //4 bytes padding from et_sized_native_pointer
        public uint MeshTagsOffset;
        //4 bytes padding from et_sized_native_pointer

        public uint MeshTagsNumTags;
        //4 bytes padding from alignment

        public uint MeshTagsInternalOffset;
        //4 bytes padding from et_sized_native_pointer

        public uint CmIndex;
        //4 bytes padding from alignment

        public void Read(string headerInputPath, string dataInputPath)
        {
            using var headerStream = new FileStream(headerInputPath, FileMode.Open);
            using var header = new BinaryReader(headerStream);
            using var dataStream = new FileStream(dataInputPath, FileMode.Open);
            using var data = new BinaryReader(dataStream);

            Signature = header.ReadUInt32();
            if (Signature != 0xC0FFEE11)
                throw new Exception($"Error! Invalid static mesh signature. Expected value is 3237998097. The detected signature is {Signature}");

            Version = header.ReadUInt32();
            if (Version != 5)
                throw new Exception($"Error! Invalid static mesh version. Expected version 5. The detected version is {Version}");

            MeshOffset = header.ReadUInt32();
            header.Skip(4);
            MaterialMapOffset = header.ReadUInt32();
            header.Skip(4);
            MaterialsOffset = header.ReadUInt32();
            header.Skip(4);
            NumMaterials = header.ReadUInt32();
            header.Skip(4);
            TextureNamesOffset = header.ReadUInt32();
            header.Skip(4);
            NumLods = header.ReadUInt32();
            header.Skip(4);
            LodSubmeshIdOffset = header.ReadUInt32();
            header.Skip(4);
            MeshTagsOffset = header.ReadUInt32();
            header.Skip(4);
            MeshTagsNumTags = header.ReadUInt32();
            header.Skip(4);
            MeshTagsInternalOffset = header.ReadUInt32();
            header.Skip(4);
            CmIndex = header.ReadUInt32();
            header.Skip(4);

            var pos = header.BaseStream.Position;
            var a = 2;
        }
    }
}
