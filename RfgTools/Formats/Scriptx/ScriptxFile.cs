using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using RfgTools.Helpers;

namespace RfgTools.Formats.Scriptx
{
    //Todo: Eventually split into Scriptx data class, and ScriptxParser class
    public class ScriptxFile
    {
        public int Version;
        public bool Modified;

        public List<string> Variables; //Todo: Replace with ScriptVar class

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

            Version = root.GetRequiredValue("version").ToInt32();
            Modified = root.GetRequiredValue("modified").ToBool();

            var varNodes = root.GetMultipleElements("var");
            if (varNodes != null)
            {
                foreach (var node in varNodes)
                {
                    Variables.Add(node.Name.LocalName);
                }
            }

            var scriptNodes = root.GetMultipleElements("script");
            if (scriptNodes != null)
            {
                //Process script nodes

                //Starts with condition
                    //Top level item is a function
                        //Top function must return true or false
                        //Functions often contain an event or other functions
                //Then has one or more actions
                    //Actions have names + args, listed here: https://www.factionfiles.com/ff.php?action=scriptxfuncs

                foreach (var script in scriptNodes)
                {
                    var condition = script.GetRequiredElement("condition");
                    var actions = script.GetMultipleElements("action");

                    //Process condition
                    //Process actions
                }
            }

            var managedNodes = root.GetMultipleElements("managed");
            if (managedNodes != null)
            {
                //Process managed nodes
            }

            var groupNodes = root.GetMultipleElements("group");
            if (groupNodes != null)
            {
                //Process group nodes
            }

            var a = 2;
        }

        public void WriteToXml(string outputPath)
        {

        }
    }
}
