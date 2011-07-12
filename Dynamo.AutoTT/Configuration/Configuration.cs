
namespace Dynamo.AutoTT
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute("configuration", Namespace = "", IsNullable = false)]
	public class Configuration
	{
		private Template[] _templatesField;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("template", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public Template[] Templates
		{
			get { return _templatesField; }
			set { _templatesField = value; }
		}
	}
}
