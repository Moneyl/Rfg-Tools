using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using RfgTools.Helpers;
using RfgTools.Types;

namespace RfgTools.Formats.Meshes
{
    public class TerrainMeshLowLod
    {
        //Header data
        public uint Signature;
        public uint Version;
        public uint NumTextureNames;
        public uint TextureNamesSize;
        public uint NumFmeshNames;
        public uint FmeshNamesSize;
        public uint StitchPieceNamesSize;
        public uint NumStitchPieceNames;
        public uint NumStitchPieces;

        //Texture names
        public List<string> TextureNames;

        //Stitch piece names
        public List<string> StitchPieceNames;

        //Fmesh names
        public List<string> FmeshNames;

        //Stitch pieces
        public List<TerrainStitchInfo> StitchPieces = new List<TerrainStitchInfo>();

        //Terrain data
        public TerrainData TerrainData = new TerrainData(); //Todo: Figure out if better to init these like this, in Read(), in constructor, or if it doesn't matter

        //String list of unknown purpose
        public List<string> UnkStringList;

        //Material data
        public List<MaterialBlock2> Materials = new List<MaterialBlock2>();

        //Believed to be a havok binary tag file //Todo: Reverse this section
        //Todo: Make a simple class here to handle reading it's data size and skipping

        //Another unknown string list
        public List<string> UnkStringList2;

        //Some data block, maybe more materials

        //Another unknown string list
        public List<string> UnkStringList3;

        //Some data block

        //Another unknown string list
        public List<string> UnkStringList4;

        //Bunch of non-RE'd data in between

        //Another unknown string list
        public List<string> UnkStringList5;

        //Mesh data
        public List<MeshDataBlock> MeshData = new List<MeshDataBlock>();

        public void Read(string headerPath, string dataPath)
        {
            //Attempt to read low-lod mesh
            using var cpuFileStream = new FileStream(headerPath, FileMode.Open);
            using var cpuFile = new BinaryReader(cpuFileStream);
            using var gpuFileStream = new FileStream(dataPath, FileMode.Open);
            using var gpuFile = new BinaryReader(gpuFileStream);

            uint expectedSignature = 1381123412; //Equals ASCII string "TERR"
            uint expectedVersion = 31;

            //Read header data
            Signature = cpuFile.ReadUInt32();
            if (Signature != expectedSignature)
                throw new Exception($"Error! Invalid low-lod terrain mesh signature. Expected value is {expectedSignature}. The detected signature is {Signature}");

            //Read file format version
            Version = cpuFile.ReadUInt32();
            if (Version != expectedVersion)
                throw new Exception($"Error! Invalid low-lod terrain mesh version. Expected version {expectedVersion}. The detected version is {Version}");

            NumTextureNames = cpuFile.ReadUInt32();
            TextureNamesSize = cpuFile.ReadUInt32();
            NumFmeshNames = cpuFile.ReadUInt32();
            FmeshNamesSize = cpuFile.ReadUInt32();
            StitchPieceNamesSize = cpuFile.ReadUInt32();
            NumStitchPieceNames = cpuFile.ReadUInt32();
            NumStitchPieces = cpuFile.ReadUInt32();

            //Complain about stitch piece names > 0. Haven't tested a file with any yet
            if(NumStitchPieceNames > 0)
                throw new Exception($"Error! NumStitchPieceNames > 0 in \"{Path.GetFileName(headerPath)}\". Not yet supported. Show this to the maintainer and tell them which file caused the error.");

            //Read texture names
            TextureNames = cpuFile.ReadSizedStringList(TextureNamesSize);
            cpuFile.Align(4);

            //Todo: Figure out what the hell is going on. For some reason even though NumStitchPieceNames == 0, StitchPieceNamesSize > 0 and fits this string data perfectly
            //Read stitch piece names
            StitchPieceNames = cpuFile.ReadSizedStringList(StitchPieceNamesSize);

            //Read stitch piece data
            for (int i = 0; i < NumStitchPieces; i++)
            {
                var stitchPiece = new TerrainStitchInfo();
                stitchPiece.Read(cpuFile);
                StitchPieces.Add(stitchPiece);
            }
            cpuFile.Align(4);

            //Read fmesh names
            FmeshNames = cpuFile.ReadSizedStringList(FmeshNamesSize);
            cpuFile.Align(4);

            //Read terrain data and align to 4 bytes
            TerrainData.Read(cpuFile);

            uint unkNamesListSize = cpuFile.ReadUInt32();
            UnkStringList = cpuFile.ReadSizedStringList(unkNamesListSize);
            cpuFile.Align(16);

            uint maybeNumMaterials = cpuFile.ReadUInt32();
            if (maybeNumMaterials == 0)
            {
                cpuFile.Skip(4);
                maybeNumMaterials = cpuFile.ReadUInt32();
                cpuFile.BaseStream.Seek(cpuFile.BaseStream.Position - 8, SeekOrigin.Begin);
                if (maybeNumMaterials == 0)
                {
                    var e = 2;
                }
            }
            //cpuFile.Skip(4); //Don't know if this skip is necessary but the game code seems to do it.
            cpuFile.Align(16);

            cpuFile.Skip(32); //Skip because I don't know what this is //Todo: RE this, something for materials
            cpuFile.Align(16);
            cpuFile.Skip(maybeNumMaterials * 4); //Todo: confirm correctness
            cpuFile.Align(16);


            //uint maybeNumMaterials = cpuFile.ReadUInt32();
            ////cpuFile.Skip(4); //Don't know if this skip is necessary but the game code seems to do it.
            //cpuFile.Align(16);

            //cpuFile.Skip(32); //Skip because I don't know what this is
            //cpuFile.Align(16);
            //cpuFile.Skip(maybeNumMaterials * 4); //Todo: confirm correctness
            //cpuFile.Align(16);

            //These are apparently the "side map materials" //Todo: Add code for handling the other materials and errors when they exist so I know to reverse the related files
            for (int i = 0; i < maybeNumMaterials; i++)
            {
                var materialBlock = new MaterialBlock2();
                materialBlock.Read(cpuFile, headerPath);
                Materials.Add(materialBlock);
            }

            if (TerrainData.ShapeHandle != 0xFFFFFFFF)
            {
                long tagfileStartOffset = cpuFile.BaseStream.Position;
                uint maybeHavokBinaryTagfileSig = cpuFile.ReadUInt32();
                if (maybeHavokBinaryTagfileSig != 1212891981)
                    throw new Exception($"Invalid havok binary tagfile sig in {Path.GetFileName(headerPath)}");

                cpuFile.Skip(4);
                uint tagfileSize = cpuFile.ReadUInt32();
                uint maybeNumCollisionModels = cpuFile.ReadUInt32();

                //Skip havok tagfile data for now until it's understood
                cpuFile.BaseStream.Seek(tagfileStartOffset + tagfileSize, SeekOrigin.Begin);
            }


            //Todo: Figure out the madness behind these skips and what data they might represent
            cpuFile.Align(4);
            cpuFile.Skip(TerrainData.NumSubzones * 4);
            if (TerrainData.NumSidemapMaterials == 0)
            {
                //throw new Exception($"NumSidemapMaterials == 0. Testing hasn't encountered a file like this yet. Show this to the maintainer with the filename \"{Path.GetFileName(headerPath)}\"");
                var debug = 2;
            }

            if (TerrainData.NumSidemapMaterials > 0)
            {
                cpuFile.Skip(8);
                cpuFile.Skip(TerrainData.NumSidemapMaterials * 4 * 2);

                uint unkStringList2Size = cpuFile.ReadUInt32();
                UnkStringList2 = cpuFile.ReadSizedStringList(unkStringList2Size);
                cpuFile.Align(16);

                //Todo: Reverse this data block. My guess is that it's more material data
                var maybeSidemapMaterials = new MaterialBlock2();
                maybeSidemapMaterials.Read(cpuFile, headerPath);

                uint unkStringList3Size = cpuFile.ReadUInt32(); //Todo: Figure out if this is the texture list for the material right before it
                UnkStringList3 = cpuFile.ReadSizedStringList(unkStringList3Size);
                cpuFile.Align(16); //Todo: Figure out if this is correct or if you really align(4) + skip(4)

                //Todo: Reverse this data block
                uint unkDataBlockSize2 = cpuFile.ReadUInt32(); //Probably more material data
                cpuFile.Skip(unkDataBlockSize2 - 4);

                uint unkStringList4Size = cpuFile.ReadUInt32();
                UnkStringList4 = cpuFile.ReadSizedStringList(unkStringList4Size);
                cpuFile.Align(16); //Todo: Figure out if this is correct or if you really align(4) + skip(4)

                //Todo: Reverse this data block
                uint unkDataBlockSize3 = cpuFile.ReadUInt32(); //Probably more material data
                cpuFile.Skip(unkDataBlockSize3 - 4);
            }

            //cpuFile.Skip(8);
            //cpuFile.Skip(TerrainData.NumSidemapMaterials * 4 * 2);

            //uint unkStringList2Size = cpuFile.ReadUInt32();
            //UnkStringList2 = cpuFile.ReadSizedStringList(unkStringList2Size);
            //cpuFile.Align(16);

            ////Todo: Reverse this data block. My guess is that it's more material data
            //var maybeSidemapMaterials = new MaterialBlock2();
            //maybeSidemapMaterials.Read(cpuFile, headerPath);

            //uint unkStringList3Size = cpuFile.ReadUInt32(); //Todo: Figure out if this is the texture list for the material right before it
            //UnkStringList3 = cpuFile.ReadSizedStringList(unkStringList3Size);
            //cpuFile.Align(16); //Todo: Figure out if this is correct or if you really align(4) + skip(4)

            ////Todo: Reverse this data block
            //uint unkDataBlockSize2 = cpuFile.ReadUInt32(); //Probably more material data
            //cpuFile.Skip(unkDataBlockSize2 - 4);

            //uint unkStringList4Size = cpuFile.ReadUInt32();
            //UnkStringList4 = cpuFile.ReadSizedStringList(unkStringList4Size);
            //cpuFile.Align(16); //Todo: Figure out if this is correct or if you really align(4) + skip(4)

            ////Todo: Reverse this data block
            //uint unkDataBlockSize3 = cpuFile.ReadUInt32(); //Probably more material data
            //cpuFile.Skip(unkDataBlockSize3 - 4);

            //Seems to be navmesh / pathfinding data
            uint maybeNumNavmeshes = cpuFile.ReadUInt32();
            uint maybeNavmeshSize = cpuFile.ReadUInt32();
            cpuFile.Skip(maybeNavmeshSize - 4);
            cpuFile.Align(16);

            //Todo: Read header and validate. Move this and other havok things into a class instead of duplicating
            long tagfileStartOffset2 = cpuFile.BaseStream.Position;
            cpuFile.Skip(8);
            uint tagfileSize2 = cpuFile.ReadUInt32();
            cpuFile.BaseStream.Seek(tagfileStartOffset2 + tagfileSize2, SeekOrigin.Begin);
            cpuFile.Align(4);

            //Some kind of invisible barriers data
            cpuFile.Skip(TerrainData.NumInvisibleBarriers * 8);
            cpuFile.Align(16);

            //Another havok tagfile, probably for the invisible barriers if I were to guess
            //Todo: Read header and validate. Move this and other havok things into a class instead of duplicating
            long tagfileStartOffset3 = cpuFile.BaseStream.Position;
            cpuFile.Skip(8);
            uint tagfileSize3 = cpuFile.ReadUInt32();
            cpuFile.BaseStream.Seek(tagfileStartOffset3 + tagfileSize3, SeekOrigin.Begin);
            
            cpuFile.Align(4);
            cpuFile.Skip(TerrainData.NumInvisibleBarriers * 8); //Todo: Determine if this varies and if this is correct

            //Layer map data. Seems to have BitDepth * ResX * ResY bits //Todo: Read the values of these bits
            cpuFile.Skip(TerrainData.LayerMap.DataSize);
            //Todo: Read values of these
            cpuFile.Skip(TerrainData.LayerMap.NumMaterials * 4);
            var layerMapMaterialNames = new List<string>();
            for (int i = 0; i < TerrainData.LayerMap.NumMaterials; i++)
            {
                layerMapMaterialNames.Add(cpuFile.ReadNullTerminatedString());
            }
            cpuFile.Align(4);
            cpuFile.Skip(TerrainData.LayerMap.NumMaterials * 4); //Todo: Figure out what this is, just following the decomp as a first pass

            if (TerrainData.NumUndergrowthLayers > 0)
            {
                //Todo: RE this data, some kind of undergrowth layer data / ug material data
                cpuFile.Skip(TerrainData.NumUndergrowthLayers * 28);
                //apparently some kind of ug layer material data

                cpuFile.Skip(TerrainData.NumUndergrowthLayers * 48); //Todo: Get better calculation here, likely varies in size
                FmeshNames = cpuFile.ReadSizedStringList(FmeshNamesSize);
                cpuFile.Align(4);
                cpuFile.Skip(16384); //Decomp does this, dunno why yet //Todo: Figure out why (holy shit so many todo's)

                //Undergrowth cell data list //Todo: Read this in
                cpuFile.Skip(TerrainData.NumUndergrowthCellLayerDatas * 10);
                cpuFile.Align(4);
                //More ug data //Todo: RE this data
                cpuFile.Skip(TerrainData.NumUndergrowthCellLayerDatas * 12);
                //Seems to be ug material data
                cpuFile.Skip(TerrainData.NumUndergrowthCellLayerDatas * 6);//Todo: Get better calculation here, don't think this is accurate
            }
            cpuFile.Align(4);

            uint unkStringList5Size = cpuFile.ReadUInt32();
            UnkStringList5 = cpuFile.ReadSizedStringList(unkStringList5Size);
            cpuFile.Align(16); //Todo: Figure out if this is correct or if you really align(4) + skip(4)

            //Todo: Reverse this data block. My guess is that it's more material data
            var maybeMinimapMaterials = new MaterialBlock2();
            maybeMinimapMaterials.Read(cpuFile, headerPath);
            cpuFile.Skip(432); //Hardcoded skip that the game also makes. Shouldn't vary

            var d = 2;

            //Read mesh data
            for(int i = 0; i < 9; i++) //Always 9 meshes inside a cterrain/gterrain file set
            {
                var meshData = new MeshDataBlock();
                meshData.Read(cpuFile, headerPath);
                MeshData.Add(meshData);
            }

            var a = 2;
        }

        //public void WriteAllCombinedMeshToObj(string dataPath, string outputPath)
        //{
        //    //Open gpu file
        //    using var gpuFileStream = new FileStream(dataPath, FileMode.Open);
        //    using var gpuFile = new BinaryReader(gpuFileStream);

        //    //Get all indices and vertices and write them to an obj. Each mesh will be it's own sub-object in the obj file
        //    var (indicesAll, verticesAll) = GetAllMeshData(gpuFile);
        //    WriteMeshToObj(indicesAll, verticesAll, outputPath);
        //}

        public (List<List<ushort>>, List<List<vector4f>>) GetAllMeshData(string gpuFilePath)
        {
            using var gpuFileStream = new FileStream(gpuFilePath, FileMode.Open);
            using var gpuFile = new BinaryReader(gpuFileStream);
            return GetAllMeshData(gpuFile);
        }

        public (List<List<ushort>>, List<List<vector4f>>) GetAllMeshData(BinaryReader gpuFile)
        {
            var indicesAll = new List<List<ushort>>();
            var verticesAll = new List<List<vector4f>>();
            foreach (var meshData in MeshData)
            {
                var (indices, vertices) = GetMeshData(meshData, gpuFile);
                indicesAll.Add(indices);
                verticesAll.Add(vertices);
            }

            return (indicesAll, verticesAll);
        }

        public (List<ushort>, List<vector4f>) GetMeshData(MeshDataBlock mesh, BinaryReader gpuFile)
        {
            var indices = new List<ushort>();
            var vertices = new List<vector4f>();

            //Get start pos for proper offsetting
            long startPos = gpuFile.BaseStream.Position;
            //Read start crc
            uint blockStartCrc = gpuFile.ReadUInt32();

            //Seek to indices and read them
            gpuFile.BaseStream.Seek(startPos + mesh.IndexBufferConfig.IndicesOffset, SeekOrigin.Begin);
            for (int i = 0; i < mesh.IndexBufferConfig.NumIndices; i++)
            {
                indices.Add(gpuFile.ReadUInt16());
            }

            //Seek to vertices and read them
            gpuFile.BaseStream.Seek(startPos + mesh.VertexBufferConfig.VertexOffset, SeekOrigin.Begin);
            for (int i = 0; i < mesh.VertexBufferConfig.NumVerts; i++)
            {
                var vertex = new vector4f(gpuFile.ReadInt16(), gpuFile.ReadInt16(), gpuFile.ReadInt16(), gpuFile.ReadInt16());
                vertices.Add(vertex);
            }

            //Read end crc and compare with start to catch read errors
            uint blockEndCrc = gpuFile.ReadUInt32();
            if (blockStartCrc != blockEndCrc)
                throw new Exception($"Failed to read mesh data from gterrain_pc file. Mesh verification CRCs do not match!");

            return (indices, vertices);
        }
    }
}
