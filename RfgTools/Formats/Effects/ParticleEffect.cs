using RfgTools.Helpers;
using System;
using System.IO;

namespace RfgTools.Formats.Effects
{
    public class ParticleEffect
    {
        public uint Signature;
        public uint Version;

        public uint NumExpressions;
        public uint ExpressionsOffset;

        public uint EffectNameOffset;
        public float Duration;
        public float Radius;

        public uint NumBitmaps;
        public uint BitmapsOffset;
        public uint MeshBitmapsOffset;

        public uint NumMeshes;
        public uint MeshesOffset;

        public uint NumEmitters;
        public uint EmittersOffset;

        public uint NumLights;
        public uint LightsOffset;

        public uint NumFilters;
        public uint FiltersOffset;

        public uint NumCoronas;
        public uint CoronasOffset;

        public string EffectName;

        public void Read(string inputPath)
        {
            using var fileStream = new FileStream(inputPath, FileMode.Open);
            using var input = new BinaryReader(fileStream);

            Signature = input.ReadUInt32();
            Version = input.ReadUInt32();

            if(Signature != 1463955767)
                throw new Exception($"Error while reading vfx file {Path.GetFileName(inputPath)}. Invalid signature. Expected 1463955767, found {Signature}");
            if (Version < 21)
                throw new Exception($"Error while reading vfx file {Path.GetFileName(inputPath)}. Invalid version. Expected version < 21, found {Version}");

            NumExpressions = input.ReadUInt32();
            input.Skip(4); //Compiler placed padding

            ExpressionsOffset = input.ReadUInt32();
            input.Skip(4); //Padding added by rfg offset type
            EffectNameOffset = input.ReadUInt32();
            input.Skip(4); //Padding added by rfg offset type

            Duration = input.ReadSingle();
            Radius = input.ReadSingle();
            NumBitmaps = input.ReadUInt32();
            input.Skip(4); //Compiler placed padding

            BitmapsOffset = input.ReadUInt32();
            input.Skip(4); //Padding added by rfg offset type
            MeshBitmapsOffset = input.ReadUInt32();
            input.Skip(4); //Padding added by rfg offset type

            NumMeshes = input.ReadUInt32();
            input.Skip(4); //Compiler placed padding
            MeshesOffset = input.ReadUInt32();
            input.Skip(4); //Padding added by rfg offset type

            NumEmitters = input.ReadUInt32();
            input.Skip(4); //Compiler placed padding
            EmittersOffset = input.ReadUInt32();
            input.Skip(4); //Padding added by rfg offset type

            NumLights = input.ReadUInt32();
            input.Skip(4); //Compiler placed padding
            LightsOffset = input.ReadUInt32();
            input.Skip(4); //Padding added by rfg offset type

            NumFilters = input.ReadUInt32();
            input.Skip(4); //Compiler placed padding
            FiltersOffset = input.ReadUInt32();
            input.Skip(4); //Padding added by rfg offset type

            NumCoronas = input.ReadUInt32();
            input.Skip(4); //Compiler placed padding
            CoronasOffset = input.ReadUInt32();
            input.Skip(4); //Padding added by rfg offset type

            EffectName = input.ReadNullTerminatedString();

            //Todo: Check pading of each offset type (some of these like emitters are gonna be a pain)
            //Todo: Read mesh data if NumMeshes != 0
            //Todo: Read emitter data if NumEmitters != 0

            long pos = input.BaseStream.Position;
            var a = 2;
        }
    }
}
