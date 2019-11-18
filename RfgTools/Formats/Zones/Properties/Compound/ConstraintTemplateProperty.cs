using System;
using System.IO;
using System.Xml.Linq;
using RfgTools.Formats.Zones.Interfaces;
using RfgTools.Helpers;
using RfgTools.Types;

namespace RfgTools.Formats.Zones.Properties.Compound
{
    [Property("template", 5)]
    public class ConstraintTemplateProperty : IProperty
    {
        //Template data
        public ConstraintType ConstraintType { get; protected set; }
        public uint NameUnused { get; protected set; }
        //Body A data
        public uint BodyANameUnused { get; protected set; }
        public uint BodyAIndex { get; protected set; }
        public matrix33 BodyAOrient { get; protected set; }
        public vector3f BodyAPos { get; protected set; }
        //Body B data
        public uint BodyBNameUnused { get; protected set; }
        public uint BodyBIndex { get; protected set; }
        public matrix33 BodyBOrient { get; protected set; }
        public vector3f BodyBPos { get; protected set; }
        //Threshold
        public float Threshold { get; protected set; }
        //Additional constraint data. Contents seem to depend on constraint type, since property never has room for all variants
        public IConstraintData ConstraintData { get; protected set; }

        //Property data
        public ushort Type { get; protected set; }
        public ushort Size { get; protected set; }
        public uint NameHash { get; protected set; }

        public string GetFullName()
        {
            switch (ConstraintType)
            {
                case ConstraintType.ConstraintNone:
                    return "Constraint none";
                case ConstraintType.ConstraintPoint:
                    return "Point constraint";
                case ConstraintType.ConstraintHinge:
                    return "Hinge constraint";
                case ConstraintType.ConstraintPrismatic:
                    return "Prismatic constraint";
                case ConstraintType.ConstraintRagdoll:
                    return "Ragdoll constraint";
                case ConstraintType.ConstraintMotor:
                    return "Motor constraint";
                case ConstraintType.ConstraintFake:
                    return "Fake constraint";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string GetTypeName() => "Constraint template";

        public bool ReadFromStream(Stream stream, ushort type, ushort size, uint nameHash)
        {
            Type = type;
            Size = size;
            NameHash = nameHash;

            if (size == 156)
            {
                var reader = new BinaryReader(stream);
                //Read data present in all templates 
                ConstraintType = (ConstraintType)reader.ReadUInt32();
                NameUnused = reader.ReadUInt32();

                BodyANameUnused = reader.ReadUInt32();
                BodyAIndex = reader.ReadUInt32();
                BodyAOrient = reader.ReadMatrix33();
                BodyAPos = reader.ReadVector3f();

                BodyBNameUnused = reader.ReadUInt32();
                BodyBIndex = reader.ReadUInt32();
                BodyBOrient = reader.ReadMatrix33();
                BodyBPos = reader.ReadVector3f();

                Threshold = reader.ReadSingle();

                //Read constraint data, dependent on constraint type
                switch (ConstraintType)
                {
                    case ConstraintType.ConstraintNone:
                        ConstraintData = null;
                        break;
                    case ConstraintType.ConstraintPoint:
                        ConstraintData = new PointConstraintData();
                        ConstraintData.ReadFromStream(stream);
                        break;
                    case ConstraintType.ConstraintHinge:
                        ConstraintData = new HingeConstraintData();
                        ConstraintData.ReadFromStream(stream);
                        break;
                    case ConstraintType.ConstraintPrismatic:
                        ConstraintData = new PrismaticConstraintData();
                        ConstraintData.ReadFromStream(stream);
                        break;
                    case ConstraintType.ConstraintRagdoll:
                        ConstraintData = new RagdollConstraintData();
                        ConstraintData.ReadFromStream(stream);
                        break;
                    case ConstraintType.ConstraintMotor:
                        ConstraintData = new MotorConstraintData();
                        ConstraintData.ReadFromStream(stream);
                        break;
                    case ConstraintType.ConstraintFake:
                        ConstraintData = new FakeConstraintData();
                        ConstraintData.ReadFromStream(stream);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return true;
            }
            else
            {
                Console.WriteLine("Error! Found constraint template property with size != 156 bytes. Unknown data! Skipping property.");
                return false;
            }
        }

        public void WriteToStream(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(Type);
            writer.Write(Size);
            writer.Write(NameHash);

            writer.Write((uint)ConstraintType);
            writer.Write(NameUnused);

            writer.Write(BodyANameUnused);
            writer.Write(BodyAIndex);
            writer.Write(BodyAOrient);
            writer.Write(BodyAPos);

            writer.Write(BodyBNameUnused);
            writer.Write(BodyBIndex);
            writer.Write(BodyBOrient);
            writer.Write(BodyBPos);

            writer.Write(Threshold);

            ConstraintData.WriteToStream(stream);
        }

        public void ReadFromXml(XElement propertyRoot, ushort type, ushort size, uint nameHash)
        {
            Type = type;
            Size = size;
            NameHash = nameHash;

            var dataNode = propertyRoot.GetRequiredElement("Data");
            ConstraintType = (ConstraintType)dataNode.GetRequiredValue("ConstraintType").ToInt32();
            NameUnused = dataNode.GetRequiredValue("NameUnused").ToUint32();

            BodyANameUnused = dataNode.GetRequiredValue("BodyANameUnused").ToUint32();
            BodyAIndex = dataNode.GetRequiredValue("BodyAIndex").ToUint32();
            BodyAOrient = dataNode.GetRequiredElement("BodyAOrient").ToMatrix33();
            BodyAPos = dataNode.GetRequiredElement("BodyAPos").ToVector3f();

            BodyBNameUnused = dataNode.GetRequiredValue("BodyBNameUnused").ToUint32();
            BodyBIndex = dataNode.GetRequiredValue("BodyBIndex").ToUint32();
            BodyBOrient = dataNode.GetRequiredElement("BodyBOrient").ToMatrix33();
            BodyBPos = dataNode.GetRequiredElement("BodyBPos").ToVector3f();

            Threshold = dataNode.GetRequiredValue("Threshold").ToSingle();

            //Read constraint data, dependent on constraint type
            var constraintDataNode = dataNode.GetRequiredElement("ConstraintData");
            switch (ConstraintType)
            {
                case ConstraintType.ConstraintNone:
                    ConstraintData = null;
                    break;
                case ConstraintType.ConstraintPoint:
                    ConstraintData = new PointConstraintData();
                    ConstraintData.ReadFromXml(constraintDataNode);
                    break;
                case ConstraintType.ConstraintHinge:
                    ConstraintData = new HingeConstraintData();
                    ConstraintData.ReadFromXml(constraintDataNode);
                    break;
                case ConstraintType.ConstraintPrismatic:
                    ConstraintData = new PrismaticConstraintData();
                    ConstraintData.ReadFromXml(constraintDataNode);
                    break;
                case ConstraintType.ConstraintRagdoll:
                    ConstraintData = new RagdollConstraintData();
                    ConstraintData.ReadFromXml(constraintDataNode);
                    break;
                case ConstraintType.ConstraintMotor:
                    ConstraintData = new MotorConstraintData();
                    ConstraintData.ReadFromXml(constraintDataNode);
                    break;
                case ConstraintType.ConstraintFake:
                    ConstraintData = new FakeConstraintData();
                    ConstraintData.ReadFromXml(constraintDataNode);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public XElement WriteToXml()
        {
            var propertyRoot = new XElement("Property");
            PropertyManager.WritePropertyInfoToXml(propertyRoot, Type, Size, NameHash, "template", "constraint template");

            var data = new XElement("Data");
            data.Add(new XElement("ConstraintType", (uint)ConstraintType));
            data.Add(new XElement("NameUnused", NameUnused));

            data.Add(new XElement("BodyANameUnused", BodyANameUnused));
            data.Add(new XElement("BodyAIndex", BodyAIndex));
            data.Add(BodyAOrient.ToXElement("BodyAOrient"));
            data.Add(BodyAPos.ToXElement("BodyAPos"));

            data.Add(new XElement("BodyBNameUnused", BodyBNameUnused));
            data.Add(new XElement("BodyBIndex", BodyBIndex));
            data.Add(BodyBOrient.ToXElement("BodyBOrient"));
            data.Add(BodyBPos.ToXElement("BodyBPos"));

            data.Add(new XElement("Threshold", Threshold));
            if (ConstraintData != null)
            {
                data.Add(ConstraintData.WriteToXml());
            }
            else
            {
                data.Add(new XElement("ConstraintData", "null"));
            }
            propertyRoot.Add(data);

            return propertyRoot;
        }
    }
    
    public enum ConstraintType : uint
    {
        ConstraintNone = 0xFFFFFFFF,
        ConstraintPoint = 0,
        ConstraintHinge = 1,
        ConstraintPrismatic = 2,
        ConstraintRagdoll = 3,
        ConstraintMotor = 4,
        ConstraintFake = 5,
    };

    /// <summary>
    /// Interface for additional constraint data in constraint templates. Actual data depends on constraint type
    /// </summary>
    public interface IConstraintData
    {
        void ReadFromStream(Stream stream);
        void WriteToStream(Stream stream);
        void ReadFromXml(XElement data);
        XElement WriteToXml();
    }

    /// <summary>
    /// Constraint data used when constraint type = 0 (ConstraintPoint)
    /// </summary>
    public class PointConstraintData : IConstraintData
    {
        public int Ctype { get; protected set; }
        public float XLimitMin { get; protected set; }
        public float XLimitMax { get; protected set; }
        public float YLimitMin { get; protected set; }
        public float YLimitMax { get; protected set; }
        public float ZLimitMin { get; protected set; }
        public float ZLimitMax { get; protected set; }
        public float StiffSpringLength { get; protected set; }

        public void ReadFromStream(Stream stream)
        {
            var reader = new BinaryReader(stream);
            Ctype = reader.ReadInt32();
            XLimitMin = reader.ReadSingle();
            XLimitMax = reader.ReadSingle();
            YLimitMin = reader.ReadSingle();
            YLimitMax = reader.ReadSingle();
            ZLimitMin = reader.ReadSingle();
            ZLimitMax = reader.ReadSingle();
            StiffSpringLength = reader.ReadSingle();
        }

        public void WriteToStream(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(Ctype);
            writer.Write(XLimitMin);
            writer.Write(XLimitMax);
            writer.Write(YLimitMin);
            writer.Write(YLimitMax);
            writer.Write(ZLimitMin);
            writer.Write(ZLimitMax);
            writer.Write(StiffSpringLength);
        }

        public void ReadFromXml(XElement data)
        {
            Ctype = data.GetRequiredValue("Ctype").ToInt32();
            XLimitMin = data.GetRequiredValue("XLimitMin").ToSingle();
            XLimitMax = data.GetRequiredValue("XLimitMax").ToSingle();
            YLimitMin = data.GetRequiredValue("YLimitMin").ToSingle();
            YLimitMax = data.GetRequiredValue("YLimitMax").ToSingle();
            ZLimitMin = data.GetRequiredValue("ZLimitMin").ToSingle();
            ZLimitMax = data.GetRequiredValue("ZLimitMax").ToSingle();
            StiffSpringLength = data.GetRequiredValue("StiffSpringLength").ToSingle();
        }

        public XElement WriteToXml()
        {
            var constraintData = new XElement("ConstraintData");
            constraintData.Add(new XAttribute("Type", "point constraint"));

            constraintData.Add(new XElement("Ctype", Ctype));
            constraintData.Add(new XElement("XLimitMin", XLimitMin));
            constraintData.Add(new XElement("XLimitMax", XLimitMax));
            constraintData.Add(new XElement("YLimitMin", YLimitMin));
            constraintData.Add(new XElement("YLimitMax", YLimitMax));
            constraintData.Add(new XElement("ZLimitMin", ZLimitMin));
            constraintData.Add(new XElement("ZLimitMax", ZLimitMax));
            constraintData.Add(new XElement("StiffSpringLength", StiffSpringLength));

            return constraintData;
        }
    }

    /// <summary>
    /// Constraint data used when constraint type = 1 (ConstraintHinge)
    /// </summary>
    public class HingeConstraintData : IConstraintData
    {
        public int Limited { get; protected set; }
        public float LimitMinAngle { get; protected set; }
        public float LimitMaxAngle { get; protected set; }
        public float LimitFriction { get; protected set; }

        public void ReadFromStream(Stream stream)
        {
            var reader = new BinaryReader(stream);
            Limited = reader.ReadInt32();
            LimitMinAngle = reader.ReadSingle();
            LimitMaxAngle = reader.ReadSingle();
            LimitFriction = reader.ReadSingle();
            reader.Skip(16); //Skip remainder of constraint data (unused by this constraint type)
        }

        public void WriteToStream(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(Limited);
            writer.Write(LimitMinAngle);
            writer.Write(LimitMaxAngle);
            writer.Write(LimitFriction);
            writer.WriteNullBytes(16);
        }

        public void ReadFromXml(XElement data)
        {
            Limited = data.GetRequiredValue("Limited").ToInt32();
            LimitMinAngle = data.GetRequiredValue("LimitMinAngle").ToSingle();
            LimitMaxAngle = data.GetRequiredValue("LimitMaxAngle").ToSingle();
            LimitFriction = data.GetRequiredValue("LimitFriction").ToSingle();
        }

        public XElement WriteToXml()
        {
            var constraintData = new XElement("ConstraintData");
            constraintData.Add(new XAttribute("Type", "hinge constraint"));

            constraintData.Add(new XElement("Limited", Limited));
            constraintData.Add(new XElement("LimitMinAngle", LimitMinAngle));
            constraintData.Add(new XElement("LimitMaxAngle", LimitMaxAngle));
            constraintData.Add(new XElement("LimitFriction", LimitFriction));

            return constraintData;
        }
    }

    /// <summary>
    /// Constraint data used when constraint type = 2 (ConstraintPrismatic)
    /// </summary>
    public class PrismaticConstraintData : IConstraintData
    {
        public int Limited { get; protected set; }
        public float LimitMin { get; protected set; }
        public float LimitMax { get; protected set; }
        public float LimitFriction { get; protected set; }

        public void ReadFromStream(Stream stream)
        {
            var reader = new BinaryReader(stream);
            Limited = reader.ReadInt32();
            LimitMin = reader.ReadSingle();
            LimitMax = reader.ReadSingle();
            LimitFriction = reader.ReadSingle();
            reader.Skip(16); //Skip remainder of constraint data (unused by this constraint type)
        }

        public void WriteToStream(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(Limited);
            writer.Write(LimitMin);
            writer.Write(LimitMax);
            writer.Write(LimitFriction);
            writer.WriteNullBytes(16);
        }

        public void ReadFromXml(XElement data)
        {
            Limited = data.GetRequiredValue("Limited").ToInt32();
            LimitMin = data.GetRequiredValue("LimitMin").ToSingle();
            LimitMax = data.GetRequiredValue("LimitMax").ToSingle();
            LimitFriction = data.GetRequiredValue("LimitFriction").ToSingle();
        }

        public XElement WriteToXml()
        {
            var constraintData = new XElement("ConstraintData");
            constraintData.Add(new XAttribute("Type", "prismatic constraint"));

            constraintData.Add(new XElement("Limited", Limited));
            constraintData.Add(new XElement("LimitMin", LimitMin));
            constraintData.Add(new XElement("LimitMax", LimitMax));
            constraintData.Add(new XElement("LimitFriction", LimitFriction));

            return constraintData;
        }
    }

    /// <summary>
    /// Constraint data used when constraint type = 3 (ConstraintRagdoll)
    /// </summary>
    public class RagdollConstraintData : IConstraintData
    {
        public float TwistMin { get; protected set; }
        public float TwistMax { get; protected set; }
        public float ConeMin { get; protected set; }
        public float ConeMax { get; protected set; }
        public float PlaneMin { get; protected set; }
        public float PlaneMax { get; protected set; }

        public void ReadFromStream(Stream stream)
        {
            var reader = new BinaryReader(stream);
            TwistMin = reader.ReadSingle();
            TwistMax = reader.ReadSingle();
            ConeMin = reader.ReadSingle();
            ConeMax = reader.ReadSingle();
            PlaneMin = reader.ReadSingle();
            PlaneMax = reader.ReadSingle();
            reader.Skip(8); //Skip remainder of constraint data (unused by this constraint type)
        }

        public void WriteToStream(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(TwistMin);
            writer.Write(TwistMax);
            writer.Write(ConeMin);
            writer.Write(ConeMax);
            writer.Write(PlaneMin);
            writer.Write(PlaneMax);
            writer.WriteNullBytes(8);
        }

        public void ReadFromXml(XElement data)
        {
            TwistMin = data.GetRequiredValue("TwistMin").ToSingle();
            TwistMax = data.GetRequiredValue("TwistMax").ToSingle();
            ConeMin = data.GetRequiredValue("ConeMin").ToSingle();
            ConeMax = data.GetRequiredValue("ConeMax").ToSingle();
            PlaneMin = data.GetRequiredValue("PlaneMin").ToSingle();
            PlaneMax = data.GetRequiredValue("PlaneMax").ToSingle();
        }

        public XElement WriteToXml()
        {
            var constraintData = new XElement("ConstraintData");
            constraintData.Add(new XAttribute("Type", "ragdoll constraint"));

            constraintData.Add(new XElement("TwistMin", TwistMin));
            constraintData.Add(new XElement("TwistMax", TwistMax));
            constraintData.Add(new XElement("ConeMin", ConeMin));
            constraintData.Add(new XElement("ConeMax", ConeMax));
            constraintData.Add(new XElement("PlaneMin", PlaneMin));
            constraintData.Add(new XElement("PlaneMax", PlaneMax));

            return constraintData;
        }
    }

    /// <summary>
    /// Constraint data used when constraint type = 4 (ConstraintMotor)
    /// </summary>
    public class MotorConstraintData : IConstraintData
    {
        public float AngularSpeed { get; protected set; }
        public float Gain { get; protected set; }
        public int Axis { get; protected set; }
        public float AxisInBodySpaceX { get; protected set; }
        public float AxisInBodySpaceY { get; protected set; }
        public float AxisInBodySpaceZ { get; protected set; }

        public void ReadFromStream(Stream stream)
        {
            var reader = new BinaryReader(stream);
            AngularSpeed = reader.ReadSingle();
            Gain = reader.ReadSingle();
            Axis = reader.ReadInt32();
            AxisInBodySpaceX = reader.ReadSingle();
            AxisInBodySpaceY = reader.ReadSingle();
            AxisInBodySpaceZ = reader.ReadSingle();
            reader.Skip(8); //Skip remainder of constraint data (unused by this constraint type)
        }

        public void WriteToStream(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(AngularSpeed);
            writer.Write(Gain);
            writer.Write(Axis);
            writer.Write(AxisInBodySpaceX);
            writer.Write(AxisInBodySpaceY);
            writer.Write(AxisInBodySpaceZ);
            writer.WriteNullBytes(8);
        }

        public void ReadFromXml(XElement data)
        {
            AngularSpeed = data.GetRequiredValue("AngularSpeed").ToSingle();
            Gain = data.GetRequiredValue("Gain").ToSingle();
            Axis = data.GetRequiredValue("Axis").ToInt32();
            AxisInBodySpaceX = data.GetRequiredValue("AxisInBodySpaceX").ToSingle();
            AxisInBodySpaceY = data.GetRequiredValue("AxisInBodySpaceY").ToSingle();
            AxisInBodySpaceZ = data.GetRequiredValue("AxisInBodySpaceZ").ToSingle();
        }

        public XElement WriteToXml()
        {
            var constraintData = new XElement("ConstraintData");
            constraintData.Add(new XAttribute("Type", "motor constraint"));

            constraintData.Add(new XElement("AngularSpeed", AngularSpeed));
            constraintData.Add(new XElement("Gain", Gain));
            constraintData.Add(new XElement("Axis", Axis));
            constraintData.Add(new XElement("AxisInBodySpaceX", AxisInBodySpaceX));
            constraintData.Add(new XElement("AxisInBodySpaceY", AxisInBodySpaceY));
            constraintData.Add(new XElement("AxisInBodySpaceZ", AxisInBodySpaceZ));

            return constraintData;
        }
    }

    /// <summary>
    /// Constraint data used when constraint type = 5 (ConstraintFake)
    /// </summary>
    public class FakeConstraintData : IConstraintData
    {
        public void ReadFromStream(Stream stream)
        {
            var reader = new BinaryReader(stream);
            reader.Skip(32); //Afaik, this type of constraint has no data.
        }

        public void WriteToStream(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.WriteNullBytes(32);
        }

        public void ReadFromXml(XElement data)
        {
            //No data here...
        }

        public XElement WriteToXml()
        {
            var constraintData = new XElement("ConstraintData");
            constraintData.Add(new XAttribute("Type", "fake constraint"));

            return constraintData;
        }
    }
}
