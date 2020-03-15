using System;
using System.Collections.Generic;
using System.Text;

namespace RfgTools.Types
{
    [Flags]
    public enum DistrictFlags : byte
    {
        None = 0,
        AllowCough = 1,
        AllowAmbEdfCivilianDump = 2,
        PlayCapstoneUnlockedLines = 4,
        DisableMoraleChange = 8,
        DisableControlChange = 16,
    }
}
