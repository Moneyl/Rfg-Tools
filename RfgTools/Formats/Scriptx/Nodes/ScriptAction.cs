
namespace RfgTools.Formats.Scriptx.Nodes
{
    public class ScriptAction : ScriptNode
    {
        public ScriptNode RootNode { get; }

        ScriptAction(ScriptNode rootNode)
        {
            RootNode = rootNode;
            ClassType = ScriptNodeClasses.Atom;
            Type = ScriptNodeType.Action;
        }
    }
}