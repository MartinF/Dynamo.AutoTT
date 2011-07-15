using System;
using System.Windows.Forms;
using EnvDTE;

// Better name: ResponseManager - Feedback, Response, UserInterface, UIFeedback, UIResponse, InterfaceFeedback, InterfaceResponse ? 

// Simple wrapper for abstraction of the feedback options to the user interface (visual studio)

namespace Dynamo.AutoTT
{
	internal interface IFeedbackManager
	{
		void Error(string text);
		void Event(string text);
	}

	internal class FeedbackManager : IFeedbackManager
	{
		#region Fields
		private readonly string _messagePrefix;
		private readonly OutputWindowPane _outputPane;
		#endregion

		#region Constructors
		public FeedbackManager(string name, OutputWindowPane outputPane)
		{
			if (name == null)
				throw new ArgumentNullException("name");
			if (outputPane == null)
				throw new ArgumentNullException("outputPane");

			_messagePrefix = name + " reporting:\n";
			_outputPane = outputPane;
		}
		#endregion

		#region Methods
		public void Error(string text)
		{
			MessageBox.Show(_messagePrefix + text);
		}
		public void Event(string text)
		{
			_outputPane.OutputString(text + Environment.NewLine);
		}
		#endregion
	}
}
