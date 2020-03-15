using System;
using System.IO;
using System.Xml.Linq;
using RfgTools.Formats.Zones.Interfaces;
using RfgTools.Helpers;

namespace RfgTools.Formats.Zones.Properties.Primitive
{
    /// <summary>
    /// Used for float properties. Provides simple behavior for handling a single float since it's so common.
    /// All properties consisting of 1 float so far have had the type of 5.
    /// Doesn't have <see cref="PropertyAttribute"/> so <see cref="PropertyManager"/> ignores it.
    /// </summary>
    public class FloatProperty : IProperty
    {
        public float Data { get; private set; } = 0.0f;
        public virtual string Name { get; protected set; } = "unknown";
        public virtual string TypeString { get; protected set; } = "float";

        public ushort Type { get; protected set; }
        public ushort Size { get; protected set; }
        public uint NameHash { get; protected set; }

        public string GetFullName() => Name;
        public string GetTypeName() => TypeString;

        public bool ReadFromStream(Stream stream, ushort type, ushort size, uint nameHash)
        {
            Type = type;
            Size = size;
            NameHash = nameHash;

            if (size == 4)
            {
                var reader = new BinaryReader(stream);
                Data = reader.ReadSingle();
                return true;
            }
            else
            {
                Console.WriteLine($"Error! Found float property({Name}) with size != 4 byte. Unknown data! Skipping property.");
                return false;
            }
        }

        public void WriteToStream(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(Type);
            writer.Write(Size);
            writer.Write(NameHash);

            writer.Write(Data);
        }

        public void ReadFromXml(XElement propertyRoot, ushort type, ushort size, uint nameHash)
        {
            Type = type;
            Size = size;
            NameHash = nameHash;

            Data = propertyRoot.GetRequiredValue("Data").ToSingle();
        }

        public XElement WriteToXml()
        {
            var propertyRoot = new XElement("Property");
            PropertyManager.WritePropertyInfoToXml(propertyRoot, Type, Size, NameHash, Name, TypeString);

            propertyRoot.Add(new XElement("Data", Data));
            return propertyRoot;
        }
    }


    //All float properties below
    [Property("wind_min_speed", 5)]
    class WindMinSpeedProperty : FloatProperty
    {
        public override string Name { get; protected set; } = "wind_min_speed";
    }

    [Property("wind_max_speed", 5)]
    class WindMaxSpeedProperty : FloatProperty
    {
        public override string Name { get; protected set; } = "wind_max_speed";
    }

    [Property("spawn_prob", 5)]
    class SpawnPropProperty : FloatProperty
    {
        public override string Name { get; protected set; } = "spawn_prob";
    }

    [Property("night_spawn_prob", 5)]
    class NightSpawnProbProperty : FloatProperty
    {
        public override string Name { get; protected set; } = "night_spawn_prob";
    }

    [Property("angle_left", 5)]
    class AngleLeftProperty : FloatProperty
    {
        public override string Name { get; protected set; } = "angle_left";
    }

    [Property("angle_right", 5)]
    class AngleRightProperty : FloatProperty
    {
        public override string Name { get; protected set; } = "angle_right";
    }

    [Property("rotation_limit", 5)]
    class RotationLimitProperty : FloatProperty
    {
        public override string Name { get; protected set; } = "rotation_limit";
    }

    [Property("game_destroyed_pct", 5)]
    class GameDestroyedPercentageProperty : FloatProperty
    {
        public override string Name { get; protected set; } = "game_destroyed_pct";
    }

    [Property("outer_radius", 5)]
    class OuterRadiusProperty : FloatProperty
    {
        public override string Name { get; protected set; } = "outer_radius";
    }

    [Property("night_trigger_prob", 5)]
    class NightTriggerProbProperty : FloatProperty
    {
        public override string Name { get; protected set; } = "night_trigger_prob";
    }

    [Property("day_trigger_prob", 5)]
    class DayTriggerProbProperty : FloatProperty
    {
        public override string Name { get; protected set; } = "day_trigger_prob";
    }

    [Property("speed_limit", 5)]
    class SpeedLimitProperty : FloatProperty
    {
        public override string Name { get; protected set; } = "speed_limit";
    }

    [Property("hotspot_falloff_size", 5)]
    class HotspotFalloffSizeProperty : FloatProperty
    {
        public override string Name { get; protected set; } = "hotspot_falloff_size";
    }

    [Property("atten_range", 5)]
    class AttenuationRangeProperty : FloatProperty
    {
        public override string Name { get; protected set; } = "atten_range";
    }

    [Property("aspect", 5)]
    class AspectProperty : FloatProperty
    {
        public override string Name { get; protected set; } = "aspect";
    }

    [Property("hotspot_size", 5)]
    class HotspotSizeProperty : FloatProperty
    {
        public override string Name { get; protected set; } = "hotspot_size";
    }

    [Property("atten_start", 5)]
    class AttenuationStartProperty : FloatProperty
    {
        public override string Name { get; protected set; } = "atten_start";
    }

    [Property("control", 5)]
    class ControlProperty : FloatProperty
    {
        public override string Name { get; protected set; } = "control";
    }

    [Property("control_max", 5)]
    class ControlMaxProperty : FloatProperty
    {
        public override string Name { get; protected set; } = "control_max";
    }

    [Property("morale", 5)]
    class MoraleProperty : FloatProperty
    {
        public override string Name { get; protected set; } = "morale";
    }

    [Property("morale_max", 5)]
    class MoraleMaxProperty : FloatProperty
    {
        public override string Name { get; protected set; } = "morale_max";
    }
}