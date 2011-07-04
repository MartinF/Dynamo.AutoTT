using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Dynamo.AutoTT.Configuration;
using EnvDTE;

// Remove unused VsLandProj references

// List of configurations
// When project is added search for config file
// When project is removed remove config file
// When Project is Saved, or Build check if there is any config for the project if there is , run it

namespace Dynamo.AutoTT
{
	public class Core
	{
		#region Fields
		private const string _configFile = "AutoTT.config";		// ignore case ?
		private readonly Dictionary<Project, configuration> _configurations = new Dictionary<Project, configuration>();
		#endregion

		#region Methods
		public void Load(Project project)
		{
			ProjectItem projectItem;
			if (TryGetProjectItem(project.ProjectItems, IsConfigurationFile, out projectItem))
			{
				// Configuration projectItem was found - Load it
				Load(projectItem);
			}
		}

		public void Unload(Project project)
		{
			_configurations.Remove(project);
		}

		private void Load(ProjectItem item)
		{
			var fullFileName = item.FileNames[0];

			configuration config;
			if (ConfigHelper.TryLoadConfig(fullFileName, out config))
			{
				_configurations.Add(item.ContainingProject, config);
			}
		}

		public bool IsConfigurationFile(string filename)
		{
			return _configFile.Equals(filename, StringComparison.OrdinalIgnoreCase);
		}

		private bool TryGetProjectItem(ProjectItems projectItems, Func<string, bool> projection, out ProjectItem projectItem)
		{
			// Is there a better way of doing this instead of recursively enumerate every project item ?
			// Would make more sense to always check root first before beginning recursion.

			// If folder is called AutoTT.config it will actually be returned ? any way of checking the type of the projectItem ?

			foreach (ProjectItem item in projectItems)
			{
				if (projection(item.Name))
				{
					projectItem = item;
					return true;
				}
				
				if (item.ProjectItems != null && TryGetProjectItem(item.ProjectItems, projection, out projectItem))
				{
					return true;
				}
			}

			projectItem = null;
			return false;
		}
		#endregion
	}
}
