using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using RfgTools.Formats.Scriptx.Nodes;
using RfgTools.Helpers;

namespace RfgTools.Formats.Scriptx
{
    //Todo: Eventually split into Scriptx data class, and ScriptxParser class or just the parser and various node/data types
    public class ScriptxFile
    {
        public int Version;
        public bool Modified;

        public List<ScriptVariable> Variables = new List<ScriptVariable>(); //Todo: Replace with ScriptVar class

        public ActionScript Script { get; private set; } = new ActionScript();

        public ScriptxFile()
        {

        }

        public void ReadFromXml(string inputPath)
        {
            //root
            //Possible level one values:
            //-version
            //-modified
            //-script (can be multiple)
            //-managed (can be multiple)
            //-group (can be multiple)
            //-var (can be multiple)

            //Possible script values
            //-condition -- All scripts start with this, can just be 'true' by default
            //Same subvalues possible as action
            //Also has:
            //-event
            //-delay
            //-action
            //-object -- val = handle?
            //-object_num -- val = num?
            //-function

            var document = XDocument.Load(new FileStream(inputPath, FileMode.Open));
            var root = document.Root;
            if (root == null)
                throw new XmlException($"Input xml doc has no root node! Input path: \"{inputPath}\"");

            //Require the version tag since the game also requires it
            Version = root.GetRequiredValue("version",
                $"Scriptx file does not have a version tag! Required by the game. Input path:  \"{inputPath}\"").ToInt32();

            //Read all variable nodes
            foreach (var varNode in root.Elements("var"))
            {
                Variables.Add(new ScriptVariable(varNode.Value, ScriptNodeType.Variable));
            }

            //Parse the rest of the script. Looks for <script>, <managed>, and <group> blocks
            ScriptRead(root, -1);

            //var scriptNodes = root.Elements("script");
            ////Process script nodes

            ////Starts with condition
            ////Top level item is a function
            ////Top function must return true or false
            ////Functions often contain an event or other functions
            ////Then has one or more actions
            ////Actions have names + args, listed here: https://www.factionfiles.com/ff.php?action=scriptxfuncs

            //foreach (var script in scriptNodes)
            //{
            //    var condition = script.GetRequiredElement("condition");
            //    var actions = script.Elements("action");

            //    //Process condition
            //    //Process actions
            //}

            //var managedNodes = root.Elements("managed");
            ////Process managed nodes

            //var groupNodes = root.Elements("group");
            ////Process group nodes

            var a = 2;
        }

        private int ScriptRead(XElement scriptXml, int groupHandle)
        {
            int numScriptsRead = 0;

            //Loop through each element of scriptXml. Works the same way if this is the root of a scriptx or a <group> block
            foreach (var xmlNode in scriptXml.Elements())
            {
                //Skip script node if it's disabled
                if (xmlNode.Contains("disabled"))
                    continue;

                switch (xmlNode.Name.LocalName)
                {
                    case "script":
                        Script = ScriptParse(scriptXml, groupHandle);
                        numScriptsRead++;
                        break;
                    case "ref":
                        //Todo: Track groups + their names
                        //Parse <group> nodes. Can contain multiple <script> nodes
                        numScriptsRead += ScriptRead(xmlNode, groupHandle);
                        break;
                    case "group":
                        //Todo: Track managed scripts / blocks + their names
                        //Parse <managed> nodes. Can contain multiple <script> nodes
                        numScriptsRead += ScriptRead(xmlNode, groupHandle);
                        break;
                    case "managed":
                        //Parse <ref> nodes. Not sure what these are for yet. Maybe to tell the game it's referencing zone objects
                        numScriptsRead += ScriptRead(xmlNode, groupHandle);
                        break;
                }
            }
            return numScriptsRead;
        }

        private ActionScript ScriptParse(XElement scriptBlock, int groupHandle)
        {
            var script = new ActionScript();

            //Todo: Come up with a better way of handling errors. Ideally it shouldn't throw exceptions unless the problem is unrecoverable
            //Get <condition> node from xml
            var conditionNodeXml = scriptBlock.Element("condition");
            if (conditionNodeXml == null)
                throw new XmlException("Did not find <condition> block in scriptx <script> block. Must be present.");

            //Parse <condition> node
            var conditionNode = NodeParse(conditionNodeXml, script);
            if (conditionNode == null || conditionNode.Type != ScriptNodeType.Bool)
                throw new XmlException("Script block condition invalid or does not evaluate to a boolean.");
            else
                script.Condition = conditionNode;

            //Parse actions
            if (!ParseActions(script, scriptBlock, 0))
                throw new XmlException("Failed to parse script actions.");
            //Parse delayed actions (inside a <delay> block)
            if (!ParseDelayedActions(script, scriptBlock, 0))
                throw new XmlException("Failed to parse script delayed actions");

            //Todo: Figure out how a value of -1 is treated by the game here. Seen in several scripts.
            //Get value of <repeatable> node if present. If not present, set to 0
            script.Repeat = scriptBlock.GetOptionalValue("repeatable", "0").ToInt32();

            return script;
        }

        private ScriptNode NodeParse(XElement nodeXml, ActionScript script)
        {
            //Todo: Consider using something similar to zone object property attributes to split node types & parsing code into different files
            var scriptAtom = nodeXml.Name.LocalName switch
            {
                //Float values
                "number" => new ScriptAtom(nodeXml.Value, ScriptNodeType.Number),

                //String values
                "string" => new ScriptAtom(nodeXml.Value, ScriptNodeType.String),
                "message" => new ScriptAtom(nodeXml.Value, ScriptNodeType.String),
                "tool_tip" => new ScriptAtom(nodeXml.Value, ScriptNodeType.String),
                "destroyed_message" => new ScriptAtom(nodeXml.Value, ScriptNodeType.String),
                "district" => new ScriptAtom(nodeXml.Value, ScriptNodeType.String),

                //Bool value
                "flag" => new ScriptAtom(nodeXml.Value, ScriptNodeType.Bool),

                //Object tags, value is the handle, <object_number> matches the 'num' value for the referenced zone object
                "object" => new ScriptAtom(nodeXml.Value, ScriptNodeType.Object), //Todo: Also get the <object_num> value and label handle + num vals

                //Same as previous, but a list. Root value is "list"
                "object_list" => new ScriptAtom(nodeXml.Value, ScriptNodeType.ObjectList), //Todo: Actually read list of objects

                "event" => ParseEvent(nodeXml), //Todo: Handle this
                "function" => ParseFunction(nodeXml), //Todo: Handle this

                //Script point. Root value is "script_point". Contains an <object> node and a <number> node
                "script_point" => new ScriptAtom(nodeXml.Value, ScriptNodeType.Point), //Todo: Actually read the values

                //Variable reference. Used by actions to modify a variable. Just a string. Todo: Check that the variable has already been declared
                "variable_ref" => new ScriptAtom(nodeXml.Value, ScriptNodeType.Ref),

                //Weapon name from weapons.xtbl
                "weapon" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "vrail_type" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                //Yes, there's a typo. The game uses this spelling in code and in scripts so it's staying unfixed.
                "cont_atttack" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "radio_message_type" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "voice_persona" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "unlocking_voice_line_group" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "team" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "alert_level" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "density_level" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "despawn_type" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "voice_line" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "anim_action" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "vehicle_type" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "human_type" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "effect_type" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "explosion" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "squad" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "convoy" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "raid_state" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "activity_state" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "air_bomb" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "objective" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "mission_objective" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "foley_type" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "music_threshold" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "upgrades" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "pa_group" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                "mission_handle" => new ScriptAtom(nodeXml.Value, ScriptNodeType.GameHandle),
                _ => null
            };

            return scriptAtom;
        }

        private bool ParseActions(ActionScript script, XElement root, int delay)
        {
            //Todo: Write this

            return true;
        }

        private bool ParseDelayedActions(ActionScript script, XElement root, int parentDelay)
        {
            //Todo: Write this

            return true;
        }

        private ScriptNode ParseEvent(XElement nodeXml)
        {
            //Todo: Write this
            return new ScriptEvent(nodeXml.Name.LocalName, ScriptNodeType.Event);
        }

        private ScriptNode ParseFunction(XElement nodeXml)
        {
            //Todo: Write this
            return new ScriptFunction(nodeXml.Name.LocalName, ScriptNodeType.Function);
        }

        //Todo: Write functions or separate writer class for writing back to xml
        //Todo: Add input/output function for binary format used in save files
    }
}
