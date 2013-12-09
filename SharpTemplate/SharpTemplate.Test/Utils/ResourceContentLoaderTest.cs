using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTemplate.Utils;

namespace SharpTemplate.TEst.Utils
{
	[TestClass]
	public class ResourceContentLoaderTest
	{
		[TestMethod]
		public void ShouldReadAResourceAsByteArray()
		{
			byte[] expected = Encoding.UTF8.GetBytes("This is a text file");
			//+3 because of starting tag for UTF8 Files
			var result = ResourceContentLoader.LoadBytes("TestTextFile.txt", Assembly.GetExecutingAssembly()).Skip(3).ToArray();
			CollectionAssert.AreEqual(expected, result);
		}

		[TestMethod]
		public void ShouldReadAResourceAsText()
		{
			const string expected = "This is a text file";
			var result = ResourceContentLoader.LoadText("TestTextFile.txt", Assembly.GetExecutingAssembly());
			Assert.IsTrue(result.Contains(expected));
		}

		[TestMethod]
		public void ShouldFindAResourceDespiteCase()
		{
			var result = ResourceContentLoader.GetResourceName("SharpTemplate.test.Resources.Utils.testTextFile.txt", Assembly.GetExecutingAssembly());
			Assert.AreEqual("SharpTemplate.Test.Resources.Utils.TestTextFile.txt", result);
		}

		[TestMethod]
		public void ShouldFindAResourceEndingWithThePassedValue()
		{
			var result = ResourceContentLoader.GetResourceName("testTextFile.txt", Assembly.GetExecutingAssembly());
			Assert.AreEqual("SharpTemplate.Test.Resources.Utils.TestTextFile.txt", result);
		}

		[TestMethod]
		public void ShouldReturnNullIfNotFound()
		{
			var result = ResourceContentLoader.GetResourceName("nonexisting.txt", Assembly.GetExecutingAssembly(), false);
			Assert.IsNull(result);
		}
	}
}
