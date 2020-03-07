using System;
using System.Collections.Generic;
using System.Text;

namespace RfgTools.Formats.Scriptx
{
    public enum ScriptNodeClasses
    {
        Invalid = 0,
        Atom = 1,
        Event = 2,
        Function = 3,
        Variable = 4
    }

    public enum ScriptNodeType
    {
        Void = 0,
        Number = 1,
        String = 2,
        Bool = 3,
        Object = 4,
        ObjectList = 5,
        Point = 6,
        Variable = 7,
        VariableRef = 8,
        Script = 9,
        Condition = 10,
        Action = 11,
        Event = 12,
        Function = 13,
        Delay = 14,
        Min = 15,
        Max = 16,
        Ref = 17,
        Group = 18,
        Repeatable = 19,
        Disabled = 20,
        Managed = 21,
        Timer = 22,
        AirBombHandle = 23,
        GameHandle = 24
    }

    public enum ScriptFunctionTypes
    {
        Generic = 0,
        Init = 1,
        Main = 2
    };

    public enum ScriptPointEffectState
    {
        Invalid = -1,
        Normal = 0,
        Stopped = 1,
        PreDeath = 2,
        Dead = 3,
    };
}
