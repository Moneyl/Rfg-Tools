using System.Xml.Linq;

namespace RfgTools.Formats.Scriptx.Nodes
{
    class ScriptFunction : ScriptNode
    {
        //Todo: Figure out way to handle multiple value types without using strings. Game uses a union for the value.
        public string Name;
        public XElement Inner;

        public ScriptFunction(string name, ScriptNodeType type)
        {
            Name = name;
            ClassType = ScriptNodeClasses.Atom;
            Type = type;
        }
    }
}
