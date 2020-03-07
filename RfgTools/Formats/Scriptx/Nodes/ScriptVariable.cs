
namespace RfgTools.Formats.Scriptx.Nodes
{
    public class ScriptVariable : ScriptNode
    {
        public string VariableName;

        public ScriptVariable(string variableName, ScriptNodeType type)
        {
            VariableName = variableName;
            ClassType = ScriptNodeClasses.Variable;
            Type = type;
        }
    }
}
