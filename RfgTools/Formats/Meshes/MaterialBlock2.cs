using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RfgTools.Helpers;

namespace RfgTools.Formats.Meshes
{
    //Todo: Reconcile this with the MaterialBlock class used by static meshes. I believe these to be identical but need see the similarities
    public class MaterialBlock2
    {
        public MaterialData MaterialData;
        public List<TextureDesc> TextureDescs;
        public List<uint> ConstantNameChecksums;
        public List<MaterialConstant> Constants;

        public void Read(BinaryReader cpuFile, string headerPath)
        {
            long startPos = cpuFile.BaseStream.Position;
            uint materialDataSize = cpuFile.ReadUInt32();
            MaterialData = new MaterialData();
            MaterialData.Read(cpuFile);

            //Read material texture descs
            TextureDescs = new List<TextureDesc>();
            for (int i = 0; i < MaterialData.NumTextures; i++)
            {
                var desc = new TextureDesc();
                desc.Read(cpuFile);
                TextureDescs.Add(desc);
            }
            //Todo: Figure out if need to align here

            //Read constant name checksums //Todo: Attempt to get the strings which generated these. Likely use HashGuesser class
            ConstantNameChecksums = new List<uint>();
            for (int i = 0; i < MaterialData.NumConstants; i++)
            {
                ConstantNameChecksums.Add(cpuFile.ReadUInt32());
            }
            cpuFile.Align(16);

            //Read material constant values //Todo: Interpret what the values actually mean
            Constants = new List<MaterialConstant>();
            for (int i = 0; i < MaterialData.MaxConstants; i++)
            {
                var constant = new MaterialConstant();
                constant.Read(cpuFile);
                //Todo: Only read num_constants then skip the rest
                Constants.Add(constant);
            }
            //data.Align(16); //Todo: Confirm that this should align(16) here, also confirm that it matches the texture offset in the header

            if(cpuFile.BaseStream.Position - startPos != materialDataSize)
                throw new Exception($"Failed to read material data block (type 2) from \"{Path.GetFileName(headerPath)}\". Length of data read doesn't equal expected data size.");
        }
    }
}
