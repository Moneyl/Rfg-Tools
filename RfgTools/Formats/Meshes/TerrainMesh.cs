using System;
using System.Collections.Generic;
using System.IO;
using RfgTools.Helpers;

namespace RfgTools.Formats.Meshes
{
    public class TerrainMesh
    {
        //Header data
        public uint Signature;
        public uint Version;

        //Subzone data
        public TerrainSubzoneData Subzone;

        //Patch data
        public List<TerrainPatch> Patches;

        //Decal data

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
        public List<SubmeshData> SubMeshes;

        //Render blocks
        public List<RenderBlock> RenderBlocks;

        public void Read(string headerPath, string dataPath)
        {
            using var cpuFileStream = new FileStream(headerPath, FileMode.Open);
            using var cpuFile = new BinaryReader(cpuFileStream);
            using var gpuFileStream = new FileStream(dataPath, FileMode.Open);
            using var gpuFile = new BinaryReader(gpuFileStream);

            uint expectedSignature = 1514296659; //Equals ASCII string "SUBZ"
            uint expectedVersion = 31;

            //Read header data
            Signature = cpuFile.ReadUInt32();
            if (Signature != expectedSignature)
                throw new Exception($"Error! Invalid static mesh signature. Expected value is {expectedSignature}. The detected signature is {Signature}");

            Version = cpuFile.ReadUInt32();
            if (Version != expectedVersion)
                throw new Exception($"Error! Invalid static mesh version. Expected version {expectedVersion}. The detected version is {Version}");

            //Read terrain subzone data
            cpuFile.BaseStream.Seek(20, SeekOrigin.Begin);
            Subzone = new TerrainSubzoneData();
            Subzone.Read(cpuFile);
            if(Subzone.NumDecals > 0) //Todo: add decal support
                throw new Exception("Error! Decal reading is supported yet. Show the maintainer this error, the command you ran, and the file you're trying "
                                    + $"to read \"{Path.GetFileName(headerPath)}\"");

            //Maybe read patch data (might be here)
            Patches = new List<TerrainPatch>();
            for (int i = 0; i < Subzone.PatchCount; i++)
            {
                var patch = new TerrainPatch();
                patch.Read(cpuFile);
                Patches.Add(patch);
            }

            cpuFile.Align(16);

            //Read decal data

            //Read mesh data
            //cpuFile.BaseStream.Seek(3536, SeekOrigin.Begin);
            MeshVersion = cpuFile.ReadUInt32();
            MeshSimpleCrc = cpuFile.ReadUInt32();
            CpuDataSize = cpuFile.ReadUInt32();
            GpuDataSize = cpuFile.ReadUInt32();

            //Read mesh data
            NumSubmeshes = cpuFile.ReadUInt32();
            SubmeshesOffset = cpuFile.ReadUInt32();
            VertexBufferConfig = new VertexBufferData();
            VertexBufferConfig.Read(cpuFile);
            IndexBufferConfig = new IndexBufferData();
            IndexBufferConfig.Read(cpuFile);

            var offsetFromMeshDataStart = cpuFile.BaseStream.Position - 3536;


            //Read submesh data
            SubMeshes = new List<SubmeshData>();
            uint numRenderBlocks = 0;
            for (int i = 0; i < NumSubmeshes; i++)
            {
                var subMesh = new SubmeshData();
                subMesh.Read(cpuFile);
                SubMeshes.Add(subMesh);
                numRenderBlocks += (uint)subMesh.NumRenderBlocks;
            }
            //Read render blocks
            //Todo: Total num might actually be sum of submeshes NumRenderBlock value
            RenderBlocks = new List<RenderBlock>();
            for (int i = 0; i < numRenderBlocks; i++)
            {
                var renderBlock = new RenderBlock();
                renderBlock.Read(cpuFile);
                RenderBlocks.Add(renderBlock);
            }
            //Todo: Compare with previous crc and report error if they don't match
            uint meshSimpleCrc2 = cpuFile.ReadUInt32();
            if(MeshSimpleCrc != meshSimpleCrc2)
                throw new Exception($"Failed to read mesh data from \"{Path.GetFileName(headerPath)}\". Mesh verification CRCs do not match!");

            //Align to 16 bytes before next section
            cpuFile.Align(16);


            var a = cpuFile.BaseStream.Position;
            var b = 2;
        }
    }
}
