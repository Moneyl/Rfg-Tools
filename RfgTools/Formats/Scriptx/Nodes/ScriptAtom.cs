
namespace RfgTools.Formats.Scriptx.Nodes
{
    public class ScriptAtom : ScriptNode
    {
        //Todo: Figure out way to handle multiple value types without using strings. Game uses a union for the value.
        public string Value;

        public ScriptAtom(string value, ScriptNodeType type)
        {
            Value = value;
            ClassType = ScriptNodeClasses.Atom;
            Type = type;
        }
    }
}
