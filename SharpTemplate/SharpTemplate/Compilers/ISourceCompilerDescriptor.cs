using System;
using System.Collections.Generic;
using System.Runtime.Remoting;

namespace SharpTemplate.Compilers
{
	public interface ISourceCompilerDescriptor
	{
		IEnumerable<string> Assemblies { get; }
		IEnumerable<SourceDescriptor> Sources { get; }
		string AddFile(string nameSpace, string name, string source);
		void Dispose();
		void AddAssembly(string assembly);
		void CopyTo(ISourceCompilerDescriptor d);
		object GetLifetimeService();
		object InitializeLifetimeService();
		ObjRef CreateObjRef(Type requestedType);
	}
}
