
WHAT IS IT
----------
Visual Studio Add-in for automatically running T4 Text Template (TT) files when one of the registered triggers are hit.



HOW
---------
Install the Add-in (remember to enable it in (Tools->Add-in Manager...)
Drop a AutoTT.config file anywhere in the project with the configuration.



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
For intellisense when writting the AutoTT.config configuration file add the xsd schema found in the \Dynamo.AutoTT\Configuration\ folder to Visual Studio either via the XML->Schemas option (visible when a xml file is open)
or by putting the xsd schema file in the Visual studio xsd schema folder which could normally be found at %InstallRoot%\Xml\Schemas 
