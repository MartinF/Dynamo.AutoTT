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
		private SolutionEvents _solutionEvents;
		private ProjectsEvents _projectsEvents;
		private ProjectItemsEvents _projectItemsEvents;
		private DocumentEvents _documentEvents;
		private BuildEvents _buildEvents;
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
			_core = new Core();

			_applicationObject = (DTE2)application;
			_addInInstance = (AddIn)addInInst;

			_events = (Events2)_applicationObject.Events;
			_solutionEvents = _events.SolutionEvents;
			_projectsEvents = _events.ProjectsEvents;
			_projectItemsEvents = _events.ProjectItemsEvents;
			_documentEvents = _events.DocumentEvents;
			_buildEvents = _events.BuildEvents;

			// Add event handlers
			_projectsEvents.ItemAdded += new _dispProjectsEvents_ItemAddedEventHandler(ProjectsEvents_ItemAdded);
			_projectsEvents.ItemRemoved += new _dispProjectsEvents_ItemRemovedEventHandler(ProjectsEvents_ItemRemoved);

			_documentEvents.DocumentSaved += new _dispDocumentEvents_DocumentSavedEventHandler(DocumentEvents_DocumentSaved);



			//_solutionEvents.Opened += new _dispSolutionEvents_OpenedEventHandler(SolutionEvents_Opened);
			//_solutionEvents.ProjectAdded += new _dispSolutionEvents_ProjectAddedEventHandler(SolutionEvents_ProjectAdded);
			//_solutionEvents.ProjectRenamed += new _dispSolutionEvents_ProjectRenamedEventHandler(SolutionEvents_ProjectRenamed);
			//_solutionEvents.ProjectRemoved += new _dispSolutionEvents_ProjectRemovedEventHandler(SolutionEvents_ProjectRemoved);

			//_projectsEvents.ItemRenamed += new _dispProjectsEvents_ItemRenamedEventHandler(ProjectsEvents_ItemRenamed);

			//_projectItemsEvents.ItemAdded += new _dispProjectItemsEvents_ItemAddedEventHandler(ProjectItemsEvents_ItemAdded);
			//_projectItemsEvents.ItemRenamed += new _dispProjectItemsEvents_ItemRenamedEventHandler(ProjectItemsEvents_ItemRenamed);
			//_projectItemsEvents.ItemRemoved += new _dispProjectItemsEvents_ItemRemovedEventHandler(ProjectItemsEvents_ItemRemoved);


			// OnBuildBegin / OnBuildProjConfigDone ?
			//_buildEvents.OnBuildProjConfigBegin += new _dispBuildEvents_OnBuildProjConfigBeginEventHandler(BuildEvents_OnBuildProjConfigBegin);

			// Publish event ?
		}

		/// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
		/// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
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
			// Happens when project is loaded, created or added			
			// MessageBox.Show("ProjectsEvents_ItemAdded - " + project.Name);
			_core.Load(project);
		}

		void ProjectsEvents_ItemRemoved(Project project)
		{
			// MessageBox.Show("ProjectsEvents_ItemRemoved - " + project.Name);
			_core.Unload(project);
		}

		// DOCUMENT
		void DocumentEvents_DocumentSaved(Document document)
		{
			MessageBox.Show("DocumentEvents_DocumentSaved");


			// first check if the document saved is the configuration ? - load it

			// Find out if there is a configuration registered for this document
			// If there is check if the document should trigger running a TT file
		}







		// SOLUTION
		void SolutionEvents_Opened()
		{
			var solution = (Solution2)_applicationObject.Solution;
			MessageBox.Show("SolutionEvents_Opened - " + solution.FullName);
			//LoadSolution((Solution2)_applicationObject.Solution);
		}

		void SolutionEvents_ProjectAdded(Project project)
		{
			MessageBox.Show("SolutionEvents_ProjectAdded - " + project.Name);
		}

		void SolutionEvents_ProjectRenamed(Project project, string oldName)
		{
			MessageBox.Show("SolutionEvents_ProjectRenamed");
		}

		void SolutionEvents_ProjectRemoved(Project project)
		{
			MessageBox.Show("SolutionEvents_ProjectRemoved");
		}



		void ProjectsEvents_ItemRenamed(Project project, string oldProjectName)
		{
			MessageBox.Show("ProjectsEvents_ItemRenamed - new project name: " + project.Name + " --- old project name: " + oldProjectName);
			//_core.RenameProject(project, oldProjectName);
		}



		// PROJECT ITEM EVENTS
		void ProjectItemsEvents_ItemAdded(ProjectItem projectItem)
		{
			// Is it configuration or one of the files that needs to be handled ?

			MessageBox.Show("ProjectItemsEvents_ItemAdded - " + projectItem.Name);
		}

		void ProjectItemsEvents_ItemRenamed(ProjectItem projectItem, string oldName)
		{
			MessageBox.Show("ProjectItemsEvents_ItemRenamed");
		}

		void ProjectItemsEvents_ItemRemoved(ProjectItem projectItem)
		{
			MessageBox.Show("ProjectItemsEvents_ItemRemoved");
		}



		// BUILD
		void BuildEvents_OnBuildProjConfigBegin(string project, string projectConfig, string platform, string solutionConfig)
		{
			MessageBox.Show("BuildEvents_OnBuildProjConfigBegin");
		}
		#endregion

		#endregion
	}
}