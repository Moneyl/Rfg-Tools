using System;
using System.Xml.Linq;

namespace RfgTools.Types
{
    public class vector3f : ICloneable
    {
        public float x;
        public float y;
        public float z;

        public vector3f(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public vector3f(float initialValue)
        {
            x = initialValue;
            y = initialValue;
            z = initialValue;
        }

        public vector3f(vector3f vec)
        {
            x = vec.x;
            y = vec.y;
            z = vec.z;
        }

        public XElement ToXElement(string nodeName)
        {
            var node = new XElement(nodeName);
            node.Add(new XElement("x", x));
            node.Add(new XElement("y", y));
            node.Add(new XElement("z", z));
            return node;
        }

        public static vector3f operator+(vector3f a, vector3f b)
        {
            return new vector3f(a.x + b.x, a.y + b.y, a.z + b.z);
        }
        public static vector3f operator-(vector3f a, vector3f b)
        {
            return new vector3f(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public object Clone()
        {
            return new vector3f(x, y, z);
        }

        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }
    }
}
