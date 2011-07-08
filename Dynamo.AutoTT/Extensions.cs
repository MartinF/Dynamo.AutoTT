using System;
using System.Linq;
using Dynamo.AutoTT.Configuration;
using EnvDTE;

namespace Dynamo.AutoTT
{
	internal static class Extensions
	{
		public static configuration LoadConfigurationAndExecuteAllTemplates(this Core core, ProjectItem configurationItem)
		{
			var config = core.LoadConfiguration(configurationItem);

			if (config != null)
				core.ExecuteAllTemplates(configurationItem.ContainingProject, config);

			return config;
		}

		public static bool IsDefault<T>(this T value)
			where T : struct
		{
			return value.Equals(default(T));
		}

		public static bool IsFile(this ProjectItem item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			return item.Kind == Constants.vsProjectItemKindPhysicalFile;
		}

		public static bool IsFile(this ProjectItem item, string name, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
		{
			if (item == null)
				throw new ArgumentNullException("item");
			if (name == null)
				throw new ArgumentNullException("name");

			return item.IsFile() && item.Name.Equals(name, comparisonType);
		}

		public static bool IsFolder(this ProjectItem item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			return item.Kind == Constants.vsProjectItemKindPhysicalFolder;
		}

		public static bool IsFolder(this ProjectItem item, string name, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
		{
			if (item == null)
				throw new ArgumentNullException("item");
			if (name == null)
				throw new ArgumentNullException("name");

			return item.IsFolder() && item.Name.Equals(name, comparisonType);
		}

		public static ProjectItem GetFile(this ProjectItems items, string name)
		{
			if (items == null)
				throw new ArgumentNullException("items");
			if (name == null)
				throw new ArgumentNullException("name");

			return items.Cast<ProjectItem>().FirstOrDefault(item => item.IsFile(name));
		}

		public static ProjectItem GetFolder(this ProjectItems items, string name)
		{
			if (items == null)
				throw new ArgumentNullException("items");
			if (name == null)
				throw new ArgumentNullException("name");

			return items.Cast<ProjectItem>().FirstOrDefault(item => item.IsFolder(name));
		}

		public static ProjectItem GetItem(this Project project, string filename)
		{
			if (project == null)
				throw new ArgumentNullException("project");
			if (filename == null)
				throw new ArgumentNullException("filename");

			return project.ProjectItems != null ? project.ProjectItems.GetItem(filename) : null;
		}

		public static ProjectItem GetItem(this ProjectItems items, string filename)
		{
			// Smarter searching than using recusion - accepts relative path to the items
			// Doesnt support for searching nested items currently

			if (items == null)
				throw new ArgumentNullException("items");
			if (filename == null)
				throw new ArgumentNullException("filename");

			// All but the last should be the file
			var foldersAndFile = filename.Split('\\');
			var current = items;

			for (int i = 0; i < foldersAndFile.Length; i++)
			{
				var name = foldersAndFile[i];

				if (i + 1 == foldersAndFile.Length)	// file
				{
					return current.GetFile(name);
				}

				var folderItem = current.GetFolder(name);
				if (folderItem != null && folderItem.ProjectItems != null)
					current = folderItem.ProjectItems;
				else
					break;
			}

			return null;
		}

		public static ProjectItem GetItem(this Project project, Func<ProjectItem, bool> projection)
		{
			if (project == null)
				throw new ArgumentNullException("project");
			if (projection == null)
				throw new ArgumentNullException("projection");

			return project.ProjectItems != null ? GetItem(project.ProjectItems, projection) : null;
		}

		public static ProjectItem GetItem(this ProjectItems items, Func<ProjectItem, bool> projection)
		{
			// Using recursion

			if (items == null)
				throw new ArgumentNullException("items");
			if (projection == null)
				throw new ArgumentNullException("projection");

			foreach (ProjectItem item in items)
			{
				if (projection(item))
					return item;

				if (item.ProjectItems != null)
				{
					var nestedItem = GetItem(item.ProjectItems, projection);

					if (nestedItem != null)
						return nestedItem;
				}
			}

			return null;
		}

		public static bool WasConfiguration(this ProjectItem item, string oldName)
		{
			if (item == null)
				throw new ArgumentNullException("item");
			if (oldName == null)
				throw new ArgumentNullException("oldName");

			return item.IsFile() && Core.ConfigFile.Equals(oldName, StringComparison.OrdinalIgnoreCase);
		}

		public static bool IsConfiguration(this ProjectItem item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			return item.IsFile(Core.ConfigFile);
		}

		//public static bool IsTextTemplate(this ProjectItem item, string textTemplateName)
		//{
		//    if (item == null)
		//        throw new ArgumentNullException("item");
		//    if (textTemplateName == null)
		//        throw new ArgumentNullException("textTemplateName");

		//    // Check if CustomTool is associated happens in the Execute method instead ?
		//    // && item.Properties != null && ((string)item.Properties.Item("CustomTool").Value) == "TextTemplatingFileGenerator"

		//    return item.IsFile(textTemplateName);
		//}
	}
}