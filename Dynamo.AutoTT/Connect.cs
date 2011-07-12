using System;
using System.IO;
using Extensibility;
using EnvDTE;
using EnvDTE80;

namespace Dynamo.AutoTT
{
	/// <summary>The object for implementing an Add-in.</summary>
	/// <seealso class='IDTExtensibility2' />
	public class Connect : IDTExtensibility2
	{
		#region Fields
		private DTE2 _application;
		private AddIn _addInInstance;
		private Events2 _events;
		private ProjectsEvents _projectsEvents;
		private ProjectItemsEvents _projectItemsEvents;
		private DocumentEvents _documentEvents;
		private BuildEvents _buildEvents;

		private ICore _core;
		//private List<Action> _delayedActions;
		#endregion

		#region Constructors
		/// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
		public Connect()
		{
		}
		#endregion

		#region Methods

		protected void AttachEvents()
		{
			_projectsEvents.ItemAdded += new _dispProjectsEvents_ItemAddedEventHandler(ProjectsEvents_ItemAdded);
			_projectsEvents.ItemRemoved += new _dispProjectsEvents_ItemRemovedEventHandler(ProjectsEvents_ItemRemoved);

			_projectItemsEvents.ItemAdded += new _dispProjectItemsEvents_ItemAddedEventHandler(ProjectItemsEvents_ItemAdded);
			_projectItemsEvents.ItemRemoved += new _dispProjectItemsEvents_ItemRemovedEventHandler(ProjectItemsEvents_ItemRemoved);
			_projectItemsEvents.ItemRenamed += new _dispProjectItemsEvents_ItemRenamedEventHandler(ProjectItemsEvents_ItemRenamed);

			_documentEvents.DocumentSaved += new _dispDocumentEvents_DocumentSavedEventHandler(DocumentEvents_DocumentSaved);

			_buildEvents.OnBuildProjConfigBegin += new _dispBuildEvents_OnBuildProjConfigBeginEventHandler(BuildEvents_OnBuildProjConfigBegin);
		}

		protected void DetachEvents()
		{
			_projectsEvents.ItemAdded -= ProjectsEvents_ItemAdded;
			_projectsEvents.ItemRemoved -= ProjectsEvents_ItemRemoved;

			_projectItemsEvents.ItemAdded -= ProjectItemsEvents_ItemAdded;
			_projectItemsEvents.ItemRemoved -= ProjectItemsEvents_ItemRemoved;
			_projectItemsEvents.ItemRenamed -= ProjectItemsEvents_ItemRenamed;

			_documentEvents.DocumentSaved -= DocumentEvents_DocumentSaved;

			_buildEvents.OnBuildProjConfigBegin -= BuildEvents_OnBuildProjConfigBegin;

			_core = null;
			//_delayedActions = null;
		}

		#region VS Hooks
		/// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
		/// <param term='application'>Root object of the host application.</param>
		/// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
		/// <param term='addInInst'>Object representing this Add-in.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{
			_application = (DTE2)application;
			_addInInstance = (AddIn)addInInst;

			// Core
			var index = new Index();
			var output = new OutputManager(_application);
			_core = new Core(index, output);
			// Init delayed actions
			//_delayedActions = new List<Action>();

			// Events
			_events = (Events2)_application.Events;
			_projectsEvents = _events.ProjectsEvents;
			_projectItemsEvents = _events.ProjectItemsEvents;
			_documentEvents = _events.DocumentEvents;
			_buildEvents = _events.BuildEvents;
			
			AttachEvents();



			// If being connected when Solution is already loaded try to load projects
			if (_application.Solution != null)
			{
				// Make part of the core ? 

				foreach (Project project in _application.Solution)
				{
					var config = _core.Load(project);		
					
					// Uncomment if all templates should be executed when addin is connected
					//if (config != null)
					//    _core.ExecuteAllTemplates(project, config);
				}
			}
		}

		/// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
		/// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
			DetachEvents();
		}

		/// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />		
		public void OnAddInsUpdate(ref Array custom)
		{
		}

		/// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref Array custom)
		{
			// OnStartupComplete is only hit if the Addin is started when Visual Studio starts.
			// Also it will only run after the ProjectEvents_itemAdded event if solution is being loaded/opened as part of the process of opening/starting Visual Studio
			// If Solution is opened after Visual Studio have been opened it will never hit it.

			//foreach (var action in _delayedActions)
			//{
			//    action();
			//}
		}
		
		/// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref Array custom)
		{
		}
		#endregion

		#region Eventhandlers
		
		// PROJECT

		void ProjectsEvents_ItemAdded(Project project)
		{
			var config = _core.Load(project);

			// Uncomment here and in OnStartupComplete if all templates should be executed when project is added/opened - only works if opened when vs is also being opened.
			//if (config != null)
			//{
			//    _delayedActions.Add(() => _core.ExecuteAllTemplates(project, config));
			//}

			// PROBLEM
			// -------
			// Cannot execute all templates here when event/project is not finished loading (T4MVC isnt working - will throw exception).
			
			// How to work around this ?
			// Is there some event that runs later, so we could just create a delegate/action here and in the event execute it etc ?

			// Currently delayed actions are used and executed in the OnStartupComplete event
			// But there is a clear limiation by doing it this way as it will only be run after the ProjectsEvents_ItemAdded event,
			// if the solution is being opened in the same process as when Visual Studio is opening - else it will never happen and Templates wont be executed.
			
			// How can all templates be executed when a project is loaded after Visual Studio have already started ?

			// Maybe dont execute Temlates at all when Project is being added ? 
			// Only reason for doing it is to make sure that the Templates are in sync and up-to-date. But maybe you wouldnt always want that ?
		}

		void ProjectsEvents_ItemRemoved(Project project)
		{
			_core.Unload(project);
		}

		// PROJECT ITEMS

		void ProjectItemsEvents_ItemAdded(ProjectItem projectItem)
		{
			if (projectItem.IsConfiguration())
			{
				_core.Load(projectItem);
				//_core.LoadConfigurationAndExecuteAllTemplates(projectItem);
			}
			else
			{
				_core.TestTriggers(projectItem);
			}
		}

		void ProjectItemsEvents_ItemRemoved(ProjectItem projectItem)
		{
			if (projectItem.IsConfiguration())
			{
				_core.Unload(projectItem);
			}
			else
			{
				_core.TestTriggers(projectItem);
			}
		}

		void ProjectItemsEvents_ItemRenamed(ProjectItem projectItem, string oldName)
		{
			if (projectItem.IsConfiguration())
			{
				// If renamed to configuration file
				_core.Load(projectItem);
				//_core.LoadConfigurationAndExecuteAllTemplates(projectItem);
			}
			else if (projectItem.WasConfiguration(oldName))
			{	
				// renamed from configuration file to something else
				_core.Unload(projectItem);
			}
			else
			{
				// Check if the new or old name triggers
				_core.TestTriggers(projectItem);

				// Combine path of current with oldFilename to get the full path
				var oldFullname = Path.GetDirectoryName(projectItem.FileNames[0]) + "\\" + oldName;
				_core.TestTriggers(projectItem.ContainingProject, oldFullname);
			}
		}

		// DOCUMENT

		void DocumentEvents_DocumentSaved(Document document)
		{
			if (document.ProjectItem.IsConfiguration())
			{
				// Unload, Load again
				_core.Unload(document.ProjectItem);
				_core.Load(document.ProjectItem);
				//_core.LoadConfigurationAndExecuteAllTemplates(document.ProjectItem);
			}
			else
			{
				// Check if file hits any triggers
				_core.TestTriggers(document.ProjectItem);
			}
		}

		// BUILD
		
		void BuildEvents_OnBuildProjConfigBegin(string projectName, string projectConfig, string platform, string solutionConfig)
		{
			// Get Project
			var project = _application.Solution.Item(projectName);
			
			// Execute all Templates where onbuild is set to true
			Configuration config;
			if (_core.Index.TryGet(project, out config))
				_core.ExecuteAllTemplates(project, config, ifOnBuild: true);
		}

		#endregion

		#endregion
	}
}
