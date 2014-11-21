// ===========================================================
// Copyright (C) 2014-2015 Kendar.org
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation 
// files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, 
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software 
// is furnished to do so, subject to the following conditions:
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS 
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF 
// OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ===========================================================


using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GenericHelpers;
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
