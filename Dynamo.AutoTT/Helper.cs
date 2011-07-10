using System.IO;
using EnvDTE;

namespace Dynamo.AutoTT
{
	internal static class Helper
	{
		public static string GetRelativePath(Project root, string file)
		{
			var projectRoot = Path.GetDirectoryName(root.FullName);
			return GetRelativePath(projectRoot, file);
		}

		public static string GetRelativePath(string root, string file)
		{
			// Any better way of doing it ? - IndexOf and Substring ?

			var newStr = file.Replace(root, "");

			// if StartsWith("\\")
				// newStr = newStr.Substring(1); ???

			// Always remove the leading "\" ?
			// Should url used for matching be "\Controllers\HomeController.cs" or "Controllers\HomeController.cs" ?

			newStr = newStr.Substring(1);

			return newStr;
		}
	}
}