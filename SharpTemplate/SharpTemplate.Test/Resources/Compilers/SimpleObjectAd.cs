using System.Reflection;
using SharpTemplate.Test.Compilers;

namespace SharpTemplate.Test.Resources.Compilers
{
	public class SimpleObjectAd : ILoadedClass
	{
		public string GetAssemblyName()
		{
			return Assembly.GetExecutingAssembly().GetName().Name;
		}
	}
}
