using System;
using EnvDTE;

// Entry vs Record ? 

namespace Dynamo.AutoTT
{
	internal class Entry
	{
		public Entry(ProjectItem configurationItem, Configuration configuration)
		{
			if (configurationItem == null)
				throw new ArgumentNullException("configurationItem");
			if (configuration == null)
				throw new ArgumentNullException("configuration");

			ConfigurationItem = configurationItem;
			Configuration = configuration;
		}

		public ProjectItem ConfigurationItem { get; private set; }
		public Configuration Configuration { get; private set; }
	}
}
