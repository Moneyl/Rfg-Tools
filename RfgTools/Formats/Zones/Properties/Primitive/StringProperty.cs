using System.IO;
using System.Xml.Linq;
using RfgTools.Formats.Zones.Interfaces;
using RfgTools.Helpers;

namespace RfgTools.Formats.Zones.Properties.Primitive
{
    /// <summary>
    /// Preset property used for type 4 properties. They're all just null terminated strings.
    /// Doesn't have <see cref="PropertyAttribute"/> so <see cref="PropertyManager"/> ignores it.
    /// </summary>
    public class StringProperty : IProperty
    {
        public string StringData { get; private set; }
        public virtual string Name { get; protected set; } = "unknown";
        public virtual string TypeString { get; protected set; } = "string";

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

            var reader = new BinaryReader(stream);
            StringData = reader.ReadFixedLengthString((int)size);
            if (reader.BaseStream.Position < reader.BaseStream.Length - 2 && reader.PeekUshort() == 0)
                reader.Skip(1); //Special case, sometimes strings have a null terminator following 'Size' bytes. Skip null terminator if present

            return true;
        }

        public void WriteToStream(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(Type);
            writer.Write(Size);
            writer.Write(NameHash);

            writer.WriteAsciiString(StringData, true);
        }

        public void ReadFromXml(XElement propertyRoot, ushort type, ushort size, uint nameHash)
        {
            Type = type;
            Size = size;
            NameHash = nameHash;

            StringData = propertyRoot.GetRequiredValue("Data");
        }

        public XElement WriteToXml()
        {
            string outputString = StringData; //Make a copy of the string so it can be modified for output
            while (outputString.Length > 0 && outputString[^1] == '\0')
            {
                outputString = outputString.Remove(outputString.Length - 1); //Remove null-terminators if present. Can cause crashes/odd behavior with xml writing
            }
            var propertyRoot = new XElement("Property");
            PropertyManager.WritePropertyInfoToXml(propertyRoot, Type, Size, NameHash, Name, TypeString);

            propertyRoot.Add(new XElement("Data", outputString));
            return propertyRoot;
        }
    }


    //All string properties below
    [Property("district", 4)]
    class DistrictProperty : StringProperty
    {
        public override string Name { get; protected set; } = "district";
    }

    [Property("terrain_file_name", 4)]
    class TerrainFileNameProperty : StringProperty
    {
        public override string Name { get; protected set; } = "terrain_file_name";
    }

    [Property("ambient_spawn", 4)]
    class AmbientSpawnProperty : StringProperty
    {
        public override string Name { get; protected set; } = "ambient_spawn";
    }

    [Property("mission_info", 4)]
    class MissionInfoProperty : StringProperty
    {
        public override string Name { get; protected set; } = "mission_info";
    }

    [Property("mp_team", 4)]
    class MpTeamProperty : StringProperty
    {
        public override string Name { get; protected set; } = "mp_team";
    }

    [Property("item_type", 4)]
    class ItemTypeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "item_type";
    }

    [Property("default_orders", 4)]
    class DefaultOrdersProperty : StringProperty
    {
        public override string Name { get; protected set; } = "default_orders";
    }

    [Property("squad_def", 4)]
    class SquadDefProperty : StringProperty
    {
        public override string Name { get; protected set; } = "squad_def";
    }

    [Property("respawn_speed", 4)]
    class RespawnSpeedProperty : StringProperty
    {
        public override string Name { get; protected set; } = "respawn_speed";
    }

    [Property("vehicle_type", 4)]
    class VehicleTypeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "vehicle_type";
    }

    [Property("spawn_set", 4)]
    class SpawnSetProperty : StringProperty
    {
        public override string Name { get; protected set; } = "spawn_set";
    }

    [Property("chunk_name", 4)]
    class ChunkNameProperty : StringProperty
    {
        public override string Name { get; protected set; } = "chunk_name";
    }

    [Property("props", 4)]
    class PropsProperty : StringProperty
    {
        public override string Name { get; protected set; } = "props";
    }

    [Property("building_type", 4)]
    class BuildingTypeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "building_type";
    }

    [Property("gameplay_props", 4)]
    class GameplayPropsProperty : StringProperty
    {
        public override string Name { get; protected set; } = "gameplay_props";
    }

    [Property("chunk_flags", 4)]
    class ChunkFlagsProperty : StringProperty
    {
        public override string Name { get; protected set; } = "chunk_flags";
    }

    [Property("display_name", 4)]
    class DisplayNameProperty : StringProperty
    {
        public override string Name { get; protected set; } = "display_name";
    }

    [Property("turret_type", 4)]
    class TurretTypeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "turret_type";
    }

    [Property("animation_type", 4)]
    class AnimationTypeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "animation_type";
    }

    [Property("weapon_type", 4)]
    class WeaponTypeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "weapon_type";
    }

    [Property("bounding_box_type", 4)]
    class BoundingBoxTypeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "bounding_box_type";
    }

    [Property("trigger_shape", 4)]
    class TriggerShapeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "trigger_shape";
    }

    [Property("trigger_flags", 4)]
    class TriggerFlagsProperty : StringProperty
    {
        public override string Name { get; protected set; } = "trigger_flags";
    }

    [Property("region_kill_type", 4)]
    class RegionKillTypeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "region_kill_type";
    }

    [Property("region_type", 4)]
    class RegionTypeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "region_type";
    }

    [Property("convoy_type", 4)]
    class ConvoyTypeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "convoy_type";
    }

    [Property("home_district", 4)]
    class HomeDistrictProperty : StringProperty
    {
        public override string Name { get; protected set; } = "home_district";
    }

    [Property("raid_type", 4)]
    class RaidTypeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "raid_type";
    }

    [Property("house_arrest_type", 4)]
    class HouseArrestTypeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "house_arrest_type";
    }

    [Property("activity_type", 4)]
    class ActivityTypeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "activity_type";
    }

    [Property("delivery_type", 4)]
    class DeliveryTypeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "delivery_type";
    }

    [Property("courier_type", 4)]
    class CourierTypeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "courier_type";
    }

    [Property("streamed_effect", 4)]
    class StreamedEffectProperty : StringProperty
    {
        public override string Name { get; protected set; } = "streamed_effect";
    }

    [Property("display_name_tag", 4)]
    class DisplayNameTagProperty : StringProperty
    {
        public override string Name { get; protected set; } = "display_name_tag";
    }

    [Property("upgrade_type", 4)]
    class UpgradeTypeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "upgrade_type";
    }

    [Property("riding_shotgun_type", 4)]
    class RidingShotgunTypeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "riding_shotgun_type";
    }

    [Property("area_defense_type", 4)]
    class AreaDefenseTypeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "area_defense_type";
    }

    [Property("dummy_type", 4)]
    class DummyTypeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "dummy_type";
    }

    [Property("demolitions_master_type", 4)]
    class DemolitionsMasterTypeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "demolitions_master_type";
    }

    [Property("team", 4)]
    class TeamProperty : StringProperty
    {
        public override string Name { get; protected set; } = "team";
    }

    [Property("sound_alr", 4)]
    class SoundAlrProperty : StringProperty
    {
        public override string Name { get; protected set; } = "sound_alr";
    }

    [Property("sound", 4)]
    class SoundProperty : StringProperty
    {
        public override string Name { get; protected set; } = "sound";
    }

    [Property("visual", 4)]
    class VisualProperty : StringProperty
    {
        public override string Name { get; protected set; } = "visual";
    }

    [Property("behavior", 4)]
    class BehaviorProperty : StringProperty
    {
        public override string Name { get; protected set; } = "behavior";
    }

    [Property("roadblock_type", 4)]
    class RoadblockTypeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "roadblock_type";
    }

    [Property("type_enum", 4)]
    class TypeEnumProperty : StringProperty
    {
        public override string Name { get; protected set; } = "type_enum";
    }

    [Property("clip_mesh", 4)]
    class ClipMeshProperty : StringProperty
    {
        public override string Name { get; protected set; } = "clip_mesh";
    }

    [Property("light_flags", 4)]
    class LightFlagsProperty : StringProperty
    {
        public override string Name { get; protected set; } = "light_flags";
    }

    [Property("backpack_type", 4)]
    class BackpackTypeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "backpack_type";
    }

    [Property("marker_type", 4)]
    class MarkerTypeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "marker_type";
    }

    [Property("area_type", 4)]
    class AreaTypeProperty : StringProperty
    {
        public override string Name { get; protected set; } = "area_type";
    }

    [Property("spawn_resource_data", 4)]
    class SpawnResourceDataProperty : StringProperty
    {
        public override string Name { get; protected set; } = "spawn_resource_data";
    }

    [Property("parent_name", 4)]
    class ParentNameProperty : StringProperty
    {
        public override string Name { get; protected set; } = "parent_name";
    }
}
