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

namespace SharpTemplate.Compilers
{
	/// <summary>
	/// The descriptor of all things that must be compiled.
	/// Marked as serializable and MarshalByRefObject to allow inter AppDomain communication
	/// </summary>
	[Serializable]
	public class SourceCompilerDescriptor : MarshalByRefObject, ISourceCompilerDescriptor
	{
		/// <summary>
		/// List of files
		/// </summary>
		private readonly Dictionary<string, SourceDescriptor> _files;

		/// <summary>
		/// List of assemblies path to load 
		/// </summary>
		private readonly Dictionary<string, string> _assemblies;

		/// <summary>
		/// The list of referenced assemblies
		/// </summary>
		public IEnumerable<string> Assemblies
		{
			get { return _assemblies.Values; }
		}

		public SourceCompilerDescriptor()
		{
			_assemblies = new Dictionary<string, string>();
			_files = new Dictionary<string, SourceDescriptor>();
		}

		/// <summary>
		/// Add a file to compile
		/// </summary>
		/// <param name="nameSpace">Namespace</param>
		/// <param name="name">Class Name</param>
		/// <param name="source">Source code</param>
		/// <returns></returns>
		public string AddFile(string nameSpace, string name, string source)
		{
			var clName = nameSpace + "." + name;
			_files.Add(clName, new SourceDescriptor
				{
					ClassName = clName,
					ClassSource = source,
					OnlyClass = clName,
					OnlyNamespace = nameSpace
				});
			return clName;
		}

		public void Dispose()
		{
			_assemblies.Clear();
			_files.Clear();
		}

		/// <summary>
		/// Add an assembly path
		/// </summary>
		/// <param name="assembly"></param>
		public void AddAssembly(string assembly)
		{
			var fileName = Path.GetFileName(assembly);
			if (!string.IsNullOrWhiteSpace(fileName) && !_assemblies.ContainsKey(fileName))
			{
				_assemblies.Add(fileName, assembly);
			}
		}

		/// <summary>
		/// The list of all values
		/// </summary>
		public IEnumerable<SourceDescriptor> Sources
		{
			get { return _files.Values; }
		}

		/// <summary>
		/// To move data outside of appdomain.
		/// </summary>
		/// <param name="d"></param>
		public void CopyTo(ISourceCompilerDescriptor d)
		{
			foreach (var item in _files)
			{
				var val = item.Value;
				d.AddFile(val.OnlyNamespace, val.OnlyClass, val.ClassSource);
			}

			foreach (var item in _assemblies)
			{
				var val = item.Value;
				d.AddAssembly(val);
			}
		}
	}
}
