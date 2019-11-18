
namespace RfgTools.Formats.Asm
{
    public enum AllocatorType : byte
    {
        None = 0,
        World = 1,
        ChunkPreload = 2,
        EffectPreload = 3,
        EffectCutscene = 4,
        ItemPreload = 5,
        DecalPreload = 6,
        ClothSimPreload = 7,
        Tod = 8,
        MpEffectPreload = 9,
        MpItemPreload = 10,
        Player = 11,
        Human = 12,
        LargeWeapon = 13,
        SmallWeapon = 14,
        Vehicle = 15,
        LargeLayer = 16,
        SmallLayer = 17,
        HumanVoicePersona = 18,
        AlwaysLoadedHumanVoicePersona = 19,
        Audio = 20,
        Interface = 21,
        Fsm = 22,
        InterfaceStack = 23,
        InterfaceSlot = 24,
        InterfaceMpPreload = 25,
        InterfaceMpSlot = 26,
        MaterialEffect = 27,
        Permanent = 28,
        DlcEffectPreload = 29,
        DlcItemPreload = 30,
        NumAllocatorTypes = 31,
    }
}
