using System;
using System.Collections.Generic;

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
			if (!_assemblies.ContainsKey(assembly))
			{
				_assemblies.Add(assembly, assembly);
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
