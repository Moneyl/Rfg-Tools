
namespace RfgTools.Formats.Zones.Properties
{
    /// <summary>
    /// Breakdown of the flags ushort that each zone object has. Unknown flags have a generic name "Flag##"
    /// </summary>
    enum ZoneObjectFlags : ushort
    {
        Flag0 = 1,
        Flag1 = 2,
        Flag2 = 4,
        Flag3 = 8,
        Flag4 = 16,
        Flag5 = 32,
        Flag6 = 64,
        Flag7 = 128,
        Flag8 = 256,
        Flag9 = 512,
        Flag10 = 1024,
        Flag11 = 2048,
        Flag12 = 4096,
        Flag13 = 8192,
        Flag14 = 16384,
        Flag15 = 32768
    }
}
