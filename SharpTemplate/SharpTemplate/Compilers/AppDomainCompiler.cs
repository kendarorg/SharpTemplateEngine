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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CSharp;

namespace SharpTemplate.Compilers
{
	/// <summary>
	/// Compiler container
	/// </summary>
	public class AppDomainCompiler : MarshalByRefObject, IDisposable, IAppDomainCompiler
	{
		/// <summary>
		/// List of errors for the current compilation
		/// </summary>
		public List<string> Errors { get; private set; }

		private string _assemblyName;
		private string _assemblyPath;
		private ISourceCompilerDescriptor _compilerDescriptor;
		private string _tempPath;
		private int _bestEffort;

		public AppDomainCompiler()
		{
			Errors = new List<string>();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="bestEffort">How many tries should do. Each try remove a not compiling item.</param>
		/// <param name="assemblyName">The output assembly name.</param>
		/// <param name="assemblyPath">The output assembly path.</param>
		/// <param name="compilerDescriptor">The specifications for the compilation.</param>
		/// <param name="tempPath">A temporary path where the .cs files will be stored.</param>
		public void Initialize(int bestEffort, string assemblyName, string assemblyPath,
			ISourceCompilerDescriptor compilerDescriptor, string tempPath)
		{
			_bestEffort = Math.Max(1, bestEffort);
			_assemblyName = assemblyName;
			_assemblyPath = assemblyPath;
			_compilerDescriptor = compilerDescriptor;
			_tempPath = tempPath;

			foreach (var toLoad in _compilerDescriptor.Assemblies)
			{
				try
				{
					AppDomain.CurrentDomain.Load(File.ReadAllBytes(toLoad));
				}
				catch (Exception ex)
				{
					Trace.WriteLine(ex.Message);
				}
			}

		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>The compiled assembly path.</returns>
		public string Compile(bool log = false)
		{
			// Create code from the codeDom and compile
			var codeProvider = new CSharpCodeProvider();
			//var options = new CodeGeneratorOptions();

			// Capture Code Generated as a string for error info
			// and debugging
			var compilerParameters = new CompilerParameters(_compilerDescriptor.Assemblies.ToArray());
			//compilerParameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().CodeBase.Substring(8));

			compilerParameters.GenerateInMemory = false;

			compilerParameters.OutputAssembly = Path.Combine(_assemblyPath, _assemblyName + ".dll");

			var nowFileTime = DateTime.UtcNow.ToFileTime().ToString();

			var fileNames = SetupFileNames(nowFileTime).ToArray();
			var bestEffort = _bestEffort;
			var currentTrial = 0;
			while (fileNames.Length > 0 && bestEffort >= 0)
			{
				var erroneousFiles = new List<string>();
				CompilerResults compilerResults = codeProvider.CompileAssemblyFromFile(compilerParameters, fileNames);
				if (compilerResults.Errors.Count > 0)
				{
					var fullErrorText = string.Empty;
					foreach (CompilerError compileError in compilerResults.Errors)
					{
						erroneousFiles.Add(compileError.FileName);
						var errorText = String.Format("File: {3}\tLine: {0}\t Col: {1}\t Error: {2}\n",
																		 compileError.Line, compileError.Column, compileError.ErrorText,
																		 compileError.FileName);
						Errors.Add(errorText);
						fullErrorText += errorText;
					}
					bestEffort--;
					currentTrial++;

					var logPath = Path.Combine(_tempPath, nowFileTime, "compilation." + currentTrial + ".log");
					File.WriteAllText(logPath, fullErrorText);
					fileNames = ReduceFiles(erroneousFiles, fileNames);
				}
				else
				{
					var dirName = Path.GetDirectoryName(fileNames.First());
					Directory.Delete(dirName,true);
					return compilerParameters.OutputAssembly;
				}
			}
			return null;
		}

		/// <summary>
		/// Removes from the files to compile the ones with errors
		/// </summary>
		/// <param name="erroneousFiles"></param>
		/// <param name="fileNames"></param>
		/// <returns></returns>
		private string[] ReduceFiles(List<string> erroneousFiles, string[] fileNames)
		{
			var newFileNames = new List<string>();
			foreach (var fileName in fileNames)
			{
				if (erroneousFiles.All(a => String.Compare(a, fileName, StringComparison.OrdinalIgnoreCase) != 0))
				{
					newFileNames.Add(fileName);
				}
			}
			return newFileNames.ToArray();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private IEnumerable<string> SetupFileNames(string partialPath)
		{
			var newPath = Path.Combine(_tempPath, partialPath);
			Directory.CreateDirectory(newPath);
			foreach (var src in _compilerDescriptor.Sources)
			{
				var srcPath = Path.Combine(newPath, src.ClassName + ".cs");
				File.WriteAllText(srcPath, src.ClassSource);
				yield return srcPath;
			}
		}

		/// <summary>
		/// Needed for MarshalByRef Object
		/// </summary>
		public void Dispose()
		{

		}
	}
}
