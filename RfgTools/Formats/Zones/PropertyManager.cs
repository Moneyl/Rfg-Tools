using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using RfgTools.Formats.Zones.Interfaces;
using RfgTools.Formats.Zones.Properties.Compound;
using RfgTools.Formats.Zones.Properties.Special;
using RfgTools.Helpers;

namespace RfgTools.Formats.Zones
{
    public static class PropertyManager
    {
        /// <summary>
        /// List of tuples with info for all property definitions available at runtime.
        /// </summary>
        public static List<(PropertyAttribute, Type)> PropertyDefinitions;

        static PropertyManager()
        {
            PropertyDefinitions = GetPropertyDefinitions(); //Only need to get once, shouldn't change while the program is running
        }

        private static List<(PropertyAttribute, Type)> GetPropertyDefinitions()
        {
            var definitions = new List<(PropertyAttribute, Type)>();
            Assembly assembly = Assembly.GetExecutingAssembly();
            
            foreach (Type type in assembly.GetTypes())
            {
                var attribute = (PropertyAttribute)Attribute.GetCustomAttribute(type, typeof(PropertyAttribute));
                if (attribute != null && typeof(IProperty).IsAssignableFrom(type)) //Skip types that don't have the property attribute and implement IProperty
                {
                    definitions.Add((attribute, type));
                }
            }
            return definitions;
        }

        /// <summary>
        /// Attempt to read a property from a binary file (rfgzone_pc or layer_pc). If a definition for the property cannot be found
        /// then return a default <see cref="UnknownDataProperty"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static IProperty ReadPropertyFromBinary(Stream stream)
        {
            var reader = new BinaryReader(stream);

            ushort type = reader.ReadUInt16();
            ushort size = reader.ReadUInt16();
            uint propertyNameHash = reader.ReadUInt32();
            long propertyStartPos = reader.BaseStream.Position;

            //Check all property definitions for a match
            foreach (var (attribute, definition) in PropertyDefinitions)
            {
                if (attribute.NameHash == propertyNameHash)
                {
                    if (attribute.Type == type)
                    {
                        //Try to get property instance from definition and read it's data. If that fails then use UnknownDataProperty
                        if (!TryCreatePropertyInstance(definition, out var property) || !property.ReadFromStream(stream, type, size, propertyNameHash))
                        {
                            reader.BaseStream.Seek(propertyStartPos, SeekOrigin.Begin);
                            return HandleUnknownStreamData(stream, type, size, propertyNameHash, attribute.Name, $"unknown_{attribute.Name}_type");
                        }

                        reader.Align(4);
                        return property;
                    }

                    //Type mismatch, print warning and use UnknownDataProperty
                    Console.WriteLine($"Error! The \"{propertyNameHash}\" property matches the name hash for it's definition, but does not match it's type." +
                                      $"The definition expects a type of {attribute.Type}, while the property has a type of {type}. Returning default property.");
                }
            }

            //No property definition match. Use UnknownDataProperty.
            return HandleUnknownStreamData(stream, type, size, propertyNameHash, "unknown", "unknown");
        }

        /// <summary>
        /// Reads a property type, size, and name hash from xml, and attempts to find a property definition that can
        /// handle that property data.
        /// </summary>
        /// <param name="propertyXml">XElement containing the property data.</param>
        /// <returns>If a matching property is found, returns that property after it reads the data. Otherwise returns a <see cref="UnknownDataProperty"/></returns>
        public static IProperty ReadPropertyFromXml(XElement propertyXml)
        {
            ushort type = propertyXml.GetRequiredAttributeValue("Type").ToUint16();
            ushort size = propertyXml.GetRequiredAttributeValue("Size").ToUint16();
            uint propertyNameHash = propertyXml.GetRequiredAttributeValue("NameHash").ToUint32();

            //Try to find a matching property definition and use it's implementation of IProperty
            foreach (var (attribute, definition) in PropertyDefinitions)
            {
                if (attribute.NameHash == propertyNameHash)
                {
                    if (attribute.Type == type)
                    {
                        //If name hash and type match a definition, attempt to create a property instance from that definition and read it's data from xml.
                        if (!TryCreatePropertyInstance(definition, out var property))
                        {
                            throw new Exception($"Failed to create property instance from definition! Definition name: {definition.FullName}, " +
                                                $"Type: {type}, Size: {size}, Name hash: {propertyNameHash}");
                        }

                        property.ReadFromXml(propertyXml, type, size, propertyNameHash);
                        return property;
                    }

                    //Log any hash-type mismatches
                    Console.WriteLine($"Warning! The \"{propertyNameHash}\" property matches the name hash for it's definition, but does not match it's type." +
                                      $"The definition expects a type of {attribute.Type}, while the property has a type of {type}. Using default property.");
                }
            }

            //If no matching definition found, then use default property
            IProperty unknownDataProperty = new UnknownDataProperty();
            unknownDataProperty.ReadFromXml(propertyXml, type, size, propertyNameHash);
            return unknownDataProperty;
        }

        /// <summary>
        /// Checks to see if a string is known for the provided hash. If it is, write that to the console.
        /// Used to notify the user when an unregistered property is found. Necessary since there are still unknown properties.
        /// </summary>
        /// <param name="hash"></param>
        private static void CheckForUnregisteredHash(uint hash, uint type, uint size)
        {
            if (HashGuesser.TryGuessHashString(hash, out string name))
            {
                Console.WriteLine($"Unregistered property found! Name: {name}, Hash: {hash}, Type: {type}, Size: {size}");
            }
        }

        /// <summary>
        /// Attempts to create a concrete type implementing <see cref="IProperty"/>. Handles different behavior for
        /// properties that simply inherit <see cref="IProperty"/> and also for ones that are generic like <see cref="ListProperty{T}"/>
        /// </summary>
        /// <param name="definition">The type of property to create</param>
        /// <param name="property">Contains the created property if successful</param>
        /// <returns>Returns false if it fails to create the property instance</returns>
        private static bool TryCreatePropertyInstance(Type definition, out IProperty property)
        {
            //Try to get property instance from definition
            if (definition.ContainsGenericParameters) //Attempt to handle generic properties
            {
                if (definition.BaseType != null) //Properties like ObjLinksProperty which inherit ListProperty<T>
                {
                    var genericArguments = definition.BaseType.GetGenericArguments();
                    var constructedType = definition.MakeGenericType(genericArguments);
                    property = Activator.CreateInstance(constructedType) as IProperty;
                }
                else //Case where a property itself is generic. Currently there are none that do this.
                {
                    var genericArguments = definition.GetGenericArguments();
                    var constructedType = definition.MakeGenericType(genericArguments);
                    property = Activator.CreateInstance(constructedType) as IProperty;
                }
            }
            else
            {
                property = Activator.CreateInstance(definition) as IProperty;
            }

            return property != null;
        }

        /// <summary>
        /// Creates a new <see cref="UnknownDataProperty"/>, reads the unknown data, and returns the property.
        /// Only for when reading binary zone data from a stream
        /// </summary>
        private static IProperty HandleUnknownStreamData(Stream stream, ushort type, ushort size, uint propertyNameHash, string customName, string customType)
        {
            var reader = new BinaryReader(stream);

            IProperty unknownDataProperty = new UnknownDataProperty();
            unknownDataProperty.ReadFromStream(stream, type, size, propertyNameHash);
            reader.Align(4);
            CheckForUnregisteredHash(propertyNameHash, type, size);
            return unknownDataProperty;
        }

        /// <summary>
        /// Writes type, size, and nameHash value for a property to propertyRoot.
        /// Prevents duplicating this code for every single property. Makes editing xml format easier.
        /// </summary>
        public static void WritePropertyInfoToXml(XElement propertyRoot, uint type, uint size, uint nameHash, string name, string typeString)
        {
            propertyRoot.Add(new XAttribute("Name", name));
            propertyRoot.Add(new XAttribute("TypeName", typeString));
            propertyRoot.Add(new XAttribute("Type", type));
            propertyRoot.Add(new XAttribute("Size", size));
            propertyRoot.Add(new XAttribute("NameHash", nameHash));
        }
    }
}
