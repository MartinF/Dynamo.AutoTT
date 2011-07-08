using System;
using System.Windows.Forms;
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
		private Core _core;
		private DTE2 _applicationObject;
		private AddIn _addInInstance;
		private Events2 _events;
		private ProjectsEvents _projectsEvents;
		private ProjectItemsEvents _projectItemsEvents;
		private DocumentEvents _documentEvents;
		#endregion

		#region Constructors
		/// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
		public Connect()
		{
		}
		#endregion

		#region Methods

		#region VS Hooks
		/// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
		/// <param term='application'>Root object of the host application.</param>
		/// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
		/// <param term='addInInst'>Object representing this Add-in.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{
			_applicationObject = (DTE2)application;
			_addInInstance = (AddIn)addInInst;

			// Init core
			_core = new Core();

			// Setup events
			_events = (Events2)_applicationObject.Events;
			_projectsEvents = _events.ProjectsEvents;
			_projectItemsEvents = _events.ProjectItemsEvents;
			_documentEvents = _events.DocumentEvents;

			// Add event handlers
			_projectsEvents.ItemAdded += new _dispProjectsEvents_ItemAddedEventHandler(ProjectsEvents_ItemAdded);
			_projectsEvents.ItemRemoved += new _dispProjectsEvents_ItemRemovedEventHandler(ProjectsEvents_ItemRemoved);

			_projectItemsEvents.ItemAdded += new _dispProjectItemsEvents_ItemAddedEventHandler(ProjectItemsEvents_ItemAdded);
			_projectItemsEvents.ItemRemoved += new _dispProjectItemsEvents_ItemRemovedEventHandler(ProjectItemsEvents_ItemRemoved);
			_projectItemsEvents.ItemRenamed += new _dispProjectItemsEvents_ItemRenamedEventHandler(ProjectItemsEvents_ItemRenamed);

			_documentEvents.DocumentSaved += new _dispDocumentEvents_DocumentSavedEventHandler(DocumentEvents_DocumentSaved);



			// Add BUILD attribute to XML on template tag ? - default should be false but should be able to trigger running it before build

			// What about Before Publish ?



			// If Connected when Solution is already loaded try to load projects
			if (_applicationObject.Solution != null)
			{
				foreach (Project project in _applicationObject.Solution)
				{
					 _core.Load(project);		// HAPPENS TO EARLY ??????????????????????????????

					// Okay to load here as long as it isnt executing ? Or is it not possible to Load the files here ?

					// Create thread that executes in 2-3-4-5 seconds ?
				}
			}
		}

		/// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
		/// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
			_projectsEvents.ItemAdded -= ProjectsEvents_ItemAdded;
			_projectsEvents.ItemRemoved -= ProjectsEvents_ItemRemoved;

			_projectItemsEvents.ItemAdded -= ProjectItemsEvents_ItemAdded;
			_projectItemsEvents.ItemRemoved -= ProjectItemsEvents_ItemRemoved;
			_projectItemsEvents.ItemRenamed -= ProjectItemsEvents_ItemRenamed;

			_documentEvents.DocumentSaved -= DocumentEvents_DocumentSaved;

			// Build

			// Core ?
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
			var test = 1;

			// Happens only if the addin is added at start up ...
			// Run Loading of solution here ? 

			// Wont be executed if it is enabled after it is started - which is the problem !

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
			_core.Load(project);
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
				_core.LoadConfigurationAndExecuteAllTemplates(projectItem);
			}
			else
			{
				// Solution items shouldnt trigger
				// Could check if Object, Properties or ProjectItems is null instead?
				if (projectItem.Kind == Constants.vsProjectItemKindSolutionItems)
					return;

				_core.TestTriggers(projectItem.ContainingProject, projectItem.FileNames[0]);
			}
		}

		void ProjectItemsEvents_ItemRemoved(ProjectItem projectItem)
		{
			if (projectItem.IsConfiguration())
			{
				_core.Unload(projectItem.ContainingProject);
			}
			else
			{
				_core.TestTriggers(projectItem.ContainingProject, projectItem.FileNames[0]);
			}
		}

		void ProjectItemsEvents_ItemRenamed(ProjectItem projectItem, string oldName)
		{
			if (projectItem.IsConfiguration())
			{
				// If renamed to configuration file
				_core.LoadConfigurationAndExecuteAllTemplates(projectItem);
			}
			else if (projectItem.WasConfiguration(oldName))
			{	
				// renamed from configuration file to something else
				_core.Unload(projectItem.ContainingProject);
			}
			else
			{
				// Check if new or old name triggers
				_core.TestTriggers(projectItem.ContainingProject, projectItem.FileNames[0]);
				//_core.TestTriggers(projectItem.ContainingProject, oldName); // need to append projectItem Directory/Path to oldName !?!?? *******************
			}
		}

		// DOCUMENT

		void DocumentEvents_DocumentSaved(Document document)
		{
			if (document.ProjectItem.IsConfiguration())
			{
				// Unload and Load again
				_core.Unload(document.ProjectItem.ContainingProject);
				_core.LoadConfigurationAndExecuteAllTemplates(document.ProjectItem);
			}
			else
			{
				// Check if file hits any triggers
				_core.TestTriggers(document.ProjectItem.ContainingProject, document.FullName);
			}
		}
		#endregion

		#endregion
	}
}