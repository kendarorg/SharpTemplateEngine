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
