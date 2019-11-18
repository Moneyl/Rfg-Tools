using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using RfgTools.Formats.Zones.Interfaces;
using RfgTools.Formats.Zones.Properties;
using RfgTools.Helpers;
using RfgTools.Types;

namespace RfgTools.Formats.Zones
{
    //Todo: Add functions like "TryGetProperty" and "TrySetProperty" and "HasProperty" etc
    public class ZoneObject : ICloneable
    {
        public uint ClassnameHash;
        public uint Handle;
        public vector3f Bmin; //Min position/corner of it's bounding box
        public vector3f Bmax; //Max position/corner of it's bounding box
        public ushort Flags; //Figure out what the values of the flags mean
        public ushort BlockSize;
        public uint Parent;
        public uint Sibling;
        public uint Child;
        public uint Num;

        public ushort NumProps;
        public ushort Size; //Size of prop data, not including this and previous variables

        public List<IProperty> Properties = new List<IProperty>();

        public string Classname { get; private set; } = "unknown object classname";
        public string Description { get; private set; } = "No description set.";

        public bool TryGetProperty(string fullName, out IProperty outProp)
        {
            outProp = null;
            foreach (var property in Properties)
            {
                if (fullName == property.GetFullName())
                {
                    outProp = property;
                    return true;
                }
            }
            return false;
        }

        public void ReadFromBinary(Stream stream)
        {
            var reader = new BinaryReader(stream);

            //56 byte data section at the start of each zone object. 
            ClassnameHash = reader.ReadUInt32();
            Handle = reader.ReadUInt32();
            Bmin = reader.ReadVector3f(); 
            Bmax = reader.ReadVector3f(); 
            Flags = reader.ReadUInt16();
            BlockSize = reader.ReadUInt16();
            Parent = reader.ReadUInt32();
            Sibling = reader.ReadUInt32();
            Child = reader.ReadUInt32();
            Num = reader.ReadUInt32();
            NumProps = reader.ReadUInt16();
            Size = reader.ReadUInt16();

            for (int i = 0; i < NumProps; i++)
            {
                var prop = PropertyManager.ReadPropertyFromBinary(stream);
                Properties.Add(prop);
            }

            TrySetClassnameString();
            TrySetDescriptionString();
        }

        public void WriteToBinary(Stream stream)
        {
            var writer = new BinaryWriter(stream);

            //Write 56 byte zone object header
            writer.Write(ClassnameHash);
            writer.Write(Handle);
            writer.Write(Bmin);
            writer.Write(Bmax);
            writer.Write((ushort)Flags);
            writer.Write(BlockSize);
            writer.Write(Parent);
            writer.Write(Sibling);
            writer.Write(Child);
            writer.Write(Num);
            writer.Write(NumProps);
            writer.Write(Size);

            //Write properties
            foreach (var property in Properties)
            {
                property.WriteToStream(stream);
                writer.Align(4);
            }
        }

        public void ReadFromXml(XElement element)
        {
            //Read zone object data & convert flags to ushort
            var objectData = element.GetRequiredElement("Data");
            ClassnameHash = objectData.GetRequiredAttributeValue("ClassnameHash").ToUint32();
            Handle = objectData.GetRequiredValue("Handle").ToUint32();  //Todo: Figure out how this value is chosen
            Bmin = objectData.GetRequiredElement("Bmin").ToVector3f();
            Bmax = objectData.GetRequiredElement("Bmax").ToVector3f();
            SetFlagsFromXml(objectData.GetRequiredElement("Flags"));
            BlockSize = objectData.GetRequiredAttributeValue("BlockSize").ToUint16();  //Todo: Update this based on xml data
            Parent = objectData.GetOptionalValue("Parent", "4294967295").ToUint32(); 
            Sibling = objectData.GetOptionalValue("Sibling", "4294967295").ToUint32();
            Child = objectData.GetOptionalValue("Child", "4294967295").ToUint32();
            Num = objectData.GetRequiredValue("Num").ToUint32();  //Todo: Figure out what the hell this means
            NumProps = objectData.GetRequiredAttributeValue("NumProps").ToUint16();  //Todo: Update this based on xml data
            Size = objectData.GetRequiredAttributeValue("Size").ToUint16();  //Todo: Update this based on xml data

            //Read properties
            var propertiesNode = element.GetRequiredElement("Properties");
            foreach (var property in propertiesNode.Elements("Property"))
            {
                Properties.Add(PropertyManager.ReadPropertyFromXml(property));
            }
            NumProps = (ushort)Properties.Count; //Update in case person editing xml forgot to
            //Todo: Update BlockSize & individual prop size when they change
            TrySetClassnameString();
            TrySetDescriptionString();
        }

        public XElement WriteToXml()
        {
            //Create object node
            HashGuesser.TryGuessHashString(ClassnameHash, out string className);
            var objectRoot = new XElement("ZoneObject", new XAttribute("Type", className));
            
            var objectData = new XElement("Data");
            objectRoot.Add(objectData);

            //Write 56 byte data block
            objectData.Add(new XAttribute("ClassnameHash", ClassnameHash));
            objectData.Add(new XElement("Handle", Handle));
            objectData.Add(Bmin.ToXElement("Bmin"));
            objectData.Add(Bmax.ToXElement("Bmax"));
            
            var flagsNode = new XElement("Flags", new XAttribute("Value", Flags));
            flagsNode.Add(new XElement("Flag0", (Flags & (ushort)ZoneObjectFlags.Flag0) != 0));
            flagsNode.Add(new XElement("Flag1", (Flags & (ushort)ZoneObjectFlags.Flag1) != 0));
            flagsNode.Add(new XElement("Flag2", (Flags & (ushort)ZoneObjectFlags.Flag2) != 0));
            flagsNode.Add(new XElement("Flag3", (Flags & (ushort)ZoneObjectFlags.Flag3) != 0));
            flagsNode.Add(new XElement("Flag4", (Flags & (ushort)ZoneObjectFlags.Flag4) != 0));
            flagsNode.Add(new XElement("Flag5", (Flags & (ushort)ZoneObjectFlags.Flag5) != 0));
            flagsNode.Add(new XElement("Flag6", (Flags & (ushort)ZoneObjectFlags.Flag6) != 0));
            flagsNode.Add(new XElement("Flag7", (Flags & (ushort)ZoneObjectFlags.Flag7) != 0));
            flagsNode.Add(new XElement("Flag8", (Flags & (ushort)ZoneObjectFlags.Flag8) != 0));
            flagsNode.Add(new XElement("Flag9", (Flags & (ushort)ZoneObjectFlags.Flag9) != 0));
            flagsNode.Add(new XElement("Flag10", (Flags & (ushort)ZoneObjectFlags.Flag10) != 0));
            flagsNode.Add(new XElement("Flag11", (Flags & (ushort)ZoneObjectFlags.Flag11) != 0));
            flagsNode.Add(new XElement("Flag12", (Flags & (ushort)ZoneObjectFlags.Flag12) != 0));
            flagsNode.Add(new XElement("Flag13", (Flags & (ushort)ZoneObjectFlags.Flag13) != 0));
            flagsNode.Add(new XElement("Flag14", (Flags & (ushort)ZoneObjectFlags.Flag14) != 0));
            flagsNode.Add(new XElement("Flag15", (Flags & (ushort)ZoneObjectFlags.Flag15) != 0));
            objectData.Add(flagsNode);

            objectData.Add(new XAttribute("BlockSize", BlockSize));
            objectData.Add(new XElement("Parent", Parent));
            objectData.Add(new XElement("Sibling", Sibling));
            objectData.Add(new XElement("Child", Child));
            objectData.Add(new XElement("Num", Num));
            objectData.Add(new XAttribute("NumProps", NumProps));
            objectData.Add(new XAttribute("Size", Size));

            var propertiesNode = new XElement("Properties");
            objectRoot.Add(propertiesNode);
            //Write properties
            foreach (var property in Properties)
            {
                propertiesNode.Add(property.WriteToXml());
            }
            //Return object node
            return objectRoot;
        }

        /// <summary>
        /// Sets the value of <see cref="Flags"/> from an xml node with true/false values for each bit of the flag.
        /// </summary>
        /// <param name="flagsNode">Xml node with separate true/false values for each bit of the flag.</param>
        private void SetFlagsFromXml(XElement flagsNode)
        {
            var valueAttribute = flagsNode.Attribute("Value");
            ushort? attributeValue = null;
            if (valueAttribute != null)
            {
                attributeValue = valueAttribute.Value.ToUint16();
            }

            bool flag0 = flagsNode.GetRequiredValue("Flag0").ToBool();
            bool flag7 = flagsNode.GetRequiredValue("Flag7").ToBool();
            bool flag8 = flagsNode.GetRequiredValue("Flag8").ToBool();
            bool flag13 = flagsNode.GetRequiredValue("Flag13").ToBool();


            if (flagsNode.GetRequiredValue("Flag0").ToBool())
                Flags |= (ushort)ZoneObjectFlags.Flag0;
            if (flagsNode.GetRequiredValue("Flag1").ToBool())
                Flags |= (ushort)ZoneObjectFlags.Flag1;
            if (flagsNode.GetRequiredValue("Flag2").ToBool())
                Flags |= (ushort)ZoneObjectFlags.Flag2;
            if (flagsNode.GetRequiredValue("Flag3").ToBool())
                Flags |= (ushort)ZoneObjectFlags.Flag3;
            if (flagsNode.GetRequiredValue("Flag4").ToBool())
                Flags |= (ushort)ZoneObjectFlags.Flag4;
            if (flagsNode.GetRequiredValue("Flag5").ToBool())
                Flags |= (ushort)ZoneObjectFlags.Flag5;
            if (flagsNode.GetRequiredValue("Flag6").ToBool())
                Flags |= (ushort)ZoneObjectFlags.Flag6;
            if (flagsNode.GetRequiredValue("Flag7").ToBool())
                Flags |= (ushort)ZoneObjectFlags.Flag7;
            if (flagsNode.GetRequiredValue("Flag8").ToBool())
                Flags |= (ushort)ZoneObjectFlags.Flag8;
            if (flagsNode.GetRequiredValue("Flag9").ToBool())
                Flags |= (ushort)ZoneObjectFlags.Flag9;
            if (flagsNode.GetRequiredValue("Flag10").ToBool())
                Flags |= (ushort)ZoneObjectFlags.Flag10;
            if (flagsNode.GetRequiredValue("Flag11").ToBool())
                Flags |= (ushort)ZoneObjectFlags.Flag11;
            if (flagsNode.GetRequiredValue("Flag12").ToBool())
                Flags |= (ushort)ZoneObjectFlags.Flag12;
            if (flagsNode.GetRequiredValue("Flag13").ToBool())
                Flags |= (ushort)ZoneObjectFlags.Flag13;
            if (flagsNode.GetRequiredValue("Flag14").ToBool())
                Flags |= (ushort)ZoneObjectFlags.Flag14;
            if (flagsNode.GetRequiredValue("Flag15").ToBool())
                Flags |= (ushort)ZoneObjectFlags.Flag15;

            //Make note of this. Doesn't really matter if someone changed the value since it uses the individual flags, but it's good to log this in case the math is bad
            if (Flags != attributeValue && attributeValue != null)
            {
                Console.WriteLine($"Warning. Individual flags and flags attribute don't match! Flags: {Flags}, attribute: {attributeValue}.");
            }
        }

        private void TrySetClassnameString()
        {
            if (HashGuesser.TryGuessHashString(ClassnameHash, out var maybeClassname))
                Classname = maybeClassname;
        }

        //Todo: Implement this func and enabled description in editor UI
        //Todo: Ideally move this into another class and match descriptions + classnames to a classname hash, rather than checking strings
        private void TrySetDescriptionString()
        {
            if(Classname == null)
                return;

            //switch (Classname)
            //{
            //    case "rfg_mover":
            //        break;
            //    case "shape_cutter":
            //        break;
            //    case "object_effect":
            //        break;
            //    case "district":
            //        break;
            //    case "multi_object_backpack":
            //        break;
            //    case "multi_object_flag":
            //        break;
            //    case "multi_object_marker":
            //        break;
            //    case "object_action_node":
            //        break;
            //    case "player":
            //        break;
            //    case "object_patrol":
            //        break;
            //    case "navpoint":
            //        break;
            //    case "cover_node":
            //        break;
            //    case "general_mover":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    case "":
            //        break;
            //    default:
            //        break;
            //}
        }

        public object Clone()
        {
            var zoneObject = new ZoneObject
            {
                ClassnameHash = this.ClassnameHash,
                Handle = this.Handle,
                Bmin = this.Bmin,
                Bmax = this.Bmax,
                Flags = this.Flags,
                BlockSize = this.BlockSize,
                Parent = this.Parent,
                Sibling = this.Sibling,
                Child = this.Child,
                Num = this.Num,

                NumProps = this.NumProps,
                Size = this.Size,

                Properties = this.Properties.ConvertAll(prop => prop),

                Classname = this.Classname,
                Description = this.Description,

            };
            return zoneObject;
        }
    }
}