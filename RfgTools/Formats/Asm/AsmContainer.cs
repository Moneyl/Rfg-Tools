using System.Collections.Generic;
using System.IO;
using RfgTools.Helpers;

namespace RfgTools.Formats.Asm
{
    public class AsmContainer
    {
        public string Name { get; set; }
        public ContainerType Type { get; set; }
        public ushort Flags { get; set; } //Todo: Figure out the values for this
        public ushort PrimitiveCount { get; set; }
        public uint DataOffset { get; set; }
        public uint SizeCount { get; set; }
        public uint CompressedSize { get; set; }

        public List<uint> PrimitiveSizes;
        public List<AsmPrimitive> Primitives;

        public AsmContainer()
        {
            PrimitiveSizes = new List<uint>();
            Primitives = new List<AsmPrimitive>();
        }

        public void ReadFromBinary(BinaryReader stream)
        {
            uint nameLength = stream.ReadUInt16(); //Todo: Try to figure out if re-mars-tered imposes a limit of 64 characters as gibbed's code shows for this.
            Name = stream.ReadFixedLengthString(nameLength);
            Type = (ContainerType)stream.ReadByte();
            Flags = stream.ReadUInt16();
            PrimitiveCount = stream.ReadUInt16();
            DataOffset = stream.ReadUInt32();
            SizeCount = stream.ReadUInt32();
            CompressedSize = stream.ReadUInt32();

            for (int i = 0; i < SizeCount; i++)
            {
                PrimitiveSizes.Add(stream.ReadUInt32());
            }

            for (int i = 0; i < PrimitiveCount; i++)
            {
                var primitive = new AsmPrimitive();
                primitive.ReadFromBinary(stream);
                Primitives.Add(primitive);
            }
        }

        public void WriteToBinary(BinaryWriter stream)
        {

        }
    }
}
