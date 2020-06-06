using RfgTools.Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace RfgTools.Formats.Animation
{
    //Class for interacting with animation rig files (.rig_pc)
    public class AnimationRig
    {
        public uint Flags;
        public uint NumBones;
        public uint NumCommonBones;
        public uint NumVirtualBones;
        public uint NumTags;

        //Todo: If these are zero in all rig files (both main campaign and DLC) then remove them
        public uint BoneNameChecksumsOffset;
        public uint BonesOffset;
        public uint TagsOffset;

        public List<uint> BoneNameHashes;
        public List<Bone> Bones;
        public List<Tag> Tags;
        public List<string> BoneNames;
        public List<string> TagNames;

        public void Read(string inputPath)
        {
            using var fileStream = new FileStream(inputPath, FileMode.Open);
            using var input = new BinaryReader(fileStream);

            //Game may interpret this as a name but it's so far always been garbage data
            input.Skip(32);

            Flags = input.ReadUInt32();
            NumBones = input.ReadUInt32();
            NumCommonBones = input.ReadUInt32();
            NumVirtualBones = input.ReadUInt32();
            NumTags = input.ReadUInt32();

            BoneNameChecksumsOffset = input.ReadUInt32();
            BonesOffset = input.ReadUInt32();
            TagsOffset = input.ReadUInt32();

            if(!(BoneNameChecksumsOffset == 0 && BonesOffset == 0 && TagsOffset == 0))
                throw new Exception("One of the offset values isn't zero! Turns out they aren't garbage data.");

            BoneNameHashes = new List<uint>();
            for(int i = 0; i < NumBones; i++)
                BoneNameHashes.Add(input.ReadUInt32());

            Bones = new List<Bone>();
            for(int i = 0; i < NumBones; i++)
            {
                var bone = new Bone();
                bone.Read(input);
                Bones.Add(bone);
            }

            Tags = new List<Tag>();
            for(int i = 0; i < NumTags; i++)
            {
                var tag = new Tag();
                tag.Read(input);
                Tags.Add(tag);
            }

            //Position to base name offsets from
            long namesStartOffset = input.BaseStream.Position;

            //Read bone and tag names from the end of the rig_pc file
            BoneNames = new List<string>();
            TagNames = new List<string>();
            
            //From what I've seen so far bone names always come first
            foreach(var bone in Bones)
            {
                input.BaseStream.Seek(namesStartOffset + bone.NameOffset, SeekOrigin.Begin);
                BoneNames.Add(input.ReadNullTerminatedString());
            }
            //After bone names are tag names. Really doesn't matter. This implementation works in either direction due to using offsets
            foreach(var tag in Tags)
            {
                input.BaseStream.Seek(namesStartOffset + tag.NameOffset, SeekOrigin.Begin);
                TagNames.Add(input.ReadNullTerminatedString());
            }
        }
    }
}