
namespace Dynamo.AutoTT
{
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public class Template
	{
		private Trigger[] triggerField;
		private string _nameField;
		private bool _onBuildField;

		public Template()
		{
			_onBuildField = false;
		}

		[System.Xml.Serialization.XmlElementAttribute("trigger", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public Trigger[] Trigger
		{
			get { return triggerField; }
			set { triggerField = value; }
		}

		[System.Xml.Serialization.XmlAttributeAttribute("name")]
		public string Name
		{
			get { return _nameField; }
			set { _nameField = value; }
		}

		[System.Xml.Serialization.XmlAttributeAttribute("onbuild")]
		[System.ComponentModel.DefaultValueAttribute(false)]
		public bool OnBuild
		{
			get { return _onBuildField; }
			set { _onBuildField = value; }
		}
	}
}
