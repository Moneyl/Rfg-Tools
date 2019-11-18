using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using RfgTools.Types;

namespace RfgTools.Helpers
{
    public static class XmlHelpers
    {
        /// <summary>
        /// Get the first child element on element with the provided name. For optional nodes,
        /// or nodes that can be recovered from if missing
        /// </summary>
        /// <param name="element">The element being searched through</param>
        /// <param name="targetName">The target name of the child element to be returned. Returns the first element with this name.</param>
        /// <param name="result">Contains the target xml element if it's found</param>
        /// <returns></returns>
        public static bool TryGetFirst(this XElement element, string targetName, out XElement result)
        {
            result = element.Elements().SingleOrDefault(p => p.Name.LocalName == targetName);
            return result != null;
        }

        //Same behavior as TryGetFirst, but it returns the value of the XElement if found
        public static bool TryGetFirstValue(this XElement element, string targetName, out string result)
        {
            result = "";
            if (!element.TryGetFirst(targetName, out var target))
                return false;

            result = target.Value;
            return true;
        }

        /// <summary>
        /// Attempt to get a required value and thrown an exception if that fails. Use for values that are required for further execution
        /// </summary>
        /// <param name="element"></param>
        /// <param name="targetName"></param>
        /// <param name="additionalFailureMessage"></param>
        /// <returns></returns>
        public static XElement GetRequiredElement(this XElement element, string targetName, string additionalFailureMessage = "")
        {
            var result = element.Elements().SingleOrDefault(p => p.Name.LocalName == targetName);
            if (result == null)
            {
                throw new XmlException($"Error! Failed to get required xml node \"{targetName}\"! " + additionalFailureMessage);
            }
            return result;
        }

        //Same behavior as GetRequiredElement, but it returns the value of the XElement if found
        public static string GetRequiredValue(this XElement element, string targetName, string additionalFailureMessage = "")
        {
            var result = element.Elements().SingleOrDefault(p => p.Name.LocalName == targetName);
            if (result == null || result.Value == null)
            {
                throw new XmlException($"Error! Failed to get required value from xml node \"{targetName}\"! " + additionalFailureMessage);
            }
            return result.Value;
        }

        //Attempt to read an optional value, if that fails, return the provided defaultValue
        public static string GetOptionalValue(this XElement element, string targetName, string defaultValue)
        {
            var result = element.Elements().SingleOrDefault(p => p.Name.LocalName == targetName);
            if (result == null || result.Value == null)
            {
                return defaultValue;
            }
            return result.Value;
        }

        //Same as GetRequiredElement, but for attributes on an element
        public static XAttribute GetRequiredAttribute(this XElement element, string targetName, string additionalFailureMessage = "")
        {
            var result = element.Attributes().SingleOrDefault(p => p.Name.LocalName == targetName);
            if (result == null)
            {
                throw new XmlException($"Error! Failed to get required xml attribute \"{targetName}\" from the element \"{element.Name}\"! " + additionalFailureMessage);
            }
            return result;
        }

        //Same behavior as GetRequiredValue, but for attributes
        public static string GetRequiredAttributeValue(this XElement element, string targetName, string additionalFailureMessage = "")
        {
            var result = element.Attributes().SingleOrDefault(p => p.Name.LocalName == targetName);
            if (result == null || result.Value == null)
            {
                throw new XmlException($"Error! Failed to get required attribute value from attribute " +
                                       $"\"{targetName}\" from the element \"{element.Name}\"! " + additionalFailureMessage);
            }
            return result.Value;
        }

        //Attempt to read an optional value from an attribute, if that fails, return the provided defaultValue
        public static string GetOptionalAttributeValue(this XElement element, string targetName, string defaultValue)
        {
            var result = element.Attributes().SingleOrDefault(p => p.Name.LocalName == targetName);
            if (result == null || result.Value == null)
            {
                return defaultValue;
            }
            return result.Value;
        }



        public static vector2f ToVector2f(this XElement element)
        {
            if (!element.TryGetFirst("x", out var xNode))
                throw new XmlException("Attempted to read an element as a vector2f, but it has no x value.");
            if (!element.TryGetFirst("y", out var yNode))
                throw new XmlException("Attempted to read an element as a vector2f, but it has no y value.");

            return new vector2f(xNode.Value.ToSingle(), yNode.Value.ToSingle());
        }

        public static vector3f ToVector3f(this XElement element)
        {
            if (!element.TryGetFirst("x", out var xNode))
                throw new XmlException("Attempted to read an element as a vector3f, but it has no x value.");
            if (!element.TryGetFirst("y", out var yNode))
                throw new XmlException("Attempted to read an element as a vector3f, but it has no y value.");
            if (!element.TryGetFirst("z", out var zNode))
                throw new XmlException("Attempted to read an element as a vector3f, but it has no z value.");

            return new vector3f(xNode.Value.ToSingle(), yNode.Value.ToSingle(),  zNode.Value.ToSingle());
        }

        public static matrix33 ToMatrix33(this XElement element)
        {
            if(!element.TryGetFirst("rvec", out var rvecNode))
                throw new XmlException("Tried to read an element as a matrix33, but is has no rvec value!");
            if (!element.TryGetFirst("uvec", out var uvecNode))
                throw new XmlException("Tried to read an element as a matrix33, but is has no uvec value!");
            if (!element.TryGetFirst("fvec", out var fvecNode))
                throw new XmlException("Tried to read an element as a matrix33, but is has no fvec value!");
            
            return new matrix33(rvecNode.ToVector3f(), uvecNode.ToVector3f(), fvecNode.ToVector3f());
        }

        /// <summary>
        /// Cast an element to the specified type. Very little error checking, so if it doesn't support the type or gets bad
        /// data it'll likely just crash.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        /// <returns></returns>
        public static T Cast<T>(this XElement element)
        {
            Type type = typeof(T);
            object ret = null;

            if (type == typeof(short))
                ret = element.Value.ToInt16();
            else if (type == typeof(ushort))
                ret = element.Value.ToUint16();
            else if (type == typeof(int))
                ret = element.Value.ToInt32();
            else if (type == typeof(uint))
                ret = element.Value.ToUint32();
            else if (type == typeof(float))
                ret = element.Value.ToSingle();
            else if (type == typeof(double))
                ret = element.Value.ToDouble();
            else if (type == typeof(bool))
                ret = element.Value.ToBool();
            else if (type == typeof(byte))
                ret = element.Value.ToByte();
            else if (type == typeof(char))
                ret = element.Value.ToChar();
            else if (type == typeof(vector3f))
                ret = element.ToVector3f();
            else if (type == typeof(vector2f))
                ret = element.ToVector2f();
            else if (type == typeof(matrix33))
                ret = element.ToMatrix33();

            if (ret == null)
                throw new ArgumentException($"Unable to cast \"{element.Value}\" xml data to \"{type}\" data.");
            return (T)ret;
        }
    }
}
