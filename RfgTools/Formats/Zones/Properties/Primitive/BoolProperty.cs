using System;
using System.IO;
using System.Xml.Linq;
using RfgTools.Formats.Zones.Interfaces;
using RfgTools.Helpers;

namespace RfgTools.Formats.Zones.Properties.Primitive
{
    /// <summary>
    /// Used for bool properties. Provides simple behavior for handling a single bool since it's so common.
    /// All properties consisting of 1 bool so far have had the type of 5.
    /// Doesn't have <see cref="PropertyAttribute"/> so <see cref="PropertyManager"/> ignores it.
    /// </summary>
    public class BoolProperty : IProperty
    {
        public bool Data { get; private set; } = false;
        public virtual string Name { get; protected set; } = "unknown";
        public virtual string TypeString { get; protected set; } = "bool";

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

            if (size == 1)
            {
                var reader = new BinaryReader(stream);
                Data = reader.ReadBoolean();
                return true;
            }
            else
            {
                Console.WriteLine($"Error! Found bool property({Name}) with size != 1 byte. Unknown data! Skipping property.");
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

            Data = propertyRoot.GetRequiredValue("Data").ToBool();
        }

        public XElement WriteToXml()
        {
            var propertyRoot = new XElement("Property");
            PropertyManager.WritePropertyInfoToXml(propertyRoot, Type, Size, NameHash, Name, TypeString);

            propertyRoot.Add(new XElement("Data", Data));
            return propertyRoot;
        }
    }


    //All bool properties below
    [Property("respawn", 5)]
    class RespawnProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "respawn";
    }

    [Property("respawns", 5)]
    class RespawnsProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "respawns";
    }

    [Property("checkpoint_respawn", 5)]
    class CheckpointRespawnProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "checkpoint_respawn";
    }

    [Property("initial_spawn", 5)]
    class InitialSpawnProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "initial_spawn";
    }

    [Property("activity_respawn", 5)]
    class ActivityRespawnProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "activity_respawn";
    }

    [Property("special_npc", 5)]
    class SpecialNpcProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "special_npc";
    }

    [Property("safehouse_vip", 5)]
    class SafehouseVipProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "safehouse_vip";
    }

    [Property("special_vehicle", 5)]
    class SpecialVehicleProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "special_vehicle";
    }

    [Property("hands_off_raid_squad", 5)]
    class HandsOffRaidSquadProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "hands_off_raid_squad";
    }

    [Property("radio_operator", 5)]
    class RadioOperatorProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "radio_operator";
    }

    [Property("squad_vehicle", 5)]
    class SquadVehicleProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "squad_vehicle";
    }

    [Property("miner_persona", 5)]
    class MinerPersonaProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "miner_persona";
    }

    [Property("raid_spawn", 5)]
    class RaidSpawnProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "raid_spawn";
    }

    [Property("no_reassignment", 5)]
    class NoReassignmentProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "no_reassignment";
    }

    [Property("disable_ambient_parking", 5)]
    class DisableAmbientParkingProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "disable_ambient_parking";
    }

    [Property("player_vehicle_respawn", 5)]
    class PlayerVehicleRespawnProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "player_vehicle_respawn";
    }

    [Property("no_defensive_combat", 5)]
    class NoDefensiveCombatProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "no_defensive_combat";
    }

    [Property("preplaced", 5)]
    class PreplacedProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "preplaced";
    }

    [Property("enabled", 5)]
    class EnabledProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "enabled";
    }

    [Property("indoor", 5)]
    class IndoorProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "indoor";
    }

    [Property("no_stub", 5)]
    class NoStubProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "no_stub";
    }

    [Property("autostart", 5)]
    class AutostartProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "autostart";
    }

    [Property("high_priority", 5)]
    class HighPriorityProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "high_priority";
    }

    [Property("run_to", 5)]
    class RunToProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "run_to";
    }

    [Property("infinite_duration", 5)]
    class InfiniteDurationProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "infinite_duration";
    }

    [Property("no_check_in", 5)]
    class NoCheckInProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "no_check_in";
    }

    [Property("combat_ready", 5)]
    class CombatReadyProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "combat_ready";
    }

    [Property("looping_patrol", 5)]
    class LoopingPatrolProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "looping_patrol";
    }

    [Property("marauder_raid", 5)]
    class MarauderRaidProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "marauder_raid";
    }

    [Property("ASD_truck_partol", 5)] //The game spelled it this way, can't change it unless we want to break the hash generated from it.
    class ASDTruckPartolProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "ASD_truck_partol";
    }

    [Property("courier_patrol", 5)]
    class CourierPatrolProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "courier_patrol";
    }

    [Property("override_patrol", 5)]
    class OverridePatrolProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "override_patrol";
    }

    [Property("allow_ambient_peds", 5)]
    class AllowAmbientPedsProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "allow_ambient_peds";
    }

    [Property("disabled", 5)]
    class DisabledProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "disabled";
    }

    [Property("tag_node", 5)]
    class TagNodeProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "tag_node";
    }

    [Property("start_node", 5)]
    class StartNodeProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "start_node";
    }

    [Property("end_game_only", 5)]
    class EndGameOnlyProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "end_game_only";
    }

    [Property("visible", 5)]
    class VisibleProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "visible";
    }

    [Property("vehicle_only", 5)]
    class VehicleOnlyProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "vehicle_only";
    }

    [Property("npc_only", 5)]
    class NpcOnlyProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "npc_only";
    }

    [Property("dead_body", 5)]
    class DeadBodyProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "dead_body";
    }

    [Property("looping", 5)]
    class LoopingProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "looping";
    }

    [Property("use_object_orient", 5)]
    class UseObjectOrientProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "use_object_orient";
    }

    [Property("random_backpacks", 5)]
    class RandomBackpacksProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "random_backpacks";
    }

    [Property("liberated", 5)]
    class LiberatedProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "liberated";
    }

    [Property("liberated_play_line", 5)]
    class LiberatedPlayLineProperty : BoolProperty
    {
        public override string Name { get; protected set; } = "liberated_play_line";
    }
}
