## Sharp Template engine. Simple runtime built template system

Available as [Nuget package](https://www.nuget.org/packages/SharpTemplateEngine/)

If it would be needed to generate files in runtime, these classes can be used in conjunction with the compilation utilities to compile the templates.
The target is to have a system like the one used by ASP.NET to compile the Razor templates into class. If you need it you could use these class as Razor classes :) I will add a specific post about this in the near future!
The documentation can be found on [Kendar.org](http://kendar.org/?p=/dotnet/sharptemplate).




#### Keywords

* &lt;&#35;using&#35; name.space &#35;&gt;: add using to template builder class
* &lt;&#35;base&#35; full.base.class.name &#35;&gt;: the base class from which will inherit the template builder
* &lt;&#35;model&#35; full.model.class.name &#35;&gt;: the model containing the data to build the new class. The template builder will use it inside the code
* &lt;&#35; source code &gt;: The source code that will be copied as-is
* Everything else: Will be replaced by __Write("Everything else");__

#### Usage - Input and Output

Given a template with the following content, like into the unit tests of the sharp template project (Resources/ClassTemplate.cs)

<pre>
<#using# SharpTemplate.Test.Resources#>
<#using#SharpTemplate.Test#>
<#model# ParseCompileModel#>
using System;
using System.Collections.Generic;

namespace TestNamespace
{
	public class <# Write(model.ClassName); #>{

		public  <# Write(model.ClassName); #>()
		{
		
		}

		public List<string> <# Write(model.MethodName); #>()
		{
			var result = new List<string>();
			for(int i = 0;i<<# Write(model.UpTo.ToString()); #>;i++){
				result.Add(i.ToString());
			}
			return result;
		}
	}
}
</pre>

Given a model, with the following value

* ClassName: "ResultingClass" 
* MethodName: "MethodToInvoke" 
* UpTo: 10 

The following class will be created calling the ISharpResult class

<pre>
using System;
using System.Collections.Generic;
using SharpTemplate.Test;
using SharpTemplate.Test.Resources;

namespace TestNamespace
{
	public class ResultingClass{

		public  ResultingClass()
		{
		
		}

		public List<string> MethodToInvoke()
		{
			var result = new List<string>();
			for(int i = 0;i<10;i++){
				result.Add(i.ToString());
			}
			return result;
		}
	}
}
</pre>

#### Usage - The compilation

To create the template the compilation utils must be used, for example (see the next paragraph for the compilation utils specific stuff)

<pre>
		const string dllName = "CompileClassTemplate";
		const string resultDllName = "CompileResultingTemplate";
		var path = TestUtils.GetExecutionPath();
		var source = ResourceContentLoader.LoadText("ClassTemplate.cs", Assembly.GetExecutingAssembly());

		var pp = new SharpParser();
		var sharpClass = pp.ParseClass(source, "TestParserClass", "SharpTemplate.Test.Resources.Parsers");

		var loadedAssembly = BuildParserClass(dllName, path, sharpClass);

		var content = File.ReadAllBytes(loadedAssembly);
		var compileSimpleObjectAsm = Assembly.Load(content);
		var instance = (ISharpResult)Activator.CreateInstance(compileSimpleObjectAsm.FullName,
			"SharpTemplate.Test.Resources.Parsers.TestParserClass").Unwrap();


		var model = new ParseCompileModel("ResultingClass", "MethodToInvoke", 10);
		instance.Execute(model);
		var writtenModel = instance.Content;

		var resultingAssembly = BuildGenericClass(resultDllName, path, "TestNamespace", "ResultingClass",writtenModel);
		content = File.ReadAllBytes(resultingAssembly);
		compileSimpleObjectAsm = Assembly.Load(content);

		var instanceFinal = Activator.CreateInstance(compileSimpleObjectAsm.FullName, "TestNamespace.ResultingClass").Unwrap();
		var method = instanceFinal.GetType().GetMethod("MethodToInvoke");
		
		List<string> result = method.Invoke(instanceFinal, new object[] { }) as List<string>;
</pre>		

* First the template is loaded.
* At a certain point a SharpParser class is created with the BuildParserClass that hides the specific of the compilation utils
* Then the ISharpResult is created. It contains
	* Execute(object): The function to which the model is passed
	* Content: The result of the evaluation of the SharpTemplate when the model is applied
* Then the resulting class is compiled with the BuildGenericClass that hides the specific of the compilation utils
* Finally the method added, "MethodToInvoke" is invoked on the instance of the object.

#### Suggestion

One thing for which i use this parser is to generate files inside the Visual Studio Extension. With this i can create parser that given a certain model inpute (xml, specific class) can create a complex class using the C# syntax like i could do with cshtml and razor templates.

### Compilation Utils

This is a bunch of classes that could be useful to compile objects in runtime. All unit-tested.

#### Usage

* Define what will be the name of the dll and its temporary path.
* ResourceContentLoader.LoadText: Load the source file content. It will implement in our example the ISimpleObject interface
* var sc = new SourceCompiler(dllName, path): Create a source compiler, that will build the dll inside the given path
* sc.UseAppdomain = true: Choose if should compile in a different AppDomain
* Set (if needed) the path of the key file that will be used for signing the resulting assembly
* sc.AddFile("SharpTemplate...: Add the files to compile specifying the namespace, class name and source content.
* sc.LoadCurrentAssemblies(): Load the assembly present in this moment or add the path for the dlls that should be loaded
* var loadedAssembly = sc.Compile(2): Compile setting the maximum number of retries. Each retry will remove the files with errors from the compilation unit. In this case two retries.
* The result of the compilation will be a path for a dll. Load it (see the notes below) and store the assembly returned from Assembly.Load
* var instance = (ISimpleObject)Activator...: Instiantiate the classes with the Activator

<pre>
	const string dllName = "CompileSimpleObject";
	var path = Path.GetTempPath();
	var source = ResourceContentLoader.LoadText("SimpleObject.cs", Assembly.GetExecutingAssembly());
	
	var sc = new SourceCompiler(dllName, path);
	sc.UseAppdomain = true;
	sc.AddFile("SharpTemplate.Test.Resources.Compilers", "SimpleObject", source);
	sc.LoadCurrentAssemblies();
	var loadedAssemblyPath = sc.Compile(2);

	var content = File.ReadAllBytes(loadedAssemblyPath);
	var compileSimpleObjectAsm = Assembly.Load(content);
	var instance = (ISimpleObject)Activator.CreateInstance(compileSimpleObjectAsm.FullName, 
		"SharpTemplate.Test.Resources.Compilers.SimpleObject").Unwrap();
	
</pre>
			
#### Notes

* Inside the AppDomainCompiler class is present a call to AppDomain.CurrentDomain.AssemblyResolve. It is needed since some assembly may not had been loaded. Thus we must give a way to find them.
* Creating assemblies in memory could be a mess. Since they are not always loadable directly. A good way (the one i use)
	* Create a temporary dll
	* Load its content with File.ReadAllBytes
	* Load the resulting byte array with Assembly.Load
	* Delete the temporary dll
	
<pre>
	var content = File.ReadAllBytes(loadedAssemblyPath);
	var compileSimpleObjectAsm = Assembly.Load(content);
	File.Delete(loadedAssemblyPath);
</pre>

* Compiling new dlls is an heavy operation. This should be done on application startup only
* Compiling requires lots of ancillary dlls to be loaded. Compiling in the same AppDomain of the application can lead to wasting space. A good solution is to compile everything in a different AppDomain (created just for this purpose) load the dll file and unload the compilation AppDomain
* Remember that is not possible to unload a dll from an AppDomain. When should change a class, you should reload THE WHOLE AppDomain (and the application)
* All types that cross AppDomain buondaries (and all the objects they reference) should be Serializable or implement MarshalByRefObject. That is the reason of using mostly simple types in this project when working with AppDomains. This is automatically handled by the utility.
* A really useful thing to do to avoid too much reflection could be creating an interface that all the generated classes implements. This to keep easy the invocation of the various methods. Like what happens in the example with the ISimpleObject interface

#### Logging

Given the temporary path in wich the library will write the dll

* myCompiled.dll
* 123456789: The file time in which the compilation had been made
	* ClassName1.cs: The source for all compiled classes
	* ...
	* ClassName2.cs	
	* compilation.0.log: The error log for each of the compilation tried by the library

#### StillTodo

* Add a "log if debug" flag for log files to AppDomainCompiler and SourceCompiler
* Inside the log show what files had been discarded

### Licensing

This is distributed with NO WARRANTY and under the terms of the GNU GPL and PHP licenses. If you use it a notice or some credit would be nice.

You can get a copy of the GNU GPL at http://www.gnu.org/copyleft/gpl.html

### Download

See [kendar.org](http://www.kendar.org/ "Kendar.org") for the latest changes.