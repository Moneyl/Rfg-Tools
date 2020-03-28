using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RfgTools.Helpers;

namespace RfgTools.Formats.Meshes
{
    public class MeshDataBlock
    {
        //Mesh data
        public uint MeshVersion;
        public uint MeshSimpleCrc; //Hash used several times in cpu and gpu file to validate mesh
        public uint CpuDataSize; //Size of data section from MeshOffset to material map data (minus alignment padding at end) in bytes
        public uint GpuDataSize; //Size of gpu file in bytes
        public uint NumSubmeshes;
        public uint SubmeshesOffset;
        public VertexBufferData VertexBufferConfig;
        public IndexBufferData IndexBufferConfig;

        //Submeshes
        public List<SubmeshData> SubMeshes;

        //Render blocks
        public List<RenderBlock> RenderBlocks;

        public void Read(BinaryReader data, string headerPath)
        {
            long startPos = data.BaseStream.Position;

            //Read mesh data
            MeshVersion = data.ReadUInt32();
            MeshSimpleCrc = data.ReadUInt32(); //Mesh CRC repeated several times in cpu & gpu file. Likely used to catch pack/unpack failures
            CpuDataSize = data.ReadUInt32(); //IIRC the size of cpu file mesh data + submesh data + render blocks + 4 for CRC 
            GpuDataSize = data.ReadUInt32(); //Size of gpu file in bytes

            //Read mesh data
            NumSubmeshes = data.ReadUInt32();
            SubmeshesOffset = data.ReadUInt32();
            VertexBufferConfig = new VertexBufferData(); //Contains info about vertex format and stride
            VertexBufferConfig.Read(data); //12 bytes
            IndexBufferConfig = new IndexBufferData(); //Contains info about index format and stride
            IndexBufferConfig.Read(data); //12 bytes
            data.Align(16);

            uint indicesSize = IndexBufferConfig.NumIndices * IndexBufferConfig.IndexSize;
            uint verticesSize = VertexBufferConfig.NumVerts * VertexBufferConfig.VertexStride0;

            //Read submesh data
            SubMeshes = new List<SubmeshData>();
            uint numRenderBlocks = 0;
            for (int i = 0; i < NumSubmeshes; i++)
            {
                var subMesh = new SubmeshData();
                subMesh.Read(data);
                SubMeshes.Add(subMesh);
                numRenderBlocks += (uint)subMesh.NumRenderBlocks;
            }

            //Read render blocks
            RenderBlocks = new List<RenderBlock>();
            for (int i = 0; i < numRenderBlocks; i++)
            {
                var renderBlock = new RenderBlock();
                renderBlock.Read(data);
                RenderBlocks.Add(renderBlock);
            }

            //Read another copy of the mesh CRC and validate
            uint meshSimpleCrc2 = data.ReadUInt32();
            if (MeshSimpleCrc != meshSimpleCrc2)
                throw new Exception($"Failed to read mesh data from \"{Path.GetFileName(headerPath)}\". Mesh verification CRCs do not match!");
            if(data.BaseStream.Position - startPos != CpuDataSize)
                throw new Exception($"Failed to read mesh data from \"{Path.GetFileName(headerPath)}\". Length of data read doesn't equal expected data size.");
        }
    }
}
