using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;

namespace Dynamo.AutoTT
{
	internal static class Extensions
	{
		public static Configuration LoadConfigurationAndExecuteAllTemplates(this Core core, ProjectItem configurationItem)
		{
			var config = core.Load(configurationItem);

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

		public static ProjectItem GetRelativeItem(this Project project, string relativeFilename)
		{
			return project.ProjectItems != null ? project.ProjectItems.GetRelativeItem(relativeFilename) : null;
		}

		public static ProjectItem GetRelativeItem(this ProjectItems items, string relativeFilename)
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

		public static IEnumerable<ProjectItem> GetAllItems(this Project project)
		{
			return project.ProjectItems.GetAllItems();
		}

		public static IEnumerable<ProjectItem> GetAllItems(this ProjectItems projectItems)
		{
			// Use Recursion to run through all items

			foreach (ProjectItem projectItem in projectItems)
			{
				yield return projectItem;

				foreach (ProjectItem subItem in GetAllItems(projectItem.ProjectItems))
				{
					yield return subItem;
				}
			}
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