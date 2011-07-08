using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using Dynamo.AutoTT.Configuration;
using EnvDTE;
using VSLangProj;

// Remove unused VsLandProj references - Vs 2010 and more can also be removed ?

// Merge methods - and move to Helper / Extensions

// Update Description ? and add to README - 
	// Automatically runs a TT (Text Templating) file when one of its triggers are saved
	// Contrary to Xxx it doenst have an infinite loop checking every second

// Add Build option (attribute) to Xsd / Xml for Template node - and add to Connect
// Also run before Publish ?

// Make Console app for build environments - AutoTT.exe /Path/AutoTT.config - Run every template using the TextTransform.exe tool

// Handle multiple saves (just after eachother) - only way seem to be a delay and a queue.
	// delay for 0.5-1 second and add all to queue - the lock/copy the queue when delay is over and if one of the files triggers a template move on to next template so it isnt executed twice
	// Bascically just accumlates all saves and make sure that template is only executed 1 time

// Add Async Thread/Task to TestTriggers or ExecuteTemplate - for better performance and no locking
// http://blogs.msdn.com/b/csharpfaq/archive/2010/06/01/parallel-programming-in-net-framework-4-getting-started.aspx
// http://msdn.microsoft.com/en-us/library/system.threading.tasks.task.aspx

// Nuget
// GitHub
// Vsi Package

// Let Ebbo and T4MVC forum know ... - and what was the name of that guy that is totally crazy about Text Templates

// Build using Release - for NuGet ! How can i install a addin via nuget ?

// TEST with T4MVC template - create configuration with correct triggers for C# MVC /Script/, /Content/, .cs^, .aspx^ ? 


// Change Load and LoadConfiguration to TryLoad and TryLoadConfiguration ?




// ALL CONNECT METHODS CALLING LOAD NEED TO CALL ExecuteAllTriggers(Project, configuratuion) !!!!!
// Or make helper method ?


// Make possible to Execute custom tool without it being part of the project (being a project item) ?

// Could cache Template ProjectItem either when loading the configuration or here when found the first time
// It is unloading when renaming, but will configuration be reset so cache is also reset ?




// TEST IsMatch - works when executing files from different folders and only hitting when wanted - regex pattern is used correctly !


namespace Dynamo.AutoTT
{
	public class Core
	{
		#region Fields
		public const string ConfigFile = "AutoTT.config";	// put where ?	
		private readonly Dictionary<Project, configuration> _configurations = new Dictionary<Project, configuration>();
		#endregion

		#region Methods
		public configuration Load(Project project)
		{
			if (project == null)
				throw new ArgumentNullException("project");

			// Find Configuration File (search whole project using recursion - so it can be placed anywhere)
			var configItem = project.GetItem(x => x.IsConfiguration());

			if (configItem != null)
				return LoadConfiguration(configItem);

			return null;
		}

		public configuration LoadConfiguration(ProjectItem configItem)
		{
			if (configItem == null)
				throw new ArgumentNullException("configItem");

			var file = configItem.FileNames[0];

			if (File.Exists(file))
			{
				using (var reader = new StreamReader(file))
				{
					try
					{
						var serializer = new XmlSerializer(typeof(configuration));
						configuration config = (configuration)serializer.Deserialize(reader);

						_configurations.Add(configItem.ContainingProject, config);

						return config;
					}
					catch (Exception)
					{
						MessageBox.Show("AutoTT reporting: Invalid configuration - " + file);
						return null;
					}				
				}
			}

			MessageBox.Show("AutoTT reporting: Error when trying to load the configuration - " + file);
			return null;	// should never happen - throw exception ?
		}

		public void Unload(Project project)
		{
			if (project == null)
				throw new ArgumentNullException("project");

			_configurations.Remove(project);
		}

		public void TestTriggers(Project project, string file)
		{
			if (project == null)
				throw new ArgumentNullException("project");

			// Try to get configuration for the project
			configuration config;
			if (_configurations.TryGetValue(project, out config))
			{
				var relativePath = Helper.GetRelativePath(project, file);

				// Run through every template and test their triggers to see if there is a match
				foreach (var template in config.Items)
				{
					var execute = template.trigger.Any(trigger => trigger.IsMatch(relativePath));
		
					if (execute)
						ExecuteTemplate(project, template.name);
				}
			}
		}

		public void ExecuteAllTemplates(Project project, configuration configuration)
		{
			foreach (var template in configuration.Items)
			{
				ExecuteTemplate(project, template.name);
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
			// Check if it is to early somehow ? Will something be different than when projet is fully loaded ?
			// Something is not present when called when project is loading compared to when loaded and triggered by file ?


			// Check if CustomTool is associated 
			if (((string)templateItem.Properties.Item("CustomTool").Value) != "TextTemplatingFileGenerator")
			{
				MessageBox.Show("AutoTT reporting: Could not execute the Text Template.\nThe TextTemplatingFileGenerator CustomTool is not associated with the file " + templateItem.FileNames[0]);
			}
			else
			{
				// Error messsage if not possible to cast ?
				var vsProjectItem = templateItem.Object as VSProjectItem;
				if (vsProjectItem != null)
					vsProjectItem.RunCustomTool();

				// Or do something like this instead ?
				//if (!item.IsOpen)
				//    item.Open();
				//item.Save();
			}
		}

		#endregion
	}
}
