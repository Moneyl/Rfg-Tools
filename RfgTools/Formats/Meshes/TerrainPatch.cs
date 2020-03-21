using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RfgTools.Helpers;
using RfgTools.Types;

namespace RfgTools.Formats.Meshes
{
    public class TerrainPatch
    {
        public uint InstanceOffset; //Likely a ptr set at runtime
        public vector3f Position;
        public matrix33 Rotation;
        public uint SubmeshIndex;
        public vector3f LocalAabbMin;
        public vector3f LocalAabbMax;
        public vector3f LocalBspherePosition;
        public float LocalBsphereRadius;

        public void Read(BinaryReader data)
        {
            InstanceOffset = data.ReadUInt32();
            Position = data.ReadVector3f();
            Rotation = data.ReadMatrix33();
            SubmeshIndex = data.ReadUInt32();
            LocalAabbMin = data.ReadVector3f();
            LocalAabbMax = data.ReadVector3f();
            LocalBspherePosition = data.ReadVector3f();
            LocalBsphereRadius = data.ReadSingle();
        }
    }
}
