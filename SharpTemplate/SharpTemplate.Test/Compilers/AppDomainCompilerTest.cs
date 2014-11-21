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
using SharpTemplate.Utils;

namespace SharpTemplate.Test.Compilers
{
	[TestClass]
	public class AppDomainCompilerTest
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
		public void CompileSimpleObject()
		{
			const string dllName = "CompileSimpleObjectAd";
			var path = TestUtils.GetExecutionPath();
			var source = ResourceContentLoader.LoadText("SimpleObjectAd.cs", Assembly.GetExecutingAssembly());

			var sc = new SourceCompiler(dllName, path);
			sc.UseAppdomain = true;

			sc.AddFile("SharpTemplate.Test.Resources.Compilers", "SimpleObjectAd", source);
			sc.LoadCurrentAssemblies();
			var loadedAssembly = sc.Compile();

			Assert.IsNotNull(loadedAssembly);
			Assert.IsFalse(sc.HasErrors);

			AddFile(loadedAssembly);

			var content = File.ReadAllBytes(loadedAssembly);
			var compileSimpleObjectAsm = Assembly.Load(content);
			var instance = (ILoadedClass)Activator.CreateInstance(compileSimpleObjectAsm.FullName, "SharpTemplate.Test.Resources.Compilers.SimpleObjectAd").Unwrap();
			Assert.IsNotNull(instance);

			var result = instance.GetAssemblyName();
			Assert.AreEqual(dllName, result);
		}


		[TestMethod]
		public void CompileRetryIfErrors()
		{
			const string dllName = "CompileRetryIfErrorsAd";
			var path = TestUtils.GetExecutionPath();
			var sourceCorrect = ResourceContentLoader.LoadText("CorrectObjectAd.cs", Assembly.GetExecutingAssembly());
			var sourceFail = ResourceContentLoader.LoadText("FailObjectAd.cs", Assembly.GetExecutingAssembly());

			var sc = new SourceCompiler(dllName, path);
			sc.UseAppdomain = true;

			sc.AddFile("SharpTemplate.Test.Resources.Compilers", "CorrectObjectAd", sourceCorrect);
			sc.AddFile("SharpTemplate.Test.Resources.Compilers", "FailObjectAd", sourceFail);
			sc.LoadCurrentAssemblies();
			var loadedAssembly = sc.Compile(2);

			Assert.IsNotNull(loadedAssembly);
			Assert.IsTrue(sc.HasErrors);
			Assert.AreEqual(1, sc.Errors.Count);
			Assert.AreEqual(1, sc.Errors[0].Count);
			Assert.IsTrue(sc.Errors[0][0].Contains("SharpTemplate.Test.Compilers.ILoadedClass.GetAssemblyName"));

			AddFile(loadedAssembly);

			var content = File.ReadAllBytes(loadedAssembly);
			var compileSimpleObjectAsm = Assembly.Load(content);
			var instance = (ILoadedClass)Activator.CreateInstance(compileSimpleObjectAsm.FullName, "SharpTemplate.Test.Resources.Compilers.CorrectObjectAd").Unwrap();
			Assert.IsNotNull(instance);
			
			var result = instance.GetAssemblyName();
			Assert.AreEqual(dllName, result);
		}

		[TestMethod]
		public void CompileSignedAssembly()
		{
			const string dllName = "CompileSignedAssemblyAd";
			var path = TestUtils.GetExecutionPath();
			var keyTemp = Path.Combine(path, "TestKey.snk");
			var keyContent = ResourceContentLoader.LoadBytes("TestKey.snk", Assembly.GetExecutingAssembly());
			File.WriteAllBytes(keyTemp, keyContent);
			AddFile(keyTemp);

			var sc = new SourceCompiler(dllName, path);
			sc.UseAppdomain = true;

			sc.Key = keyTemp;
			sc.LoadCurrentAssemblies();
			var loadedAssembly = sc.Compile();

			Assert.IsNotNull(loadedAssembly);
			Assert.IsFalse(sc.HasErrors);

			AddFile(loadedAssembly);

			var content = File.ReadAllBytes(loadedAssembly);
			var compileSimpleObjectAsm = Assembly.Load(content);
			Assert.IsNotNull(compileSimpleObjectAsm);

			var fullName = compileSimpleObjectAsm.FullName;
			Assert.IsTrue(fullName.Contains("3e48d4993a05c8f5"));
		}
	}
}
