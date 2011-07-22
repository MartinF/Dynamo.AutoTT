using System;
using System.IO;
using System.Linq;
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
  Or use System.IO.Directory.GetFiles() ?

- Verify that Template exists when Configuration is loaded ? 

- Write tests

- Could easily work with any custom tool, just remove check in ExecuteTemplate - Rename project.
*/

namespace Dynamo.AutoTT
{
	internal interface ICore
	{
		IFeedbackManager Feedback { get; }

		void Load(Solution solution);
		void Load(Project project);
		void Load(ProjectItem configurationItem);
		void Unload(Project project);
		void Unload(ProjectItem configurationItem);
		void TestTriggers(ProjectItem item);
		void TestTriggers(Project project, string file);
		void ExecuteBuildTemplates(Project project);
	}

	internal class Core : ICore
	{
		#region Fields
		public const string ConfigFile = "AutoTT.config";
		private readonly IIndex _index;
		#endregion

		#region Constructors
		public Core(IIndex index, IFeedbackManager feedback)
		{
			if (index == null)
				throw new ArgumentNullException("index");
			if (feedback == null)
				throw new ArgumentNullException("feedback");

			_index = index;
			Feedback = feedback;
		}
		public Core(IFeedbackManager feedback) : this(new Index(), feedback)
		{
			// Use default implementation of IIndex
		}
		#endregion

		#region Properties
		public IFeedbackManager Feedback { get; private set; }
		#endregion

		#region Methods
		public void Load(Solution solution)
		{
			if (solution == null)
				throw new ArgumentNullException("solution");

			foreach (Project project in solution)
			{
				Load(project);
			}
		}

		public void Load(Project project)
		{
			if (project == null)
				throw new ArgumentNullException("project");

			// Do not load solution project/items
			if (project.Kind == Constants.vsProjectKindSolutionItems)
				return;

			// Find Configuration File (search whole project using recursion - so it can be placed anywhere)
			var configItem = project.GetAllItems().FirstOrDefault(x => x.IsConfiguration());

			if (configItem != null)
				Load(configItem);
		}

		public void Load(ProjectItem configurationItem)
		{
			if (configurationItem == null)
				throw new ArgumentNullException("configurationItem");

			var project = configurationItem.ContainingProject;

			// Check if project already have a configuration loaded
			if (_index.Contains(project))
			{
				Feedback.Error("A configuration file is already loaded for this project - " + configurationItem.ContainingProject.Name);
			}

			var file = configurationItem.FileNames[0];

			using (var reader = new StreamReader(file))
			{
				try
				{
					var serializer = new XmlSerializer(typeof(Configuration));
					Configuration config = (Configuration)serializer.Deserialize(reader);

					_index.Add(configurationItem, config);
				}
				catch (Exception)
				{
					Feedback.Error("Invalid configuration - " + file);
				}
			}
		}

		public void Unload(Project project)
		{
			if (project == null)
				throw new ArgumentNullException("project");

			_index.Remove(project);
		}

		public void Unload(ProjectItem configurationItem)
		{
			if (configurationItem == null)
				throw new ArgumentNullException("configurationItem");

			_index.Remove(configurationItem);
		}

		public void TestTriggers(ProjectItem item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			// Only allow physical files which are part of a loaded project to test triggers ? or just dont allow Constants.vsProjectItemKindSolutionItems ?
			// Move this check to the Connect.Event ? should never do anything if the item saved is not a ProjectFile
			if (item.IsProjectFile())
				TestTriggers(item.ContainingProject, item.FileNames[0]);
		}

		public void TestTriggers(Project project, string file)
		{
			if (project == null)
				throw new ArgumentNullException("project");
			if (file == null)
				throw new ArgumentNullException("file");

			Configuration configuration;
			if (_index.TryGet(project, out configuration))
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

		public void ExecuteBuildTemplates(Project project)
		{
			if (project == null)
				throw new ArgumentNullException("project");

			// Try to get the configuration
			Configuration config;
			if (_index.TryGet(project, out config))
			{
				// Enumerate all templates
				foreach (var template in config.Templates)
				{
					// Execute if OnBuild is true
					if (template.OnBuild)
						ExecuteTemplate(project, template.Name);
				}
			}
		}

		private void ExecuteTemplate(Project project, string template)
		{
			// Try to find it so it can be run
			var templateItem = project.GetRelativeItem(template);

			// Make sure template is found
			if (templateItem == null)
			{
				Feedback.Error("Could not find template - " + template);
				return;
			}

			// Make sure correct Custom Tool is associated
			if (((string)templateItem.Properties.Item("CustomTool").Value) != "TextTemplatingFileGenerator")
			{
				Feedback.Error("Could not execute the Text Template.\nThe TextTemplatingFileGenerator CustomTool is not associated with the file " + templateItem.FileNames[0]);
				return;
			}

			// Try/catch ?
			var vsProjectItem = (VSProjectItem)templateItem.Object;
			vsProjectItem.RunCustomTool();
		}
		#endregion
	}
}
