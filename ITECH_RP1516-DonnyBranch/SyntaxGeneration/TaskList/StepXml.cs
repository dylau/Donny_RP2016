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
            string Step4Digit = null;

            if (Step < 10)
            {
                Step4Digit = String.Format("000{0}", Step);
                Step3Digit = String.Format("00{0}", Step);
            }


            if (10 <= Step && Step < 100)
            {
                Step4Digit = String.Format("00{0}", Step);
                Step3Digit = String.Format("0{0}", Step);
            }

            if (100 <= Step && Step < 1000)
            {
                Step4Digit = String.Format("0{0}", Step);
                Step3Digit = String.Format("{0}", Step);
            }

            if (1000 <= Step && Step < 10000)
            {
                Step4Digit = Step.ToString();
            }

            // settings 
            XmlWriterSettings Settings = new XmlWriterSettings();
            Settings.Indent = true;
            Settings.IndentChars = "\t";
            Settings.OmitXmlDeclaration = true;
            XmlDocument xmlDoc = new XmlDocument(); // This is the xml file of 1 fiber.
            
            // stepNode
            XmlNode stepNode = xmlDoc.CreateElement("Step");

            // no neighbours
            XmlAttribute stepID = xmlDoc.CreateAttribute("id");
            stepID.Value = Step4Digit.ToString();
            stepNode.Attributes.Append(stepID);


            XmlAttribute Wind_pin = xmlDoc.CreateAttribute("wind_pin");

            string OperationSide = null;
            if (StepCurve.Direction == "AB")
            {
                OperationSide = "B";

                XmlAttribute PinStart = xmlDoc.CreateAttribute("pinStart");
                PinStart.Value = StepCurve.PinA.PinID;
                stepNode.Attributes.Append(PinStart);

                XmlAttribute PinEnd = xmlDoc.CreateAttribute("pinEnd");
                PinEnd.Value = StepCurve.PinB.PinID;
                stepNode.Attributes.Append(PinEnd);

                Wind_pin.Value = string.Format("W_{0}", StepCurve.PinB.Index.ToString());

            }

            if (StepCurve.Direction == "BA")
            {
                OperationSide = "A";

                XmlAttribute PinStart = xmlDoc.CreateAttribute("pinStart");
                PinStart.Value = StepCurve.PinB.PinID;
                stepNode.Attributes.Append(PinStart);

                XmlAttribute PinEnd = xmlDoc.CreateAttribute("pinEnd");
                PinEnd.Value = StepCurve.PinA.PinID;
                stepNode.Attributes.Append(PinEnd);

                Wind_pin.Value = string.Format("W_{0}", StepCurve.PinA.Index.ToString());

            }
            stepNode.Attributes.Append(Wind_pin);

            // stepNode done
            xmlDoc.AppendChild(stepNode);

            

            // Drone
            TaskNode Node_0 = new TaskNode("0", "Drone", StepCurve.Direction, xmlDoc); 
            stepNode.AppendChild(Node_0.WriteTaskNode());

            // Effectors: Gripper
            TaskNode Node_1 = new TaskNode("1", "Effectors", string.Format("G{0}OPEN", OperationSide), xmlDoc);
            stepNode.AppendChild(Node_1.WriteTaskNode());

            // Robot: 
            TaskNode Node_2 = new TaskNode("2", string.Format("Robot{0}", OperationSide), "50", xmlDoc); 
            stepNode.AppendChild(Node_2.WriteTaskNode());

            // Effectors: Gripper
            TaskNode Node_3 = new TaskNode("3", "Effectors", string.Format("G{0}CLOSE", OperationSide), xmlDoc);
            stepNode.AppendChild(Node_3.WriteTaskNode());

            // Magnet: off
            TaskNode Node_4 = new TaskNode("4", "Drone", "MOFF", xmlDoc); 
            stepNode.AppendChild(Node_3.WriteTaskNode());

            // Tension 
            TaskNode Node_5 = new TaskNode("5", "Effectors", "TON", xmlDoc); 
            stepNode.AppendChild(Node_5.WriteTaskNode());

            // Robot: out with effector
            TaskNode Node_6 = new TaskNode("6", string.Format("Robot{0}", OperationSide), "61", xmlDoc);
            stepNode.AppendChild(Node_6.WriteTaskNode());

            // Robot: travel forward
            TaskNode Node_7 = new TaskNode("7", string.Format("Robot{0}", OperationSide), "70", xmlDoc);
            stepNode.AppendChild(Node_7.WriteTaskNode());

            // Robot: wind 
            TaskNode Node_8 = new TaskNode("8", string.Format("Robot{0}", OperationSide), "90", xmlDoc);
            stepNode.AppendChild(Node_7.WriteTaskNode());

            // Robot: travel back
            TaskNode Node_9 = new TaskNode("9", string.Format("Robot{0}", OperationSide), "80", xmlDoc);
            stepNode.AppendChild(Node_9.WriteTaskNode());

            // Robot: in with effector
            TaskNode Node_10 = new TaskNode("10", string.Format("Robot{0}", OperationSide), "51", xmlDoc);
            stepNode.AppendChild(Node_10.WriteTaskNode());

            // Magnet: on
            TaskNode Node_11 = new TaskNode("11", "Drone", "MON", xmlDoc);
            stepNode.AppendChild(Node_11.WriteTaskNode());

            // Tension 
            TaskNode Node_12 = new TaskNode("12", "Effectors", "TOFF", xmlDoc);
            stepNode.AppendChild(Node_12.WriteTaskNode());

            // Gripper
            TaskNode Node_13 = new TaskNode("13", "Effectors", string.Format("G{0}OPEN", OperationSide), xmlDoc);
            stepNode.AppendChild(Node_13.WriteTaskNode());

            // Robot: out without effector
            TaskNode Node_14 = new TaskNode("14", string.Format("Robot{0}", OperationSide), "60", xmlDoc);
            stepNode.AppendChild(Node_14.WriteTaskNode());

            // writing and namming the xml file
            XmlWriter write = XmlWriter.Create(string.Format("{0}\\TaskList_{1}.xml", path, Step4Digit), Settings);

            xmlDoc.Save(write);
        }

    }
}
