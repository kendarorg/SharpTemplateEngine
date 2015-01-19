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


using System.Collections.Generic;

namespace SharpTemplate.Parsers
{
	/// <summary>
	/// Descriptor for a sharp parser result
	/// </summary>
	public class SharpClass
	{
		public SharpClass()
		{
			//Default using
			Using = new List<string>
			        {
				        "System", 
						"System.Collections.Generic",
						"SharpTemplate.Parsers"
			        };
		}

		/// <summary>
		/// Class name
		/// </summary>
		public string ClassName { get; set; }

		/// <summary>
		/// Namespace for the generateed class
		/// </summary>
		public string ClassNamespace { get; set; }

		/// <summary>
		/// Base class
		/// </summary>
		public string Base { get; set; }

		/// <summary>
		/// Type of the object that will be passed to execute function
		/// </summary>
		public string Model { get; set; }

		/// <summary>
		/// List of using
		/// </summary>
		public List<string> Using { get; set; }

		/// <summary>
		/// Source code for the Execute method
		/// </summary>
		public string Content { get; set; }

		public override string ToString()
		{
			var total = string.Empty;
			foreach (var item in Using)
			{
				total += "using " + item + ";\r\n";
			}
			total += "\r\n";
			total += "namespace " + ClassNamespace + "\r\n";
			total += "{\r\n";

			total += "public class " + ClassName;
			if (Base != null)
			{
				total += " : " + Base + ", ISharpResult";
			}
			else
			{
				total += " : ISharpResult";
			}
			total += "\r\n";
			total += "{\r\n";

			total += "public string Content { get; set; }\r\n";

			total += "public void Write(string toWrite, bool plusCrLf = false)\r\n";
			total += "{\r\n";
			total += "Content += toWrite;\r\n";
			total += "if (plusCrLf) Content += \"\\r\\n\";\r\n";
			total += "}\r\n";

			total += "public void Execute(object modelAsObject)\r\n";
			total += "{\r\n";
			if (Model != "object")
			{
				total += "var model = modelAsObject as " + Model + ";\r\n";
				total += "if(model==null) throw new InvalidCastException(\"Model must be of type '" + Model + "'\");\r\n";
			}
			else
			{
				total += "var model = modelAsObject;\r\n";
			}
			total += Content;
			total += "\r\n";
			total += "}\r\n";

			total += "}\r\n";

			total += "}\r\n";
			return total;
		}
	}
}