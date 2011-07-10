using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
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

// Make possible to Execute custom tool using the TextTransform.exe instead so it works without the file/item being part of the project

// Handle multiple saves (just after eachother) - only way seem to be a delay and a queue.
	// delay for 0.5-1 second and add all to queue - the lock/copy the queue when delay is over and if one of the files triggers a template move on to next template so it isnt executed twice
	// Bascically just accumlates all saves and make sure that template is only executed 1 time

// Add Async Thread/Task to TestTriggers or ExecuteTemplate - for better performance and no locking ?
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



// Could cache Template ProjectItem either when loading the configuration or here when found the first time
// It is unloading when renaming, but will configuration be reset so cache is also reset ?




// TEST IsMatch - works when executing files from different folders and only hitting when wanted - regex pattern is used correctly !


// Installer T4 Editor eller Devart T4 editor instead of Tangible ?

// Not sure about the xsd/xml Configuration format / naming ?


// Rename from AutoTT to AutoT4 ? 


// Should all Text Templates be executed when a project is added/loaded to make sure everything is up-to-date ? - Big question


// Most can actually be moved out ?


// Rename AutoTT.cs to Configuration !? and put in root ? together with other files ?
// Rename AutoTT.xsd to AutoTT.config.xsd ? 
// Rename AutoTT.xml to AutoTT.config


// Questions/Concerns that need to be resolved:
// - When should it run the text template ? only when one of the triggers is hit, or also when AutoTT.config file is saved ? and when AutoTT.config is loaded at startup ?
// - Should TextTemplate be automatically executed when the AutoTT.config file is either loaded, added or saved ?



// Clean up code - some of it probably shouldnt be part of the core but could be moved to helper/extensions

// Laod and LoadConfiguration as TryLoad ?

namespace Dynamo.AutoTT
{
	internal class Core
	{
		#region Fields
		public const string ConfigFile = "AutoTT.config";	// put where ?	
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
			// Could Solution.FindProjectItem("\project\AutoTT.config") be used instead ?
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

			if (File.Exists(file))
			{
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
						return null;
					}
				}
			}

			MessageBox.Show("AutoTT reporting: Error when trying to load the configuration - " + file);
			return null;	// should never happen - throw exception ?
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

			// Try to get configuration for the project
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


		// Test triggers could also jsut take in ProjectItem ? 

		// Why not just take in Project and automatically look up configuration ? 
		// Or just keep in ExecuteTemplate(ProjectItem) ? 
		

		public void ExecuteAllTemplates(Project project, Configuration configuration, bool ifOnBuild = false)
		{
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
