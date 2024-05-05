using Siemens.Engineering.CrossReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace TIA程序生成.Common.Models
{
    public class Configuration
    {

        // 根类，代表整个XML文档
        public class Document
        {
            [XmlElement("Engineering")]
            public Engineering Engineering { get; set; }

            [XmlElement("SW.Blocks.OB")]
            public SWBlocksOB SWBlocksOB { get; set; }
        }

        public class Engineering
        {
            [XmlAttribute("version")]
            public string Version { get; set; }
        }

        public class SWBlocksOB
        {
            [XmlAttribute("ID")]
            public int ID { get; set; }
            public BlockAttributeList AttributeList { get; set; }
            public BlockObjectList ObjectList { get; set; }
        }

        #region BlockAttributeList
        public class BlockAttributeList
        {
            [XmlElement("Name")]
            public string Name { get; set; }

            [XmlElement("Namespace")]
            public string Namespace { get; set; } 

            [XmlElement("Number")]
            public int Number { get; set; }

            [XmlElement("ProgrammingLanguage")]
            public string ProgrammingLanguage { get; set; }
        }
        #endregion

        #region BlockObjectList
        public class BlockObjectList
        {
            [XmlElement("MultilingualText", typeof(MultilingualText))]
            [XmlElement("SW.Blocks.CompileUnit", typeof(SWBlocksCompileUnit))]
            public List<object> Items { get; set; }
        }

        #region MultilingualText
        public class MultilingualText
        {
            [XmlAttribute("ID")]
            public int ID { get; set; }

            [XmlAttribute("CompositionName")]
            public string CompositionName = "Comment";

            [XmlElement("ObjectList")]
            public ObjectList ObjectList { get; set; }
        }

        public class ObjectList
        {
            [XmlElement("MultilingualTextItem")]
            public MultilingualTextItem MultilingualTextItem { get; set; }
        }

        public class MultilingualTextItem
        {
            [XmlAttribute("ID")]
            public int ID { get; set; }

            [XmlAttribute("CompositionName")]
            public string CompositionName { get; set; }

            [XmlElement("AttributeList")]
            public AttributeList AttributeList { get; set; }
        }

        public class AttributeList
        {
            [XmlElement("Culture")]
            public string Culture { get; set; }

            [XmlElement("Text")]
            public string Text { get; set; }
        }
        #endregion
        public class SWBlocksCompileUnit
        {
            [XmlAttribute("ID")]
            public int ID { get; set; }

            [XmlAttribute("CompositionName")]
            public string CompositionName { get; set; }

            [XmlElement("AttributeList")]
            public SWBlocksCompileUnitAttributeList AttributeList { get; set; }

            [XmlElement("ObjectList")]
            public SWBlocksCompileUnitObjectList ObjectList { get; set; }
        }

        public class SWBlocksCompileUnitAttributeList
        {
            [XmlElement("NetworkSource")]
            public SWBlocksCompileUnitNetworkSource NetworkSource { get; set; }
        }

        public class SWBlocksCompileUnitNetworkSource
        {
            [XmlElement("FlgNet")]
            public SWBlocksCompileUnitFlgNet FlgNet { get; set; }
        }

        public class SWBlocksCompileUnitFlgNet
        {
            [XmlElement("Parts")]
            public SWBlocksCompileUnitParts Parts { get; set; }

            [XmlElement("Wires")]
            public SWBlocksCompileUnitWires Wires { get; set; }
        }

        public class SWBlocksCompileUnitParts
        {
            [XmlElement("Access")]
            public Access Accesses { get; set; }

            [XmlElement("Part")]
            public Part Parts { get; set; }
        }

        public class SWBlocksCompileUnitWires
        {
            //[XmlElement("Wire")]
            //public Wire Wire { get; set; }
        }

        public class Access
        {
            [XmlAttribute("Scope")]
            public string Scope { get; set; }

            [XmlAttribute("UId")]
            public int UId { get; set; }

            // 使用XmlElement来标记可能的子元素
            [XmlElement("Symbol", typeof(Symbol))]
            [XmlElement("Constant", typeof(Constant))]
            public object SpecificElement { get; set; }
        }

        public class Symbol
        {
            [XmlElement("Component")]
            public List<Component> Components { get; set; }
        }

        public class Constant
        {
            [XmlElement("ConstantType")]
            public string ConstantType { get; set; }

            [XmlElement("ConstantValue")]
            public int ConstantValue { get; set; }
        }

        public class Component
        {
            [XmlAttribute("Name")]
            public string Name { get; set; }
        }

        public class Part
        {
            [XmlAttribute("Name")]
            public string Name { get; set; }

            [XmlAttribute("UId")]
            public int UId { get; set; }

            [XmlAttribute("DisabledENO")]
            public bool DisabledENO { get; set; }

            // 可以添加其他属性以匹配Part元素的复杂内容
            [XmlElement("TemplateValue")]
            public TemplateValue TemplateValue { get; set; }
        }

        //public class Wire
        //{
        //    [XmlAttribute("UId")]
        //    public int UId { get; set; }

        //    //[XmlElement("Powerrail")]
        //    //public object Powerrail { get; set; } // 假设Powerrail没有属性

        //    [XmlElement("NameCon")]
        //    [XmlElement("IdentCon")]
        //    public List<Connection> Connections { get; set; }
        //}

        public class Connection
        {
            [XmlAttribute("UId")]
            public int UId { get; set; }

            [XmlAttribute("Name")]
            public string Name { get; set; }
        }

        public class TemplateValue
        {
            [XmlAttribute("Name")]
            public string Name { get; set; }

            [XmlAttribute("Type")]
            public string Type { get; set; }

            public string Value { get; set; }
        }

        public class SWBlocksCompileUnitObjectList
        {
           
            [XmlElement("MultilingualText", typeof(MultilingualText))]

            public List<MultilingualText> MultilingualTexts { get; set; }
        }
        #endregion
    }
}

