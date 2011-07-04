using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Dynamo.AutoTT.Configuration
{
	public static class ConfigHelper
	{
		public static bool TryLoadConfig(string filename, out configuration configuration)
		{
			if (File.Exists(filename))
			{
				using (var reader = new StreamReader(filename))
				{
					try
					{
						var serializer = new XmlSerializer(typeof(configuration));
						configuration = (configuration)serializer.Deserialize(reader);

						return true;
					}
					catch (Exception)
					{
						MessageBox.Show("Invalid configuration - " + filename);
					}				
				}
			}

			configuration = null;
			return false;
		}
	}
}