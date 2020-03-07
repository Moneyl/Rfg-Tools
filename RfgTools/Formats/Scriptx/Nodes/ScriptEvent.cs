using System.Xml.Linq;

namespace RfgTools.Formats.Scriptx.Nodes
{
    public class ScriptEvent : ScriptNode
    {
        public string Name;
        public XElement Inner;

        public ScriptEvent(string name, ScriptNodeType type)
        {
            Name = name;
            ClassType = ScriptNodeClasses.Atom;
            Type = type;
        }
    }
}
