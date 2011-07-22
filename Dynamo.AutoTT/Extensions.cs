using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;

namespace Dynamo.AutoTT
{
	internal static class Extensions
	{
		public static bool IsProjectFile(this ProjectItem item)
		{
			return item.Kind == Constants.vsProjectItemKindPhysicalFile;
		}

		public static bool IsProjectFile(this ProjectItem item, string name, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
		{
			return item.IsProjectFile() && item.Name.Equals(name, comparisonType);
		}

		public static bool IsProjectFolder(this ProjectItem item)
		{
			return item.Kind == Constants.vsProjectItemKindPhysicalFolder;
		}

		public static bool IsProjectFolder(this ProjectItem item, string name, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
		{
			return item.IsProjectFolder() && item.Name.Equals(name, comparisonType);
		}

		public static ProjectItem GetFile(this ProjectItems items, string name)
		{
			return items.Cast<ProjectItem>().FirstOrDefault(item => item.IsProjectFile(name));
		}

		public static ProjectItem GetFolder(this ProjectItems items, string name)
		{
			return items.Cast<ProjectItem>().FirstOrDefault(item => item.IsProjectFolder(name));
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
			if (project.ProjectItems != null)
				return project.ProjectItems.GetAllItems();

			return Enumerable.Empty<ProjectItem>();
		}

		public static IEnumerable<ProjectItem> GetAllItems(this ProjectItems projectItems)
		{
			// Use Recursion to run through all items

			if (projectItems == null)		// Will it still continue when this is meet when inside recursion ?
				yield break;

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
			return item.IsProjectFile() && Core.ConfigFile.Equals(oldName, StringComparison.OrdinalIgnoreCase);
		}

		public static bool IsConfiguration(this ProjectItem item)
		{
			return item.IsProjectFile(Core.ConfigFile);
		}

		public static OutputWindowPane GetPane(this OutputWindowPanes panes, string name)
		{
			// Try to get if it already exists
			var pane = panes.Cast<OutputWindowPane>().FirstOrDefault(x => x.Name == name);

			if (pane == null)
			{
				// create it and select it
				pane = panes.Add(name);
				pane.Activate();
			}

			return pane;
		}
	}
}