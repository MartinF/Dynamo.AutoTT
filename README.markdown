
INTELLISENSE
------------
For intellisense when writting the configuration file add the xsd schema found in the \Dynamo.AutoTT\Configuration\ folder to Visual Studio either via the XML->Schemas option (visible when a xml file is open)
or by putting the xsd schema file in the Visual studio xsd schema folder which could normally be found at %InstallRoot%\Xml\Schemas 


BUILD
-----
Use the TextTransform Utility or MSBuild integration if you want to run your template as part of a build process. - http://msdn.microsoft.com/en-us/library/bb126245.aspx
I will add a Console application which can read the configuration file and execute the Text Templates when i get the time - or help me and make it right now ?