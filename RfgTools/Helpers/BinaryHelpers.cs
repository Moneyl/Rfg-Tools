using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RfgTools.Types;

namespace RfgTools.Helpers
{
    public static class BinaryHelpers
    {
        //BinaryReader extension functions
        public static string ReadNullTerminatedString(this BinaryReader stream)
        {
            var String = new StringBuilder();
            do
            {
                String.Append(stream.ReadChar()); //Since the character isn't a null byte, add it to the string
            }
            while (stream.PeekChar() != 0); //Read bytes until a null byte (string terminator) is reached

            stream.ReadByte(); //Read past the null terminator
            return String.ToString();
        }

        public static string ReadFixedLengthString(this BinaryReader stream, int length)
        {
            var String = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                String.Append(stream.ReadChar());
            }
            return String.ToString();
        }

        public static string ReadFixedLengthString(this BinaryReader stream, uint length)
        {
            return stream.ReadFixedLengthString((int)length);
        }

        public static void Align(this BinaryReader reader, long alignmentValue)
        {
            long position = reader.BaseStream.Position;
            int remainder = (int)(position % alignmentValue);
            int paddingSize = 0;
            if (remainder > 0)
            {
                paddingSize = (int)alignmentValue - remainder;
            }
            else
            {
                paddingSize = 0;
            }
            reader.BaseStream.Seek(paddingSize, SeekOrigin.Current);
        }

        public static ushort PeekUshort(this BinaryReader reader)
        {
            ushort val = reader.ReadUInt16();
            reader.BaseStream.Seek(-2, SeekOrigin.Current);
            return val;
        }

        public static BinaryReader Skip(this BinaryReader reader, long skipDistance)
        {
            reader.BaseStream.Seek(skipDistance, SeekOrigin.Current);
            return reader; //Return self so things like Align() can be chained with this.
        }

        public static void Skip(this Stream stream, long skipDistance)
        {
            stream.Seek(skipDistance, SeekOrigin.Current);
        }

        public static XElement ReadVector3fToXElement(this BinaryReader reader, string elementName)
        {
            var data = new XElement(elementName);
            data.Add(new XElement("x", reader.ReadSingle()));
            data.Add(new XElement("y", reader.ReadSingle()));
            data.Add(new XElement("z", reader.ReadSingle()));
            return data;
        }

        public static vector3f ReadVector3f(this BinaryReader reader)
        {
            return new vector3f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static vector2f ReadVector2f(this BinaryReader reader)
        {
            return new vector2f(reader.ReadSingle(), reader.ReadSingle());
        }

        public static matrix33 ReadMatrix33(this BinaryReader reader)
        {
            return new matrix33(reader.ReadVector3f(), reader.ReadVector3f(), reader.ReadVector3f());
        }

        public static T ReadType<T>(this BinaryReader reader)
        {
            Type type = typeof(T);
            object ret = null;

            if (type == typeof(short))
                ret = reader.ReadInt16();
            else if (type == typeof(ushort))
                ret = reader.ReadUInt16();
            else if (type == typeof(int))
                ret = reader.ReadInt32();
            else if (type == typeof(uint))
                ret = reader.ReadUInt32();
            else if (type == typeof(Int64))
                ret = reader.ReadInt64();
            else if (type == typeof(UInt64))
                ret = reader.ReadUInt64();
            else if (type == typeof(float))
                ret = reader.ReadSingle();
            else if (type == typeof(double))
                ret = reader.ReadDouble();
            else if (type == typeof(bool))
                ret = reader.ReadBoolean();
            else if (type == typeof(byte))
                ret = reader.ReadByte();
            else if (type == typeof(sbyte))
                ret = reader.ReadSByte();
            else if (type == typeof(char))
                ret = reader.ReadChar();
            else if (type == typeof(vector3f))
                ret = reader.ReadVector3f();
            else if (type == typeof(vector2f))
                ret = reader.ReadVector2f();
            else if (type == typeof(matrix33))
                ret = reader.ReadMatrix33();

            if (ret == null)
                throw new ArgumentException($"Unable to read type \"{type}\"");
            return (T)ret;
        }

        public static bool TryReadList<T>(this BinaryReader reader, int bytesToRead, out List<T> outList)
        { 
            outList = new List<T>();
            int typeSize = SizeHelper.GetTypeSize<T>();
            if (bytesToRead % typeSize == 0)
            {
                int numItems = bytesToRead / typeSize;
                for (int i = 0; i < numItems; i++)
                {
                    outList.Add(reader.ReadType<T>());
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void SeekAndSkip(this BinaryReader reader, long seekPos, long skipDistance)
        {
            reader.BaseStream.Seek(seekPos, SeekOrigin.Begin);
            reader.Skip(skipDistance);
        }



        //BinaryWriter extension functions
        public static BinaryWriter WriteAsciiString(this BinaryWriter writer, String stringOut, bool nullTerminate)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(stringOut);
            writer.Write(bytes, 0, bytes.Length);
            if (nullTerminate)
            {
                writer.Write((byte)0);
            }

            return writer;
        }

        public static void Align(this BinaryWriter writer, long alignmentValue)
        {
            long position = writer.BaseStream.Position;
            int remainder = (int)(position % alignmentValue);
            int paddingSize = 0;
            if (remainder > 0)
            {
                paddingSize = (int)alignmentValue - remainder;
            }
            else
            {
                paddingSize = 0;
            }

            writer.Write(Enumerable.Repeat((byte)0x0, paddingSize).ToArray(), 0, paddingSize);
        }

        public static void WriteNullBytes(this BinaryWriter writer, long numBytesToWrite)
        {
            var bytes = new byte[numBytesToWrite];
            writer.Write(bytes);
        }

        public static void Write(this BinaryWriter writer, vector2f vec2)
        {
            writer.Write(vec2.x);
            writer.Write(vec2.y);
        }

        public static void Write(this BinaryWriter writer, vector3f vec3)
        {
            writer.Write(vec3.x);
            writer.Write(vec3.y);
            writer.Write(vec3.z);
        }

        public static void Write(this BinaryWriter writer, matrix33 mat33)
        {
            writer.Write(mat33.rvec);
            writer.Write(mat33.uvec);
            writer.Write(mat33.fvec);
        }

        //Todo: See if this function can be written in a cleaner way
        public static void GenericWrite<T>(this BinaryWriter writer, T data)
        {
            Type type = typeof(T);

            if (type == typeof(short))
                writer.Write((short)(object)data);
            else if (type == typeof(ushort))
                writer.Write((ushort)(object)data);
            else if (type == typeof(int))
                writer.Write((int)(object)data);
            else if (type == typeof(uint))
                writer.Write((uint)(object)data);
            else if (type == typeof(Int64))
                writer.Write((Int64)(object)data);
            else if (type == typeof(UInt64))
                writer.Write((UInt64)(object)data);
            else if (type == typeof(float))
                writer.Write((float)(object)data);
            else if (type == typeof(double))
                writer.Write((double)(object)data);
            else if (type == typeof(bool))
                writer.Write((bool)(object)data);
            else if (type == typeof(byte))
                writer.Write((byte)(object)data);
            else if (type == typeof(sbyte))
                writer.Write((sbyte)(object)data);
            else if (type == typeof(char))
                writer.Write((char)(object)data);
            else if (type == typeof(vector3f))
                writer.Write((vector3f)(object)data);
            else if (type == typeof(vector2f))
                writer.Write((vector2f)(object)data);
            else if (type == typeof(matrix33))
                writer.Write((matrix33)(object)data);
            else
                throw new ArgumentException($"Unable to write type \"{type}\". Not yet supported by BinaryWriter.Write<T>() extension method.");
        }

        public static void WriteNullTerminatedString(this BinaryWriter stream, string output)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(output);
            stream.Write(bytes, 0, bytes.Length);
            stream.Write((byte)0);
        }

        public static uint GetAlignmentPad(long currentPos, long alignmentValue)
        {
            uint remainder = (uint)(currentPos % alignmentValue);
            uint paddingSize = 0;
            if (remainder > 0)
            {
                paddingSize = (uint)alignmentValue - remainder;
            }
            else
            {
                paddingSize = 0;
            }

            return paddingSize;
        }
    }
}
