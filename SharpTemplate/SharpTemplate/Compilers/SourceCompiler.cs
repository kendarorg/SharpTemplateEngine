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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using SharpTemplate.Parsers;

namespace SharpTemplate.Compilers
{
	public class SourceCompiler : IDisposable
	{
		private readonly string _asmName;
		private readonly string _asmPath;
		private readonly Dictionary<string, object> _references;
		private readonly ISourceCompilerDescriptor _sourceCompilerDescriptor;


		/// <summary>
		/// Internal template to build a reference to the eventual keyFile
		/// </summary>
		const string KEY_FILE_DUMMY = @"
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyKeyFile(""{0}"")]

namespace Testn
{{
    public class Testc
{{

}}
}}
";

		/// <summary>
		/// The assemblies that are not loaded yet but known (thus added through AddSearchAssembly)
		/// </summary>
		private ConcurrentDictionary<string, Assembly> _preloadedAssembly;

		/// <summary>
		/// Errors, by compilation run
		/// </summary>
		public List<List<string>> Errors { get; private set; }

		/// <summary>
		/// The full qualified path of the key file for signing. The ".snk" file
		/// </summary>
		public string Key { get; set; }

		public bool HasErrors
		{
			get
			{
				if (Errors.Count == 0) return false;
				foreach (var error in Errors)
				{
					if (error.Count > 0) return true;
				}
				return false;
			}
		}

		/// <summary>
		/// If should setup the compiler into a new AppDomain and discard it after the compilation
		/// </summary>
		public bool UseAppdomain = false;

		/// <summary>
		/// Add an assembly to the current assembly resolver
		/// </summary>
		/// <param name="asm"></param>
		public void AddSearchAssembly(Assembly asm)
		{
			var asmName = asm.GetName().Name;
			_preloadedAssembly.AddOrUpdate(asmName, asm, (a, b) => asm);
		}

		/// <summary>
		/// Unregister the AssemblyResolve callback
		/// </summary>
		public void Dispose()
		{
			AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="asmName">The new name for the assembly</param>
		/// <param name="asmPath">The new assembly path</param>
		public SourceCompiler(string asmName, string asmPath)
		{
			_preloadedAssembly = new ConcurrentDictionary<string, Assembly>();
			AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

			AddSearchAssembly(Assembly.GetExecutingAssembly());
			Errors = new List<List<string>>();
			_asmName = asmName;
			_asmPath = asmPath;
			var fullPath = Path.Combine(_asmPath, _asmName + ".dll");
			if (File.Exists(fullPath)) File.Delete(fullPath);
			_sourceCompilerDescriptor = new SourceCompilerDescriptor();
			_references = new Dictionary<string, object>();
		}

		/// <summary>
		/// Add a file to compile
		/// </summary>
		/// <param name="nameSpace">Namespace for the file</param>
		/// <param name="name">New class name</param>
		/// <param name="source">Source content</param>
		/// <param name="reference"></param>
		/// <returns></returns>
		public string AddFile(string nameSpace, string name, string source, object reference = null)
		{
			var className = nameSpace + "." + name;
			if (!_references.ContainsKey(className))
			{
				_references.Add(className, reference);
				return _sourceCompilerDescriptor.AddFile(nameSpace, name, source);
			}
			throw new DuplicateNameException(string.Format("Duplicate class name {0}", className));
		}

		/// <summary>
		/// Add a parsed Sharp file
		/// </summary>
		/// <param name="classDefinition"></param>
		/// <param name="reference"></param>
		/// <returns></returns>
		public string AddFile(SharpClass classDefinition, object reference = null)
		{
			return AddFile(classDefinition.ClassNamespace, classDefinition.ClassName, classDefinition.ToString(), reference);
		}

		/// <summary>
		/// Load path of all assemblies loaded in the current appdomain
		/// </summary>
		public void LoadCurrentAssemblies()
		{
			AddSearchAssembly(Assembly.GetCallingAssembly());
			AddAssembly(Assembly.GetCallingAssembly().Location);
			AddAssembly(Assembly.GetExecutingAssembly().Location);

			if (typeof(Microsoft.CSharp.RuntimeBinder.Binder).Name == "")
			{
				throw new NotSupportedException("Runtime binder must exist");
			}

			var assemblies = new List<string>();

			assemblies.Add(Assembly.GetExecutingAssembly().CodeBase.Replace("file:\\", "")
							.Replace("file:///", ""));

			assemblies.AddRange(new List<string>(AppDomain.CurrentDomain.GetAssemblies().
																					 Where((a) => !a.IsDynamic).Select((a) =>
																					 {
																						 var uri = new Uri(a.CodeBase);
																						 return uri.LocalPath;
																					 })));
			assemblies.Reverse();

			var dict = new HashSet<string>();
			for (var i = assemblies.Count - 1; i >= 0; i--)
			{
				var asmPath = Path.GetFileName(assemblies[i]);
				if (asmPath != null)
				{
					asmPath = asmPath.ToLowerInvariant();
					if (dict.Contains(asmPath))
					{
						assemblies.RemoveAt(i);
					}
					else
					{
						dict.Add(asmPath);
					}
				}
			}
			foreach (var toAddAsm in assemblies)
			{
				AddAssembly(toAddAsm);
			}
		}

		/// <summary>
		/// Add a single assembly path
		/// </summary>
		/// <param name="assembly"></param>
		public void AddAssembly(string assembly)
		{
			_sourceCompilerDescriptor.AddAssembly(assembly);
		}

		/// <summary>
		/// Compile all that is defined
		/// </summary>
		/// <param name="bestEffort">How many times should try to compile.</param>
		/// <returns>The path of the freshly created dll</returns>
		public string Compile(int bestEffort = 1)
		{
			if (!string.IsNullOrEmpty(Key))
			{
				var fileContent = string.Format(KEY_FILE_DUMMY, Key.Replace("\\", "\\\\"));
				AddFile("Testn", "Testc", fileContent);
			}
			string resultAssemblyPath = null;

			AppDomain compileAppDomain = null;
			if (UseAppdomain) compileAppDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString());
			try
			{
				Assembly ass = Assembly.GetExecutingAssembly();
				string assemblyLocation = ass.Location;
				string assemblyCodeBase = ass.CodeBase.Replace("file:///", "");

				var appDomainCompilerType = typeof(AppDomainCompiler);
				var sourceCompilerDescriptorType = typeof(SourceCompilerDescriptor);

				IAppDomainCompiler appDomainCompiler = null;
				ISourceCompilerDescriptor clonedSourceDescriptor = null;

				if (UseAppdomain)
				{
					appDomainCompiler = (IAppDomainCompiler)CreateInstance(compileAppDomain, assemblyLocation, appDomainCompilerType, assemblyCodeBase);
				}
				else
				{
					appDomainCompiler = new AppDomainCompiler();
				}

				if (UseAppdomain)
				{
					clonedSourceDescriptor = (ISourceCompilerDescriptor)CreateInstance(compileAppDomain, assemblyLocation, sourceCompilerDescriptorType, assemblyCodeBase);
				}
				else
				{
					clonedSourceDescriptor = new SourceCompilerDescriptor();
				}

				_sourceCompilerDescriptor.CopyTo(clonedSourceDescriptor);

				appDomainCompiler.Initialize(bestEffort, _asmName, _asmPath, clonedSourceDescriptor, _asmPath);
				resultAssemblyPath = appDomainCompiler.Compile();
				if (appDomainCompiler.Errors.Count > 0)
				{
					Errors.Add(appDomainCompiler.Errors);
				}
			}
			catch (Exception ex)
			{
				Errors.Add(new List<string>
                {
                    "Unexpected Exception",
                    ex.ToString()
                });
			}
			finally
			{
				if (UseAppdomain) AppDomain.Unload(compileAppDomain);
			}

			return resultAssemblyPath;
		}

		/// <summary>
		/// Create an instance of the required type using the AppDomain
		/// </summary>
		/// <param name="compileAppDomain"></param>
		/// <param name="assemblyLocation"></param>
		/// <param name="classType"></param>
		/// <param name="assemblyCodeBase"></param>
		/// <returns></returns>
		private static object CreateInstance(AppDomain compileAppDomain, string assemblyLocation,
																																						 Type classType, string assemblyCodeBase)
		{
			object instance = null;
			try
			{
				instance = compileAppDomain.CreateInstanceFrom(assemblyLocation, classType.FullName).Unwrap();
			}
			catch (Exception)
			{
			}
			if (instance == null)
			{
				try
				{
					instance = compileAppDomain.CreateInstanceFrom(assemblyCodeBase, classType.FullName).Unwrap();
				}
				catch (Exception)
				{
				}
			}
			return instance;
		}


		/// <summary>
		/// Internal callback to facilitate assembly resolving
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			var asmName = new AssemblyName(args.Name).Name;
			if (_preloadedAssembly.ContainsKey(asmName))
			{
				return _preloadedAssembly[asmName];
			}
			return null;
		}
	}
}
