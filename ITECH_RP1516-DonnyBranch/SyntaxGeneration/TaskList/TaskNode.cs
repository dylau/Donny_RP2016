using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace RP1516
{
    public class TaskNode
    {
        public string ID;
        public string Agent;
        public string Command;
        public XmlDocument Documnent;

        public TaskNode(string id, string agent, string command, XmlDocument document)
        {
            ID = id;
            Agent = agent;
            Command = command;
            Documnent = document;
        }

        public XmlNode WriteTaskNode()
        {
            XmlNode node = Documnent.CreateElement("Task");

            // 3 attributes
            XmlAttribute nodeID = Documnent.CreateAttribute("id");
            nodeID.Value = ID;
            node.Attributes.Append(nodeID);

            XmlAttribute nodeAgent = Documnent.CreateAttribute("agent");
            nodeAgent.Value = Agent;
            node.Attributes.Append(nodeAgent);

            XmlAttribute nodeCommand = Documnent.CreateAttribute("command");
            nodeCommand.Value = Command;
            node.Attributes.Append(nodeCommand);

            return node;
        }
       
    }
}
