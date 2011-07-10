using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using EnvDTE;
using VSLangProj;

/* 
POSSIBLE IMPROVEMENTS
---------------------

- Handle multiple saves (just after eachother) - only way seem to be a delay and a queue.
	delay for 0.5-1 second and add all event actions to queue - then lock the queue when delay is over and if one of the files triggers a template move on to next template so it isnt executed twice
	Bascically just accumlates all saves and make sure that template is only executed 1 time

- Add Async Thread/Task to TestTriggers or ExecuteTemplate - for better performance ?
	http://blogs.msdn.com/b/csharpfaq/archive/2010/06/01/parallel-programming-in-net-framework-4-getting-started.aspx
	http://msdn.microsoft.com/en-us/library/system.threading.tasks.task.aspx

- Could use Solution.FindProjectItem() instead of my own GetItem (the one without recursion) - but would require to prepend either full path or project path ("c:\....\" or "ProjectName\" + Template)

- How to Write tests ?
*/

namespace Dynamo.AutoTT
{
	internal class Core
	{
		#region Fields
		public const string ConfigFile = "AutoTT.config";
		private readonly Dictionary<Project, Configuration> _configurations = new Dictionary<Project, Configuration>();
		#endregion

		#region Methods
		public bool TryGetConfiguration(Project project, out Configuration configuration)
		{
			if (project == null)
				throw new ArgumentNullException("project");

			return _configurations.TryGetValue(project, out configuration);
		}

		public Configuration Load(Project project)
		{
			// TryXxx pattern instead ?

			if (project == null)
				throw new ArgumentNullException("project");

			// Find Configuration File (search whole project using recursion - so it can be placed anywhere)
			var configItem = project.GetItem(x => x.IsConfiguration());

			if (configItem != null)
				return LoadConfiguration(configItem);

			return null;
		}

		public void Unload(Project project)
		{
			if (project == null)
				throw new ArgumentNullException("project");

			_configurations.Remove(project);
		}

		public Configuration LoadConfiguration(ProjectItem configItem)
		{
			// TryXxx Pattern instead ?

			if (configItem == null)
				throw new ArgumentNullException("configItem");

			var file = configItem.FileNames[0];

			using (var reader = new StreamReader(file))
			{
				try
				{
					var serializer = new XmlSerializer(typeof(Configuration));
					Configuration config = (Configuration)serializer.Deserialize(reader);

					_configurations.Add(configItem.ContainingProject, config);

					return config;
				}
				catch (Exception)
				{
					MessageBox.Show("AutoTT reporting: Invalid configuration - " + file);
					
				}
			}

			return null;
		}

		public void TestTriggers(ProjectItem item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			// Only allow physical files to test triggers ? or just dont allow Constants.vsProjectItemKindSolutionItems ?
			if (item.Kind == Constants.vsProjectItemKindPhysicalFile)
				TestTriggers(item.ContainingProject, item.FileNames[0]);
		}

		public void TestTriggers(Project project, string file)
		{
			if (project == null)
				throw new ArgumentNullException("project");
			if (file == null)
				throw new ArgumentNullException("file");

			Configuration config;
			if (_configurations.TryGetValue(project, out config))
			{
				var relativePath = Helper.GetRelativePath(project, file);

				// Run through every template and test their triggers to see if there is a match
				foreach (var template in config.Templates)
				{
					var execute = template.Trigger.Any(trigger => trigger.IsMatch(relativePath));
		
					if (execute)
						ExecuteTemplate(project, template.Name);
				}
			}
		}

		public void ExecuteAllTemplates(Project project, Configuration configuration, bool ifOnBuild = false)
		{
			if (project == null)
				throw new ArgumentNullException("project");
			if (configuration == null)
				throw new ArgumentNullException("configuration");
			
			foreach (var template in configuration.Templates)
			{
				if (!ifOnBuild || template.OnBuild)
					ExecuteTemplate(project, template.Name);
			}
		}

		private void ExecuteTemplate(Project project, string template)
		{
			// Try to find it so it can be run
			var templateItem = project.GetItem(template);

			if (templateItem == null)
				MessageBox.Show("AutoTT reporting: Could not find template - " + template);
			else
				ExecuteTemplate(templateItem);
		}

		private void ExecuteTemplate(ProjectItem templateItem)
		{
			// Check if CustomTool is associated 
			if (((string)templateItem.Properties.Item("CustomTool").Value) != "TextTemplatingFileGenerator")
			{
				MessageBox.Show("AutoTT reporting: Could not execute the Text Template.\nThe TextTemplatingFileGenerator CustomTool is not associated with the file " + templateItem.FileNames[0]);
			}
			else
			{
				var vsProjectItem = (VSProjectItem)templateItem.Object;
				vsProjectItem.RunCustomTool();
			}
		}
		#endregion
	}
}
