//using System.Xml;
//using Microsoft.Build.Framework;
//using Microsoft.Build.Utilities;

//namespace Xunit.Runner.MSBuild
//{
//    public class CombineXunitXml : Task
//    {
//        [Required]
//        public ITaskItem[] InputFiles { get; set; }

//        [Required]
//        public ITaskItem OutputFile { get; set; }

//        public override bool Execute()
//        {
//            XmlDocument outputDoc = new XmlDocument();
//            outputDoc.LoadXml("<assemblies/>");
//            XmlNode assembliesNode = outputDoc.ChildNodes[0];

//            foreach (ITaskItem inputFile in InputFiles)
//            {
//                XmlDocument inputDoc = new XmlDocument();
//                inputDoc.Load(inputFile.GetMetadata("FullPath"));

//                assembliesNode.InnerXml += inputDoc.OuterXml;
//            }

//            outputDoc.Save(OutputFile.GetMetadata("FullPath"));
//            return true;
//        }
//    }
//}