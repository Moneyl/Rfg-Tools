using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace RfgTools.Types
{
    public class matrix43
    {
        //The names of these vectors are based on the common purpose of a 4x3 matrix in RFG
        public vector3f rvec;
        public vector3f uvec;
        public vector3f fvec;
        public vector3f translation;

        public matrix43(vector3f rvec, vector3f uvec, vector3f fvec, vector3f translation)
        {
            this.rvec = rvec;
            this.uvec = uvec;
            this.fvec = fvec;
            this.translation = translation;
        }

        public matrix43(float initialValue)
        {
            rvec = new vector3f(initialValue);
            uvec = new vector3f(initialValue);
            fvec = new vector3f(initialValue);
            translation = new vector3f(initialValue);
        }

        public XElement ToXElement(string nodeName)
        {
            var node = new XElement(nodeName);
            node.Add(rvec.ToXElement("rvec"));
            node.Add(uvec.ToXElement("uvec"));
            node.Add(fvec.ToXElement("fvec"));
            node.Add(translation.ToXElement("translation"));
            return node;
        }
    }
}