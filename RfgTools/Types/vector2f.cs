using System.Xml.Linq;

namespace RfgTools.Types
{
    public class vector2f
    {
        public float x;
        public float y;

        public vector2f()
        {

        }

        public vector2f(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public vector2f(float initialValue)
        {
            x = initialValue;
            y = initialValue;
        }

        public XElement ToXElement(string nodeName)
        {
            var node = new XElement(nodeName);
            node.Add(new XElement("x", x));
            node.Add(new XElement("y", y));
            return node;
        }

        public static implicit operator XElement(vector2f vec)
        {
            return new XElement("Item", 
                new XElement("x", vec.x), 
                new XElement("y", vec.y));
        }
    }
}
