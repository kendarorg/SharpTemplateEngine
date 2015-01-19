// ===========================================================
// Copyright (c) 2014-2015, Enrico Da Ros/kendar.org
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
