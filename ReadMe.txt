
INTELLISENSE
------------
For intellisense when writting the configuration add the xsd schema found in \Dynamo.AutoTT\Configuration\ to Visual Studio via the XML->Schemas option (visible when a xml file is open)
or put xsd schema file in the Visual studio xsd schema folder which could normally be found at for example C:\prog...


BUILD
-----
Use the TextTransform Utility or MSBuild integration if you want to run your template as part of a build process. - http://msdn.microsoft.com/en-us/library/bb126245.aspx
Will add a Console application which can read the configuration file and execute the Text templates some time - or help me make it right now ?