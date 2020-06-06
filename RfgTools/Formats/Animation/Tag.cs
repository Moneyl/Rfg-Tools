using RfgTools.Helpers;
using RfgTools.Types;
using System.IO;


namespace RfgTools.Formats.Animation
{
    //Data tag used in animation rigs
    public class Tag
    {
        public uint NameOffset;
        public matrix43 Transform;
        public uint ParentIndex;
        public uint Vid;

        public void Read(BinaryReader data)
        {
            NameOffset = data.ReadUInt32();
            Transform = data.ReadMatrix43();
            ParentIndex = data.ReadUInt32();
            Vid = data.ReadUInt32();
        }
    }
}