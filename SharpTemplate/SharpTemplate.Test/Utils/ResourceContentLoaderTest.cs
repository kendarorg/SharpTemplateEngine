// ===========================================================
// Copyright (C) 2014-2015 Kendar.org
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation 
// files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, 
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software 
// is furnished to do so, subject to the following conditions:
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS 
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF 
// OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ===========================================================


using System.Linq;
using System.Reflection;
using System.Text;
using GenericHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharpTemplate.Test.Utils
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
