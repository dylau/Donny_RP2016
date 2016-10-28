using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace RP1516
{
    public class StepXml
    {
        // This class generate one xml-file for one fiber step. Functions will be run in loops. 
        // All the xml syntax is modified here!

        public Fiber StepCurve;
        public int Step;
        public string path;

        public StepXml(Fiber iFiber, int iStep, string Path)
        {
            StepCurve = iFiber;
            Step = iStep;
            path = Path;
        }

        public void Write()
        {
            // digit control
            string Step3Digit = null;
            if (Step < 10)
                Step3Digit = String.Format("00{0}", Step);

            if (10 <= Step && Step < 100)
                Step3Digit = String.Format("0{0}", Step);

            if (100 <= Step && Step < 1000)
                Step3Digit = Step.ToString();

            // settings 
            XmlWriterSettings Settings = new XmlWriterSettings();
            Settings.Indent = true;
            Settings.IndentChars = "\t";
            Settings.OmitXmlDeclaration = true;

            XmlDocument xmlDoc = new XmlDocument(); // This is the xml file of 1 fiber.
            
            // schema
            // rootNode, step general info  

            XmlNode stepNode = xmlDoc.CreateElement("Step");

            //
            XmlAttribute stepID = xmlDoc.CreateAttribute("id");
            stepID.Value = Step3Digit.ToString();
            stepNode.Attributes.Append(stepID);

            if (StepCurve.Direction == "AB")
            {
                XmlAttribute PinStart = xmlDoc.CreateAttribute("pinStart");
                PinStart.Value = StepCurve.PinA.PinID;
                stepNode.Attributes.Append(PinStart);

                XmlAttribute PinEnd = xmlDoc.CreateAttribute("pinEnd");
                PinEnd.Value = StepCurve.PinB.PinID;
                stepNode.Attributes.Append(PinEnd);
            }

            if (StepCurve.Direction == "BA")
            {
                XmlAttribute PinStart = xmlDoc.CreateAttribute("pinStart");
                PinStart.Value = StepCurve.PinB.PinID;
                stepNode.Attributes.Append(PinStart);

                XmlAttribute PinEnd = xmlDoc.CreateAttribute("pinEnd");
                PinEnd.Value = StepCurve.PinA.PinID;
                stepNode.Attributes.Append(PinEnd);
            }


            XmlAttribute Wind_pin = xmlDoc.CreateAttribute("wind_pin");
            if (StepCurve.Direction == "AB")
                Wind_pin.Value = string.Format("W_{0}", StepCurve.PinB.Index.ToString());
            else
                Wind_pin.Value = string.Format("W_{0}", StepCurve.PinA.Index.ToString());
            stepNode.Attributes.Append(Wind_pin);


            xmlDoc.AppendChild(stepNode);

            // Drone
            TaskNode droneNode_0 = new TaskNode("0", "Drone", StepCurve.Direction, xmlDoc); 
            stepNode.AppendChild(droneNode_0.WriteTaskNode());

            // Robot
            TaskNode robotNode_1 = new TaskNode("1", string.Format("Robot{0}", StepCurve.EndPinID[0]), "50", xmlDoc); 
            stepNode.AppendChild(robotNode_1.WriteTaskNode());

            // Grip
            TaskNode robotNode_2 = new TaskNode("2", string.Format("Gripper{0}", StepCurve.EndPinID[0]), "On", xmlDoc);
            stepNode.AppendChild(robotNode_2.WriteTaskNode());

            // Magnet: off
            TaskNode robotNode_3 = new TaskNode("3", "Magnet", "Off", xmlDoc); 
            stepNode.AppendChild(robotNode_3.WriteTaskNode());

            // Tension 
            TaskNode robotNode_4 = new TaskNode("4", "Tension", "On", xmlDoc); 
            stepNode.AppendChild(robotNode_4.WriteTaskNode());

            // Robot: out with effector
            TaskNode robotNode_5 = new TaskNode("5", string.Format("Robot{0}", StepCurve.EndPinID[0]), "61", xmlDoc);
            stepNode.AppendChild(robotNode_5.WriteTaskNode());

            // Robot: travel forward
            TaskNode robotNode_6 = new TaskNode("6", string.Format("Robot{0}", StepCurve.EndPinID[0]), "70", xmlDoc);
            stepNode.AppendChild(robotNode_6.WriteTaskNode());

            // Robot: wind 
            TaskNode robotNode_7 = new TaskNode("7", string.Format("Robot{0}", StepCurve.EndPinID[0]), "90", xmlDoc);
            stepNode.AppendChild(robotNode_7.WriteTaskNode());

            /*
            TaskNode robotNode_7 = new TaskNode("7", string.Format("Robot{0}", StepCurve.EndPinID[0]), string.Format("W_{0}", StepCurve.PinB.Index.ToString()), xmlDoc);
            if (StepCurve.Direction == "BA")
                robotNode_7.Command = string.Format("W_{0}", StepCurve.PinA.Index.ToString());
            stepNode.AppendChild(robotNode_7.WriteTaskNode());
            */

            // Robot: travel back
            TaskNode robotNode_8 = new TaskNode("8", string.Format("Robot{0}", StepCurve.EndPinID[0]), "80", xmlDoc);
            stepNode.AppendChild(robotNode_8.WriteTaskNode());

            // Robot: in with effector
            TaskNode robotNode_9 = new TaskNode("9", string.Format("Robot{0}", StepCurve.EndPinID[0]), "51", xmlDoc);
            stepNode.AppendChild(robotNode_9.WriteTaskNode());

            // Magnet: on
            TaskNode robotNode_10 = new TaskNode("10", "Magnet", "On", xmlDoc);
            stepNode.AppendChild(robotNode_10.WriteTaskNode());

            // Tension 
            TaskNode robotNode_11 = new TaskNode("11", "Tension", "Off", xmlDoc);
            stepNode.AppendChild(robotNode_11.WriteTaskNode());

            // Gripper
            TaskNode robotNode_12 = new TaskNode("12", string.Format("Gripper{0}", StepCurve.EndPinID[0]), "Off", xmlDoc);
            stepNode.AppendChild(robotNode_12.WriteTaskNode());

            // Robot: out without effector
            TaskNode robotNode_13 = new TaskNode("13", string.Format("Robot{0}", StepCurve.EndPinID[0]), "60", xmlDoc);
            stepNode.AppendChild(robotNode_13.WriteTaskNode());

            // writing and namming the xml file
            //XmlWriter write = XmlWriter.Create(string.Format("C:\\Users\\dyliu\\desktop\\TaskList\\TaskList_{0}.xml", Step3Digit), Settings);
            XmlWriter write = XmlWriter.Create(string.Format("{0}\\TaskList_{1}.xml", path, Step3Digit), Settings);

            xmlDoc.Save(write);
        }

    }
}
