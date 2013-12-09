using System.IO;
using System.Reflection;

namespace SharpTemplate.Test
{
	public class TestUtils
	{
		public static string Clean(string toClean)
		{
			toClean = toClean.Replace(" ", "");
			toClean = toClean.Replace("\t", "");
			toClean = toClean.Replace("\r", "");
			toClean = toClean.Replace("\f", "");
			toClean = toClean.Replace("\n", "");
			return toClean.Trim();
		}

		public static string GetExecutionPath()
		{
			var currentAssemblyLocation = Assembly.GetExecutingAssembly().Location;
			return Path.GetDirectoryName(currentAssemblyLocation);
		}
	}
}
