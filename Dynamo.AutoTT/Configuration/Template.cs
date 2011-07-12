
namespace Dynamo.AutoTT
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public class Template
	{
		private Trigger[] _triggerField;
		private string _nameField;
		private bool _onBuildField;

		/// <remarks/>
		public Template()
		{
			_onBuildField = false;
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("trigger", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public Trigger[] Trigger
		{
			get { return _triggerField; }
			set { _triggerField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute("name")]
		public string Name
		{
			get { return _nameField; }
			set { _nameField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute("onbuild")]
		[System.ComponentModel.DefaultValueAttribute(false)]
		public bool OnBuild
		{
			get { return _onBuildField; }
			set { _onBuildField = value; }
		}
	}
}
