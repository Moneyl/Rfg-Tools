using System.Xml.Linq;

namespace RfgTools.Types
{
    public class matrix33
    {
        public vector3f rvec;
        public vector3f uvec;
        public vector3f fvec;

        public matrix33(vector3f rvec, vector3f uvec, vector3f fvec)
        {
            this.rvec = rvec;
            this.uvec = uvec;
            this.fvec = fvec;
        }

        public matrix33(float initialValue)
        {
            rvec = new vector3f(initialValue);
            uvec = new vector3f(initialValue);
            fvec = new vector3f(initialValue);
        }

        public XElement ToXElement(string nodeName)
        {
            var node = new XElement(nodeName);
            node.Add(rvec.ToXElement("rvec"));
            node.Add(uvec.ToXElement("uvec"));
            node.Add(fvec.ToXElement("fvec"));
            return node;
        }
    }
}
