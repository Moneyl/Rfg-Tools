using System;
using System.Xml.Linq;

namespace RfgTools.Types
{
    public class vector4f : ICloneable
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public vector4f()
        {

        }

        public vector4f(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public vector4f(float initialValue)
        {
            x = initialValue;
            y = initialValue;
            z = initialValue;
            w = initialValue;
        }

        public vector4f(vector4f vec)
        {
            x = vec.x;
            y = vec.y;
            z = vec.z;
            w = vec.w;
        }

        public XElement ToXElement(string nodeName)
        {
            var node = new XElement(nodeName);
            node.Add(new XElement("x", x));
            node.Add(new XElement("y", y));
            node.Add(new XElement("z", z));
            node.Add(new XElement("w", w));
            return node;
        }

        public static vector4f operator +(vector4f a, vector4f b)
        {
            return new vector4f(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        }
        public static vector4f operator -(vector4f a, vector4f b)
        {
            return new vector4f(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
        }

        public object Clone()
        {
            return new vector4f(x, y, z, w);
        }

        public override string ToString()
        {
            return $"({x}, {y}, {z}, {w})";
        }
    }
}
