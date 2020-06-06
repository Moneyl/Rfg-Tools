using RfgTools.Helpers;
using RfgTools.Types;
using System.IO;

namespace RfgTools.Formats.Animation
{
    //Bone used in animation rig
    public class Bone
    {
        public uint NameOffset;
        public vector3f InverseTranslation;
        public vector3f Translation;
        public uint ParentIndex;
        public uint Vid;

        public void Read(BinaryReader data)
        {
            NameOffset = data.ReadUInt32();
            InverseTranslation = data.ReadVector3f();
            Translation = data.ReadVector3f();
            ParentIndex = data.ReadUInt32();
            Vid = data.ReadUInt32();
        }
    }
}