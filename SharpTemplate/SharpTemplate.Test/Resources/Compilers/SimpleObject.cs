using System.Reflection;

namespace SharpTemplate.Test.Resources.Compilers
{
	public class SimpleObject
	{
		public string GetAssemblyName()
		{
			return Assembly.GetExecutingAssembly().GetName().Name;
		}
	}
}
