using System;
using System.Collections.Generic;
using EnvDTE;

namespace Dynamo.AutoTT
{
	internal interface IIndex
	{
		void Add(ProjectItem configurationItem, Configuration configuration);
		void Remove(Project project);
		void Remove(ProjectItem configurationItem);
		bool TryGet(Project project, out Configuration configuration);
		bool TryGet(Project project, out Entry entry);
		bool TryGet(ProjectItem configurationItem, out Configuration configuration);
		bool Contains(Project project);
		bool Contains(ProjectItem configurationItem);
	}

	internal class Index : IIndex
	{
		#region Fields
		private readonly Dictionary<Project, Entry> _configurations = new Dictionary<Project, Entry>();
		#endregion

		#region Methods
		public void Add(ProjectItem configurationItem, Configuration configuration)
		{
			if (configurationItem == null)
				throw new ArgumentNullException("configurationItem");
			if (configuration == null)
				throw new ArgumentNullException("configuration");

			var project = configurationItem.ContainingProject;
			var entry = new Entry(configurationItem, configuration);

			_configurations.Add(project, entry);
		}

		public void Remove(Project project)
		{
			// Always entry with the project key

			if (project == null)
				throw new ArgumentNullException("project");

			_configurations.Remove(project);
		}

		public void Remove(ProjectItem configurationItem)
		{
			// Removes entry only if the configurationItem is the same as one already stored for the project

			if (configurationItem == null)
				throw new ArgumentNullException("configurationItem");

			var project = configurationItem.ContainingProject;

			Entry entry;
			if (_configurations.TryGetValue(project, out entry))
			{
				if (entry.ConfigurationItem == configurationItem)
					_configurations.Remove(project);
			}
		}

		public bool TryGet(Project project, out Configuration configuration)
		{
			if (project == null)
				throw new ArgumentNullException("project");

			Entry entry;
			if (_configurations.TryGetValue(project, out entry))
			{
				configuration = entry.Configuration;
				return true;
			}

			configuration = null;
			return false;
		}

		public bool TryGet(Project project, out Entry entry)
		{
			if (project == null)
				throw new ArgumentNullException("project");

			return _configurations.TryGetValue(project, out entry);
		}

		public bool TryGet(ProjectItem configurationItem, out Configuration configuration)
		{
			if (configurationItem == null)
				throw new ArgumentNullException("configurationItem");

			var project = configurationItem.ContainingProject;

			Entry entry;
			if (_configurations.TryGetValue(project, out entry))
			{
				if (entry.ConfigurationItem == configurationItem)
				{
					configuration = entry.Configuration;
					return true;
				}
			}

			configuration = null;
			return false;
		}

		public bool Contains(Project project)
		{
			if (project == null)
				throw new ArgumentNullException("project");

			return _configurations.ContainsKey(project);
		}

		public bool Contains(ProjectItem configurationItem)
		{
			if (configurationItem == null)
				throw new ArgumentNullException("configurationItem");

			var project = configurationItem.ContainingProject;
			
			Entry entry;
			if (_configurations.TryGetValue(project, out entry))
			{
				return (entry.ConfigurationItem == configurationItem);
			}

			return false;
		}
		#endregion
	}
}
