using System.Collections.Generic;
using RfgTools.Formats.Scriptx.Nodes;

namespace RfgTools.Formats.Scriptx
{
    public class ActionScript
    {
        //Todo: Figure out if scripts with > 2 actions work. Game has an object_pool_client<script_action,2> here but have seen scripts with more than 2 actions
        private List<ScriptAction> _actions = new List<ScriptAction>();

        public int Repeat = 0;
        public ScriptNode Condition { get; set; }
        public IReadOnlyList<ScriptAction> Actions => _actions;

        public ActionScript()
        {
            
        }

        public void AddAction(ScriptAction action)
        {
            _actions.Add(action);
        }
    }
}
