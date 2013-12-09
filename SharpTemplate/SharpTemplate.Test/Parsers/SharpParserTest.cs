using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTemplate.Parsers;
using SharpTemplate.Test;
using SharpTemplate.Utils;

namespace SharpTemplate.Test.Parsers
{
	[TestClass]
	public class SharpParserTest
	{
		[TestMethod]
		public void ShouldLoadCorrectlyANonConsistentFile()
		{
			var toParse = ResourceContentLoader.LoadText("ShouldLoadCorrectlyANonConsistentFile.input", Assembly.GetExecutingAssembly());
			var expected = ResourceContentLoader.LoadText("ShouldLoadCorrectlyANonConsistentFile.cs", Assembly.GetExecutingAssembly());
			var pp = new SharpParser();
			var result = pp.Parse(toParse, "ClassName", "NameSpaceName");
			Assert.AreEqual(TestUtils.Clean(expected), TestUtils.Clean(result));
		}

		[TestMethod]
		public void ShouldLoadBaseUsingAndModel()
		{
			var toParse = ResourceContentLoader.LoadText("ShouldLoadBaseUsingAndModel.input", Assembly.GetExecutingAssembly());
			var expected = ResourceContentLoader.LoadText("ShouldLoadBaseUsingAndModel.cs", Assembly.GetExecutingAssembly());
			var pp = new SharpParser();
			var result = pp.Parse(toParse, "ClassName", "NameSpaceName");
			Assert.AreEqual(TestUtils.Clean(expected), TestUtils.Clean(result));
		}
	}
}
