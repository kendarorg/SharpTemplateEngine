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
