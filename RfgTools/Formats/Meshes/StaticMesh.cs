using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic;
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
        public List<SubmeshData> SubMeshes;

        //Render blocks
        public List<RenderBlock> RenderBlocks;

        //Material data block
        public MaterialBlock MaterialBlock;

        //Texture names
        public List<string> TextureNames;


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

            SubMeshes = new List<SubmeshData>();
            for (int i = 0; i < NumSubmeshes; i++)
            {
                var subMesh = new SubmeshData();
                subMesh.Read(cpuFile);
                SubMeshes.Add(subMesh);
            }
            //Todo: Total num might actually be sum of submeshes NumRenderBlock value
            RenderBlocks = new List<RenderBlock>();
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

            //Read material data block
            MaterialBlock = new MaterialBlock();
            MaterialBlock.Read(cpuFile);

            //Read texture names
            TextureNames = new List<string>();
            cpuFile.BaseStream.Seek(SharedHeader.TextureNamesOffset, SeekOrigin.Begin);
            foreach (var desc in MaterialBlock.TextureDescs)
            {
                cpuFile.BaseStream.Seek(SharedHeader.TextureNamesOffset + desc.NameOffset, SeekOrigin.Begin);
                TextureNames.Add(cpuFile.ReadNullTerminatedString());
            }

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
            //gpuFile.Align(4);
            gpuFile.Align(16);

            long verticesOffset = gpuFile.BaseStream.Position;
            //Read vertex buffer
            for (int i = 0; i < VertexBufferConfig.NumVerts; i++)
            {
                var vertex = new Vertex();
                vertex.Read(gpuFile, VertexBufferConfig.VertexFormat);
                Vertices.Add(vertex);
            }

            var pos = cpuFile.BaseStream.Position;
            var a = 2;
        }

        public void WriteToObjFile(string outputPath, string diffuseMapPath, string normalMapPath, string specularMapPath)
        {
            //Open obj file and write mesh data to it. Can easily be opened in blender
            using var objStream = new FileStream(outputPath, FileMode.Create);
            using var objWriter = new StreamWriter(objStream);

            string outputFileNameNoExt = Path.GetFileNameWithoutExtension(outputPath);
            string outputMaterialName = $"{outputFileNameNoExt}_mat";
            string outputMaterialFileName = $"{outputFileNameNoExt}_material.mtl";
            string outputMaterialPath = $"{Path.GetDirectoryName(outputPath)}//{outputMaterialFileName}";

            //Write material
            objWriter.WriteLine($"mtllib {outputMaterialFileName}");
            objWriter.WriteLine($"usemtl {outputMaterialName}");

            //Write vertices
            float index = 0.0f;
            foreach (var vertex in Vertices)
            {
                objWriter.WriteLine($"v {vertex.Pos.x} {vertex.Pos.y} {vertex.Pos.z}");// {index:0.#}");
                index += 0.1f;
            }

            //Write uv data
            index = 0.0f;
            foreach (var vertex in Vertices)
            {
                //objWriter.WriteLine($"vt {MathF.Abs(vertex.Uvs.x / 1024)} {MathF.Abs(vertex.Uvs.y / 1024)}");
                objWriter.WriteLine($"vt {MathF.Abs(vertex.Uvs.x / 1024)} {MathF.Abs(vertex.Uvs.y / 1024)}");
                index += 0.1f;
            }

            //Write normals
            //Todo: Figure out if tangent vecs are needed, currently are discarded
            //Todo: Figure out if normal vecs are needed, currently are discarded
            //foreach (var vertex in Vertices)
            //{
            //    objWriter.WriteLine($"vn {vertex.Normal.x} {vertex.Normal.y} {vertex.Normal.z}");
            //    index += 0.1f;
            //}

            //Write faces
            for (int i = 1; i < Indices.Count - 2; i++)
            {
                int index0 = Indices[i] + 1;
                int index1 = Indices[i + 1] + 1;
                int index2 = Indices[i + 2] + 1;

                objWriter.WriteLine($"f {index0}/{index0} {index1}/{index1} {index2}/{index2}");
            }


            //Write material file
            using var materialStream = new FileStream(outputMaterialPath, FileMode.Create);
            using var materialWriter = new StreamWriter(materialStream);

            materialWriter.WriteLine($"newmtl {outputMaterialName}");
            materialWriter.WriteLine("Ka 1.000 1.000 1.000");
            materialWriter.WriteLine("Kd 1.000 1.000 1.000");
            materialWriter.WriteLine("Ks 0.000 0.000 0.000");
            materialWriter.Write("\n");

            if(diffuseMapPath != null)
                materialWriter.WriteLine($"map_Kd {diffuseMapPath}");
            if (normalMapPath != null)
                materialWriter.WriteLine($"map_bump {normalMapPath}");
            if (diffuseMapPath != null)
                materialWriter.WriteLine($"map_Ns {specularMapPath}");
        }
    }
}
