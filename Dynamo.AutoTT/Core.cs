using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using EnvDTE;
using VSLangProj;

/* 
POSSIBLE IMPROVEMENTS
---------------------

- Handle multiple saves (just after eachother) 
	Create thread/task than runs in 0.5-1 second, and store it on the template configuration etc.
	Just check if there is a task registered already, check if it is started, if it is not either just return or cancel the task/thread and start a new one. 
	If it is started, wait or put the new one in the queue to be executed when other is finished, or just set the new on to start in 0.5-1 seconds
	Bascically just accumlates all saves and make sure that template is only executed 1 time
	
- Use Async Thread/Task ? - Task.Factory.StartNew(() => {}); - ... but it doesnt seem to make any difference at all, GUI is still blocking.	
	
- Could use Solution.FindProjectItem() instead of my own GetRelativeItem - but would require to prepend either full path or project path ("c:\....\" or "ProjectName\" + Template)

- Write tests
*/

namespace Dynamo.AutoTT
{
	internal class Core
	{
		#region Fields
		public const string ConfigFile = "AutoTT.config";
		#endregion

		#region Constructors
		public Core()
		{
			Index = new Index();
		}
		#endregion

		#region Properties
		public Index Index { get; private set; }	
		#endregion

		#region Methods

		public Configuration Load(Project project)
		{
			if (project == null)
				throw new ArgumentNullException("project");

			// Find Configuration File (search whole project using recursion - so it can be placed anywhere)
			var configItem = project.GetAllItems().FirstOrDefault(x => x.IsConfiguration());

			if (configItem != null)
				return Load(configItem);

			return null;
		}

		public Configuration Load(ProjectItem configurationItem)
		{
			if (configurationItem == null)
				throw new ArgumentNullException("configurationItem");

			var project = configurationItem.ContainingProject;

			// Check if project already have a configuration loaded
			if (Index.Contains(project))
			{
				MessageBox.Show("AutoTT rapporting: A configuration file is already loaded for this project - " + configurationItem.ContainingProject.Name);
				return null;
			}

			var file = configurationItem.FileNames[0];

			using (var reader = new StreamReader(file))
			{
				try
				{
					var serializer = new XmlSerializer(typeof(Configuration));
					Configuration config = (Configuration)serializer.Deserialize(reader);

					Index.Add(configurationItem, config);

					return config;
				}
				catch (Exception)
				{
					MessageBox.Show("AutoTT reporting: Invalid configuration - " + file);
				}
			}

			return null;
		}

		public void Unload(Project project)
		{
			if (project == null)
				throw new ArgumentNullException("project");

			Index.Remove(project);
		}

		public void Unload(ProjectItem configurationItem)
		{
			if (configurationItem == null)
				throw new ArgumentNullException("configurationItem");

			Index.Remove(configurationItem);
		}

		public void TestTriggers(ProjectItem item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			// Only allow physical files to test triggers ? or just dont allow Constants.vsProjectItemKindSolutionItems ?
			if (item.IsFile())
				TestTriggers(item.ContainingProject, item.FileNames[0]);
		}

		public void TestTriggers(Project project, string file)
		{
			if (project == null)
				throw new ArgumentNullException("project");
			if (file == null)
				throw new ArgumentNullException("file");

			Configuration configuration;
			if (Index.TryGet(project, out configuration))
			{
				var relativePath = Helper.GetRelativePath(project, file);

				// Run through every template and test their triggers to see if there is a match
				foreach (var template in configuration.Templates)
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
			var templateItem = project.GetRelativeItem(template);

			// Make sure template is found
			if (templateItem == null)
			{
				MessageBox.Show("AutoTT reporting: Could not find template - " + template);
				return;
			}

			// Make sure correct Custom Tool is associated
			if (((string)templateItem.Properties.Item("CustomTool").Value) != "TextTemplatingFileGenerator")
			{
				MessageBox.Show("AutoTT reporting: Could not execute the Text Template.\nThe TextTemplatingFileGenerator CustomTool is not associated with the file " + templateItem.FileNames[0]);
				return;
			}

			// Try/catch ?
			var vsProjectItem = (VSProjectItem)templateItem.Object;
			vsProjectItem.RunCustomTool();
		}
		#endregion
	}
}
