using System.Reflection;

namespace SharpTemplate.Test.Resources.Compilers
{
	public class CorrectObject
	{
		public string GetAssemblyName()
		{
			return Assembly.GetExecutingAssembly().GetName().Name;
		}
	}
}
