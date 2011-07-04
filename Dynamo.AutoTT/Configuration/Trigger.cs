using System.Text.RegularExpressions;

// Partial extending the configuration with some simple logic

namespace Dynamo.AutoTT.Configuration
{
	public partial class configurationTemplateTrigger
	{
		private Regex _regex;

		public bool IsMatch(string filename)
		{
			if (_regex == null)
			{
				_regex = new Regex(this.pattern, RegexOptions.Compiled);	// IgnoreCase ? CultureInvariant ?
			}

			return _regex.IsMatch(filename);
		}
	}
}
