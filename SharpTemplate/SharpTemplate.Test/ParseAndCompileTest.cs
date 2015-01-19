// ===========================================================
// Copyright (c) 2014-2015, Enrico Da Ros/kendar.org
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
