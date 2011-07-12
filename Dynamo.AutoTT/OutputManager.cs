using System;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;

// Always append the AutoTT reporting: (maybe even use the application.name reporting: ?

namespace Dynamo.AutoTT
{
	internal interface IOutputManager
	{
		void Error(string text);
		void Event(string text);
	}

	internal class OutputManager : IOutputManager
	{
		private readonly DTE2 _application;
		private readonly OutputWindowPane _outputPane;

		public OutputManager(DTE2 application)
		{
			if (application == null)
				throw new ArgumentNullException("application");
			
			_application = application;

			// Init output window
			_outputPane = _application.ToolWindows.OutputWindow.GetOutputWindow(_application.Name);
		}

		public void Error(string text)
		{
			MessageBox.Show(text);
		}

		public void Event(string text)
		{
			_outputPane.OutputStringLine(text);
			_outputPane.Activate(); // always select it when writing to it ?
		}
	}
}
