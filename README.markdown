
WHAT IS IT
----------
Visual Studio Add-in for automatically running T4 Text Template (TT) files when one of the registered triggers are hit.



HOW
---------
You can install the Add-in by downloading the source code here on GitHub and put it in the Visual Studio addins folder, or by using the installer that can be found here [Dynamo.AutoTT Installer](http://fakeyourbeauty.com/projects/Dynamo.AutoTT.msi) and on Visual Studios Extension Manager (Tools->Extension Manager).
Enable the add-in in Visual Studio (Tools->Add-in Manager).
Just create an AutoTT.config file anywhere in the project with the configuration, and it will automatically be loaded.



CONFIGURATION EXAMPLE
---------------------
	<configuration>
		<template name="T4MVC.tt" onbuild="true" >
			<trigger pattern="^Controllers\\" />
			<trigger pattern="^Content\\" />
		</template>
	</configuration>

* Note the trigger pattern is a regular expression and special characters need to be escaped



INTELLISENSE
------------
For intellisense when writting the AutoTT.config configuration file add the xsd schema found in the \Dynamo.AutoTT\Configuration\ folder (also included in the install folder if using the installer) to Visual Studio either via the XML->Schemas option (visible when a xml file is open)
or by putting the xsd schema file in the Visual studio xsd schema folder which could normally be found at %InstallRoot%\Xml\Schemas 
