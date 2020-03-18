using System.Collections.Generic;
using System.IO;
using System.Linq;
using RfgTools.Helpers;
using RfgTools.Types;

namespace RfgTools.Formats.Meshes
{
    public class StaticMesh
    {
        //Shared mesh header used by all mesh types
        public MeshHeaderShared SharedHeader;
        //Static mesh specific header data (may be similar to other mesh types data)
        public uint NumLods;
        public uint LodSubmeshIdOffset;
        public uint MeshTagsOffset;
        public uint MeshTagsNumTags;
        public uint MeshTagsInternalOffset;
        public uint CmIndex;

        //Mesh data
        public uint MeshVersion;
        public uint MeshSimpleCrc; //Hash used several times in cpu and gpu file to validate mesh
        public uint CpuDataSize; //Size of data section from MeshOffset to material map data (minus alignment padding at end) in bytes
        public uint GpuDataSize; //Size of gpu file in bytes
        public uint NumSubmeshes;
        public uint SubmeshesOffset; //Seems to be a pointer set at runtime
        public VertexBufferData VertexBufferConfig;
        public IndexBufferData IndexBufferConfig;

        //Submeshes
        public List<SubmeshData> SubMeshes = new List<SubmeshData>();

        //Render blocks
        public List<RenderBlock> RenderBlocks = new List<RenderBlock>();


        //Gpu file data
        public List<ushort> Indices = new List<ushort>();
        public List<Vertex> Vertices = new List<Vertex>();
        //public List<vector3f> Uvs = new List<vector3f>();
        //public List<vector3f> Normals = new List<vector3f>();

        public void Read(string headerInputPath, string dataInputPath)
        {
            using var cpuFileStream = new FileStream(headerInputPath, FileMode.Open);
            using var cpuFile = new BinaryReader(cpuFileStream);
            using var gpuFileStream = new FileStream(dataInputPath, FileMode.Open);
            using var gpuFile = new BinaryReader(gpuFileStream);

            //Read header
            //Shared mesh header
            SharedHeader = new MeshHeaderShared();
            SharedHeader.Read(cpuFile, 0xC0FFEE11);

            //Static mesh specific header data
            NumLods = cpuFile.ReadUInt32();
            cpuFile.Skip(4);
            LodSubmeshIdOffset = cpuFile.ReadUInt32();
            cpuFile.Skip(4);
            MeshTagsOffset = cpuFile.ReadUInt32();
            cpuFile.Skip(4);

            MeshTagsNumTags = cpuFile.ReadUInt32();
            cpuFile.Skip(4);
            MeshTagsInternalOffset = cpuFile.ReadUInt32();
            cpuFile.Skip(4);

            CmIndex = cpuFile.ReadUInt32();
            cpuFile.Skip(4);

            //Read mesh data
            //Seek to mesh data offset
            cpuFile.BaseStream.Seek(SharedHeader.MeshOffset, SeekOrigin.Begin);

            MeshVersion = cpuFile.ReadUInt32();
            MeshSimpleCrc = cpuFile.ReadUInt32();
            CpuDataSize = cpuFile.ReadUInt32();
            GpuDataSize = cpuFile.ReadUInt32();
            NumSubmeshes = cpuFile.ReadUInt32();
            SubmeshesOffset = cpuFile.ReadUInt32();
            VertexBufferConfig = new VertexBufferData();
            VertexBufferConfig.Read(cpuFile);
            IndexBufferConfig = new IndexBufferData();
            IndexBufferConfig.Read(cpuFile);

            for (int i = 0; i < NumSubmeshes; i++)
            {
                var subMesh = new SubmeshData();
                subMesh.Read(cpuFile);
                SubMeshes.Add(subMesh);
            }
            //Todo: Total num might actually be sum of submeshes NumRenderBlock value
            for (int i = 0; i < NumSubmeshes; i++)
            {
                var renderBlock = new RenderBlock();
                renderBlock.Read(cpuFile);
                RenderBlocks.Add(renderBlock);
            }
            //Todo: Compare with previous crc and report error if they don't match
            uint MeshSimpleCrc2 = cpuFile.ReadUInt32();
            //Align to 16 bytes before next section
            cpuFile.Align(16);

            //material map data

            //material data


            //Read gpu file data
            uint gpuFileMeshSimpleCrc = gpuFile.ReadUInt32(); //Todo: Compare with other CRCs and error if not equal
            gpuFile.Align(16);

            //Note: Currently assuming only one index and vertex type for static meshes, need to set up way of dynamically handling different layouts across meshes
            //Todo: Add support for different data layouts based on the PrimitiveTopology and VertexFormat enums
            //Read index buffer
            for (int i = 0; i < IndexBufferConfig.NumIndices; i++)
            {
                Indices.Add(gpuFile.ReadUInt16());
            }
            gpuFile.Align(4);

            //Read vertex buffer
            for (int i = 0; i < VertexBufferConfig.NumVerts; i++)
            {
                //Todo: Check that these aren't flipped or that it actually isn't all the normals then all the UVs or vice versa
                //Todo: Check if its really index, vec2 uv, vec3 vertex
                //Uvs.Add(gpuFile.ReadVector3f());
                //Normals.Add(gpuFile.ReadVector3f());
                var vertex = new Vertex();
                vertex.Read(gpuFile);
                Vertices.Add(vertex);
            }

            //103 in test file, should equal Max index value and num vertices
            int numUniqueIndexValues = Indices.Select(x => x).Distinct().Count();

            //Pixlit1UvNmap - vertStride0 = 24
            //Pixlit3UvNmap - vertStride0 = 32

            var pos = cpuFile.BaseStream.Position;
            var a = 2;
        }
    }
}
