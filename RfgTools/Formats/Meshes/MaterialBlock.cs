using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RfgTools.Helpers;

namespace RfgTools.Formats.Meshes
{
    public class MaterialBlock
    {
        //Materials
        public List<MaterialData> Materials;

        //Material texture descriptions
        public List<TextureDesc> TextureDescs;

        //Constant name checksums
        public List<uint> ConstantNameChecksums;

        //Constant values
        public List<MaterialConstant> Constants;

        public void Read(BinaryReader data)
        {
            //material map data
            uint unk1 = data.ReadUInt32();
            uint numMaterials = data.ReadUInt32();
            uint unk2 = data.ReadUInt32();
            data.Skip(4);
            uint maybeFirstMaterialOffset = data.ReadUInt32();
            data.Align(16);
            //Debug message used to attempt to disprove that value is an offset since I'm not sure what it is yet
            if (maybeFirstMaterialOffset != data.BaseStream.Position)
                Console.WriteLine($"maybeFirstMaterialOffset ({maybeFirstMaterialOffset}) != data.BaseStream.Position ({data.BaseStream.Position})");

            //material data
            //Todo: Add support for multiple materials, need to find a file with multiple to understand the layout
            if (numMaterials != 1)
                throw new Exception("Error! Meshes with > 1 material are unsupported. Show this to the maintainer so they can add support.");

            uint unk3 = data.ReadUInt32();

            Materials = new List<MaterialData>();
            var material = new MaterialData();
            material.Read(data);
            Materials.Add(material);

            //Read material texture descs
            TextureDescs = new List<TextureDesc>();
            foreach (var mat in Materials)
            {
                for (int i = 0; i < mat.NumTextures; i++)
                {
                    var desc = new TextureDesc();
                    desc.Read(data);
                    TextureDescs.Add(desc);
                }
            }
            //Todo: Figure out if need to align here

            //Read constant name checksums //Todo: Attempt to get the strings which generated these
            ConstantNameChecksums = new List<uint>();
            foreach (var mat in Materials)
            {
                for (int i = 0; i < mat.NumConstants; i++)
                {
                    ConstantNameChecksums.Add(data.ReadUInt32());
                }
            }
            data.Align(16);

            //Read material constant values //Todo: Interpret what the values actually mean
            Constants = new List<MaterialConstant>();
            foreach (var mat in Materials)
            {
                for (int i = 0; i < mat.MaxConstants; i++)
                {
                    var constant = new MaterialConstant();
                    constant.Read(data);
                    //Todo: Only read num_constants then skip the rest
                    Constants.Add(constant);
                }
            }
            //data.Align(16); //Todo: Confirm that this should align(16) here, also confirm that it matches the texture offset in the header
        }
    }
}
