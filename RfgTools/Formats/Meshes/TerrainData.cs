using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RfgTools.Helpers;
using RfgTools.Types;

namespace RfgTools.Formats.Meshes
{
    public class TerrainData
    {
        public vector3f Bmin;
        public vector3f Bmax;
        public uint Xres;
        public uint Zres;
        public uint NumOccluders;
        public uint OccludersOffset;
        public uint TerrainMaterialMapOffset;
        public uint TerrainMaterialsOffset;
        public uint NumTerrainMaterials;
        public uint MinimapMaterialHandle;
        public uint MinimapMaterialOffset;
        public uint LowLodPatchesOffset;
        public uint LowLodMaterialOffset;
        public uint LowLodMaterialMapOffset;
        public uint NumSubzones;
        public uint SubzonesOffset;
        public uint PfDataOffset;
        public TerrainLayerMap LayerMap = new TerrainLayerMap();
        public uint NumUndergrowthLayers;
        public uint UndergrowthLayersOffset;
        public uint UndergrowthCellDataOffset;
        public uint NumUndergrowthCellLayerDatas;
        public uint UndergrowthCellLayerDataOffset;
        public uint SingleUndergrowthCellLayerDataOffset;
        public uint StitchPieceCmIndex;
        public uint NumInvisibleBarriers;
        public uint InvisibleBarriersOffset;
        public uint ShapeHandle;
        public uint NumSidemapMaterials;
        public uint SidemapDataOffset;
        public uint ObjectStubOffset;
        public uint StitchPhysicsInstancesOffset;
        public uint NumStitchPhysicsInstances;
        public uint ObjectStubPtr;
        public uint ObjectStubPtrPadding;
        //880 bytes padding

        public void Read(BinaryReader data)
        {
            Bmin = data.ReadVector3f();
            Bmax = data.ReadVector3f();
            Xres = data.ReadUInt32();
            Zres = data.ReadUInt32();
            NumOccluders = data.ReadUInt32();
            OccludersOffset = data.ReadUInt32();
            TerrainMaterialMapOffset = data.ReadUInt32();
            TerrainMaterialsOffset = data.ReadUInt32();
            NumTerrainMaterials = data.ReadUInt32();
            MinimapMaterialHandle = data.ReadUInt32();
            MinimapMaterialOffset = data.ReadUInt32();
            LowLodPatchesOffset = data.ReadUInt32();
            LowLodMaterialOffset = data.ReadUInt32();
            LowLodMaterialMapOffset = data.ReadUInt32();
            NumSubzones = data.ReadUInt32();
            SubzonesOffset = data.ReadUInt32();
            PfDataOffset = data.ReadUInt32();
            LayerMap.Read(data);
            NumUndergrowthLayers = data.ReadUInt32();
            UndergrowthLayersOffset = data.ReadUInt32();
            UndergrowthCellDataOffset = data.ReadUInt32();
            NumUndergrowthCellLayerDatas = data.ReadUInt32();
            UndergrowthCellLayerDataOffset = data.ReadUInt32();
            SingleUndergrowthCellLayerDataOffset = data.ReadUInt32();
            StitchPieceCmIndex = data.ReadUInt32();
            NumInvisibleBarriers = data.ReadUInt32();
            InvisibleBarriersOffset = data.ReadUInt32();
            ShapeHandle = data.ReadUInt32();
            NumSidemapMaterials = data.ReadUInt32();
            SidemapDataOffset = data.ReadUInt32();
            ObjectStubOffset = data.ReadUInt32();
            StitchPhysicsInstancesOffset = data.ReadUInt32();
            NumStitchPhysicsInstances = data.ReadUInt32();
            ObjectStubPtr = data.ReadUInt32();
            ObjectStubPtrPadding = data.ReadUInt32();
            data.Skip(880);
        }
    }
}
