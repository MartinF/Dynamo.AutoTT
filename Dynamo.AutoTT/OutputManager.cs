using System;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;

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
		private readonly string _name;

		public OutputManager(DTE2 application, string name)
		{
			if (application == null)
				throw new ArgumentNullException("application");
			if (name == null)
				throw new ArgumentNullException("name");

			_application = application;
			_name = name;

			// Get output window pane
			_outputPane = _application.ToolWindows.OutputWindow.GetOutputWindow(_name);
		}

		public void Error(string text)
		{
			MessageBox.Show(_name + " reporting:\n" + text);
		}

		public void Event(string text)
		{
			_outputPane.OutputStringLine(text);
			_outputPane.Activate(); // always select it when writing to it ?
		}
	}
}
