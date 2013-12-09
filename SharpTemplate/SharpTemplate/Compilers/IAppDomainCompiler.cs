using System;
using System.Collections.Generic;
using System.Runtime.Remoting;

namespace SharpTemplate.Compilers
{
	/// <summary>
	/// Compiler interface
	/// </summary>
	public interface IAppDomainCompiler
	{
		List<string> Errors { get; }

		void Initialize(int bestEffort, string assemblyName, string assemblyPath,
				ISourceCompilerDescriptor compilerDescriptor, string tempPath);

		string Compile();
		void Dispose();
		object GetLifetimeService();
		object InitializeLifetimeService();
		ObjRef CreateObjRef(Type requestedType);
	}
}
