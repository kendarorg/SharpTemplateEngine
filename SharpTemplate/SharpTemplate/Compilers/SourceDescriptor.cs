using System;

namespace SharpTemplate.Compilers
{
	/// <summary>
	/// Single source descriptor.
	/// Uset to pass the data for a single file between AppDomains
	/// </summary>
	[Serializable]
	public class SourceDescriptor : MarshalByRefObject
	{
		public string OnlyNamespace;
		public string OnlyClass;
		public string ClassName;
		public string ClassSource;
	}
}