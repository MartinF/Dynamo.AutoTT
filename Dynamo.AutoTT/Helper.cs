using System;
using System.IO;
using EnvDTE;

namespace Dynamo.AutoTT
{
	internal static class Helper
	{
		public static string GetRelativePath(Project root, string file)
		{
			if (root == null)
				throw new ArgumentNullException("root");
			if (file == null)
				throw new ArgumentNullException("file");

			var projectRoot = Path.GetDirectoryName(root.FullName);
			return GetRelativePath(projectRoot, file);
		}

		public static string GetRelativePath(string root, string file)
		{
			if (root == null)
				throw new ArgumentNullException("root");
			if (file == null)
				throw new ArgumentNullException("file");

			// Any better way of doing it ? - IndexOf and Substring ?

			var newStr = file.Replace(root, "");

			// if StartsWith("\\")
				// newStr = newStr.Substring(2); 

			newStr = newStr.Substring(1);	// 1 eller 2 ? - Nothing wrong with it starting with \\ ? but it is used for IsMatch and it should work using ^Content\ ? 

			return newStr;
		}
	}
}