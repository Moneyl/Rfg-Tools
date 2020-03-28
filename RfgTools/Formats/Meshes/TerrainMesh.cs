﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using RfgTools.Helpers;
using RfgTools.Types;

namespace RfgTools.Formats.Meshes
{
    public class TerrainMesh
    {
        //Header data
        public uint Signature;
        public uint Version;
        public uint Index;
        public uint NumStitchPieceNames;
        public uint StitchPieceNamesSize;

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
        public uint SubmeshesOffset;
        public VertexBufferData VertexBufferConfig;
        public IndexBufferData IndexBufferConfig;

        //Submeshes
        public List<SubmeshData> SubMeshes;

        //Render blocks
        public List<RenderBlock> RenderBlocks;


        public void Read(string headerPath, string dataPath, string lowLodHeaderPath, string lowLodDataPath)
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
                throw new Exception(
                    $"Error! Invalid terrain mesh signature. Expected value is {expectedSignature}. The detected signature is {Signature}");

            //Read file format version
            Version = cpuFile.ReadUInt32();
            if (Version != expectedVersion)
                throw new Exception(
                    $"Error! Invalid terrain mesh version. Expected version {expectedVersion}. The detected version is {Version}");

            Index = cpuFile.ReadUInt32();
            NumStitchPieceNames = cpuFile.ReadUInt32();
            StitchPieceNamesSize = cpuFile.ReadUInt32();

            //Read terrain subzone data
            Subzone = new TerrainSubzoneData();
            Subzone.Read(cpuFile);
            if (Subzone.NumDecals > 0) //Todo: add decal support
                throw new Exception(
                    "Error! Decal reading is supported yet. Show the maintainer this error, the command you ran, and the file you're trying "
                    + $"to read \"{Path.GetFileName(headerPath)}\"");

            //Read patch data
            Patches = new List<TerrainPatch>();
            for (int i = 0; i < Subzone.PatchCount; i++)
            {
                var patch = new TerrainPatch();
                patch.Read(cpuFile);
                Patches.Add(patch);
            }

            cpuFile.Align(16);

            //Read decal data (Thought to be here but current test file has 0 decals)

            //Read mesh data
            MeshVersion = cpuFile.ReadUInt32();
            MeshSimpleCrc =
                cpuFile.ReadUInt32(); //Mesh CRC repeated several times in cpu & gpu file. Likely used to catch pack/unpack failures
            CpuDataSize =
                cpuFile.ReadUInt32(); //IIRC the size of cpu file mesh data + submesh data + render blocks + 4 for CRC 
            GpuDataSize = cpuFile.ReadUInt32(); //Size of gpu file in bytes

            //Read mesh data
            NumSubmeshes = cpuFile.ReadUInt32();
            SubmeshesOffset = cpuFile.ReadUInt32();
            VertexBufferConfig = new VertexBufferData(); //Contains info about vertex format and stride
            VertexBufferConfig.Read(cpuFile);
            IndexBufferConfig = new IndexBufferData(); //Contains info about index format and stride
            IndexBufferConfig.Read(cpuFile);

            //Read submesh data
            SubMeshes = new List<SubmeshData>();
            uint numRenderBlocks = 0;
            for (int i = 0; i < NumSubmeshes; i++)
            {
                var subMesh = new SubmeshData();
                subMesh.Read(cpuFile);
                SubMeshes.Add(subMesh);
                numRenderBlocks += (uint) subMesh.NumRenderBlocks;
            }

            //Read render blocks
            RenderBlocks = new List<RenderBlock>();
            for (int i = 0; i < numRenderBlocks; i++)
            {
                var renderBlock = new RenderBlock();
                renderBlock.Read(cpuFile);
                RenderBlocks.Add(renderBlock);
            }

            //Read another copy of the mesh CRC and validate
            uint meshSimpleCrc2 = cpuFile.ReadUInt32();
            if (MeshSimpleCrc != meshSimpleCrc2)
                throw new Exception(
                    $"Failed to read mesh data from \"{Path.GetFileName(headerPath)}\". Mesh verification CRCs do not match!");

            //Align to 16 bytes before next section
            cpuFile.Align(16);

            //Todo: Reverse the rest of the header format. Has some info about materials and more




            //Attempt to read gpu file data
            //First 4 bytes are the mesh CRC again, then align to 16 bytes
            uint gpuFileMeshSimpleCrc = gpuFile.ReadUInt32(); //Todo: Compare with other CRCs and error if not equal. Also should read CRC at end of gpuFile (last 4 bytes)
            //Align to 16 bytes to get to index list start
            gpuFile.Align(16);

            List<ushort> indices = new List<ushort>();
            List<TerrainMeshVert> vertices = new List<TerrainMeshVert>();

            //Read indices
            for (int i = 0; i < IndexBufferConfig.NumIndices; i++)
            {
                indices.Add(gpuFile.ReadUInt16());
            }

            gpuFile.Align(16); //Align to 16 bytes after reading indices

            //Read vertices
            for (int i = 0; i < VertexBufferConfig.NumVerts; i++)
            {
                var vert = new TerrainMeshVert();
                vert.Read(gpuFile);
                vertices.Add(vert);
            }

            //Read last instance of the mesh CRC at the end of the gpu file
            uint gpuFileEndCrc = gpuFile.ReadUInt32();
        }

        public class TerrainMeshVert
        {
            public vector3f Pos;
            public vector4f Normal;

            public void Read(BinaryReader data)
            {
                Pos = new vector3f(data.ReadUInt16(), data.ReadUInt16(), 0.0f);
                Normal = data.ReadCompressedVector4f();
            }
        }
    }
}
