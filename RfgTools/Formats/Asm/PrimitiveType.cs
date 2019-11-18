
namespace RfgTools.Formats.Asm
{
    public enum PrimitiveType : byte
    {
        None = 0,
        Peg = 1,
        Chunk = 2,
        Zone = 3,
        Terrain = 4,
        StaticMesh = 5,
        CharacterMesh = 6,
        FoliageMesh = 7,
        Material = 8,
        ClothSim = 9,
        Vehicle = 10,
        VehicleAudio = 11,
        Vfx = 12,
        Wavebank = 13,
        FoleyBank = 14,
        MeshMorph = 15,
        VoicePersona = 16,
        AnimFile = 17,
        Vdoc = 18,
        LuaScript = 19,
        Localization = 20,
        TerrainHighLod = 21,
        LandmarkLod = 22,
        NumPrimitiveTypes = 23,
    }
}
