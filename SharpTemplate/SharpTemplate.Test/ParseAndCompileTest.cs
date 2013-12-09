using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTemplate.Compilers;
using SharpTemplate.Parsers;
using SharpTemplate.Utils;

namespace SharpTemplate.Test
{
	[TestClass]
	public class ParseAndCompileTest
	{
		private readonly List<string> _files = new List<string>();

		private void AddFile(string path)
		{
			_files.Add(path);
		}

		[TestInitialize]
		public void Initialize()
		{
			_files.Clear();
		}
		[TestCleanup]
		public void CleanUp()
		{
			foreach (var file in _files)
			{
				if (File.Exists(file))
				{
					try
					{
						File.Delete(file);
					}
					catch
					{
						Console.WriteLine("Unable to delete " + file);
					}
				}
			}
		}


		[TestMethod]
		public void CompileSimpleTemplate()
		{
			const string dllName = "CompileClassTemplate";
			const string resultDllName = "CompileResultingTemplate";
			var path = TestUtils.GetExecutionPath();
			var source = ResourceContentLoader.LoadText("ClassTemplate.cs", Assembly.GetExecutingAssembly());

			var pp = new SharpParser();
			var sharpClass = pp.ParseClass(source, "TestParserClass", "SharpTemplate.Test.Resources.Parsers");



			var loadedAssembly = BuildParserClass(dllName, path, sharpClass);

			AddFile(loadedAssembly);

			var content = File.ReadAllBytes(loadedAssembly);
			var compileSimpleObjectAsm = Assembly.Load(content);
			var instance = (ISharpResult)Activator.CreateInstance(compileSimpleObjectAsm.FullName, "SharpTemplate.Test.Resources.Parsers.TestParserClass").Unwrap();

			Assert.IsNotNull(instance);

			var model = new ParseCompileModel("ResultingClass", "MethodToInvoke", 10);
			instance.Execute(model);
			var writtenModel = instance.Content;

			var resultingAssembly = BuildGenericClass(resultDllName, path, "TestNamespace", "ResultingClass",writtenModel);
			content = File.ReadAllBytes(resultingAssembly);
			compileSimpleObjectAsm = Assembly.Load(content);

			var instanceFinal = Activator.CreateInstance(compileSimpleObjectAsm.FullName, "TestNamespace.ResultingClass").Unwrap();
			var method = instanceFinal.GetType().GetMethod("MethodToInvoke");
			Assert.IsNotNull(method);

			var result = method.Invoke(instanceFinal, new object[] { }) as List<string>;
			Assert.IsNotNull(result);
			Assert.AreEqual(10, result.Count);
		}

		private static string BuildParserClass(string dllName, string path, SharpClass sharpClass)
		{
			var sc = new SourceCompiler(dllName, path);
			sc.UseAppdomain = true;

			sc.AddFile(sharpClass);
			sc.LoadCurrentAssemblies();
			var loadedAssembly = sc.Compile();

			Assert.IsNotNull(loadedAssembly);
			Assert.IsFalse(sc.HasErrors);
			return loadedAssembly;
		}


		private static string BuildGenericClass(string dllName, string path,string nameSpace,string className,string source)
		{
			var sc = new SourceCompiler(dllName, path);
			sc.UseAppdomain = true;

			sc.AddFile(nameSpace,className,source);
			sc.LoadCurrentAssemblies();
			var loadedAssembly = sc.Compile();

			Assert.IsNotNull(loadedAssembly);
			Assert.IsFalse(sc.HasErrors);
			return loadedAssembly;
		}
	}
}
