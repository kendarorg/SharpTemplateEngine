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
using GenericHelpers;
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
			var expected = ResourceContentLoader.LoadText("ShouldLoadCorrectlyANonConsistentFile.csx", Assembly.GetExecutingAssembly());
			var pp = new SharpParser();
			var result = pp.Parse(toParse, "ClassName", "NameSpaceName");
			Assert.AreEqual(TestUtils.Clean(expected), TestUtils.Clean(result));
		}

		[TestMethod]
		public void ShouldLoadBaseUsingAndModel()
		{
			var toParse = ResourceContentLoader.LoadText("ShouldLoadBaseUsingAndModel.input", Assembly.GetExecutingAssembly());
			var expected = ResourceContentLoader.LoadText("ShouldLoadBaseUsingAndModel.csx", Assembly.GetExecutingAssembly());
			var pp = new SharpParser();
			var result = pp.Parse(toParse, "ClassName", "NameSpaceName");
			Assert.AreEqual(TestUtils.Clean(expected), TestUtils.Clean(result));
		}
	}
}
