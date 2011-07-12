using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Dynamo.AutoTT
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public class Trigger
	{
		private string _patternField;
		private Regex _regex;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute("pattern")]
		public string Pattern
		{
			get { return _patternField; }
			set { _patternField = value; }
		}

		/// <remarks/>
		public bool IsMatch(string filename)
		{
			if (_regex == null)
			{
				try
				{
					_regex = new Regex(this.Pattern, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
				}
				catch (Exception)
				{
					// Use OutputManager somehow, ioc or let call catch exception
					MessageBox.Show("AutoTT reporting: Problem parsing the pattern - " + this.Pattern);
					return false;
				}
			}

			return _regex.IsMatch(filename);
		}
	}
}
