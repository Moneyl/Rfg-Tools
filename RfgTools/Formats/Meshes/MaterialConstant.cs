using System.IO;

namespace RfgTools.Formats.Meshes
{
    public class MaterialConstant
    {
        public float[] Constants = new float[4];

        public void Read(BinaryReader data)
        {
            Constants[0] = data.ReadSingle();
            Constants[1] = data.ReadSingle();
            Constants[2] = data.ReadSingle();
            Constants[3] = data.ReadSingle();
        }
    }
}
