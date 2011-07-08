using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

// Partial extending the configuration with some simple logic for testing the Trigger

namespace Dynamo.AutoTT.Configuration
{
	public partial class configurationTemplateTrigger
	{
		private Regex _regex;

		public bool IsMatch(string filename)
		{
			if (_regex == null)
			{
				try
				{
					_regex = new Regex(this.pattern, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
				}
				catch (Exception)
				{
					MessageBox.Show("AutoTT reporting: Problem parsing the pattern - " + this.pattern);
					return false;
				}
			}
			
			return _regex.IsMatch(@filename);		// Dont need to replace \\ if using @ ? how to fix it ? or always replace \ with \\ ? 
		}
	}
}
