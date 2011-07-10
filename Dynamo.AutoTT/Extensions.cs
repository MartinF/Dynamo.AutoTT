using System;
using System.Linq;
using EnvDTE;

// Faster to use Solution.FindProjectItem() rather than my own GetItem ?
// Make sure it accepts the url as \Project\Item - else it cant be used.

namespace Dynamo.AutoTT
{
	internal static class Extensions
	{
		public static Configuration LoadConfigurationAndExecuteAllTemplates(this Core core, ProjectItem configurationItem)
		{
			var config = core.LoadConfiguration(configurationItem);

			if (config != null)
				core.ExecuteAllTemplates(configurationItem.ContainingProject, config);

			return config;
		}



		public static bool IsFile(this ProjectItem item)
		{
			return item.Kind == Constants.vsProjectItemKindPhysicalFile;
		}

		public static bool IsFile(this ProjectItem item, string name, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
		{
			return item.IsFile() && item.Name.Equals(name, comparisonType);
		}

		public static bool IsFolder(this ProjectItem item)
		{
			return item.Kind == Constants.vsProjectItemKindPhysicalFolder;
		}

		public static bool IsFolder(this ProjectItem item, string name, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
		{
			return item.IsFolder() && item.Name.Equals(name, comparisonType);
		}

		public static ProjectItem GetFile(this ProjectItems items, string name)
		{
			return items.Cast<ProjectItem>().FirstOrDefault(item => item.IsFile(name));
		}

		public static ProjectItem GetFolder(this ProjectItems items, string name)
		{
			return items.Cast<ProjectItem>().FirstOrDefault(item => item.IsFolder(name));
		}

		public static ProjectItem GetItem(this Project project, string relativeFilename)
		{
			return project.ProjectItems != null ? project.ProjectItems.GetItem(relativeFilename) : null;
		}

		public static ProjectItem GetItem(this ProjectItems items, string relativeFilename)
		{
			// Smarter searching than using recusion - accepts relative path to the items
			// Doesnt support for searching nested items currently

			var foldersAndFile = relativeFilename.Split('\\');
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
			return project.ProjectItems != null ? GetItem(project.ProjectItems, projection) : null;
		}

		public static ProjectItem GetItem(this ProjectItems items, Func<ProjectItem, bool> projection)
		{
			// Using recursion - find file only by name (without path)

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
			return item.IsFile() && Core.ConfigFile.Equals(oldName, StringComparison.OrdinalIgnoreCase);
		}

		public static bool IsConfiguration(this ProjectItem item)
		{
			return item.IsFile(Core.ConfigFile);
		}
	}
}