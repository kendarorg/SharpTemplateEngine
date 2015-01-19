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


using System;
using System.Collections.Generic;

namespace SharpTemplate.Parsers
{
	/// <summary>
	/// Parser for the .sharp files
	/// </summary>
	public class SharpParser
	{
		/// <summary>
		/// Parse and generate the full SharpClass text
		/// </summary>
		/// <param name="toParse"></param>
		/// <param name="className"></param>
		/// <param name="nameSpace"></param>
		/// <returns></returns>
		public string Parse(string toParse, string className, string nameSpace)
		{
			var parsed = SplitBlocks(toParse);
			var elaborated = GenerateBlocks(parsed);
			var sharpClass = GenerateClass(elaborated, className, nameSpace);
			return sharpClass.ToString();
		}

		/// <summary>
		/// Parse and generate a single SharpClass item
		/// </summary>
		/// <param name="toParse"></param>
		/// <param name="className"></param>
		/// <param name="nameSpace"></param>
		/// <returns></returns>
		public SharpClass ParseClass(string toParse, string className, string nameSpace)
		{
			var parsed = SplitBlocks(toParse);
			var elaborated = GenerateBlocks(parsed);
			return GenerateClass(elaborated, className, nameSpace);
		}

		/// <summary>
		/// Iterate the block and setup the SharpClass content
		/// </summary>
		/// <param name="elaborated"></param>
		/// <param name="className"></param>
		/// <param name="nameSpace"></param>
		/// <returns></returns>
		private SharpClass GenerateClass(IEnumerable<ParserBlock> elaborated, string className, string nameSpace)
		{
			var sharpClass = new SharpClass
											 {
												 ClassName = className,
												 ClassNamespace = nameSpace
											 };

			foreach (var block in elaborated)
			{
				switch (block.Blocktype)
				{
					case (ParserBlockType.Base):
						if (!string.IsNullOrWhiteSpace(sharpClass.Base)) throw new Exception("Duplicate Base Tag");
						sharpClass.Base = block.Content;
						break;
					case (ParserBlockType.Code):
						sharpClass.Content += block.Content;
						break;
					case (ParserBlockType.Direct):
						sharpClass.Content += block.Content;
						break;
					case (ParserBlockType.Model):
						if (!string.IsNullOrWhiteSpace(sharpClass.Model)) throw new Exception("Duplicate Model Tag");
						sharpClass.Model = block.Content;
						break;
					case (ParserBlockType.Using):
						sharpClass.Using.Add(block.Content);
						break;
				}
			}
			if (string.IsNullOrWhiteSpace(sharpClass.Model))
			{
				sharpClass.Model = "object";
			}
			return sharpClass;
		}

		/// <summary>
		/// Identify special blocks in .sharp files,
		/// </summary>
		/// <param name="toParse"></param>
		/// <returns></returns>
		private static IEnumerable<string> SplitBlocks(string toParse)
		{
			var parsed = new List<string>();
			int prevPos = 0;
			int pos = toParse.IndexOf("<#", prevPos, StringComparison.OrdinalIgnoreCase);

			// ReSharper disable once TooWideLocalVariableScope
			int newPos;
			while (pos >= 0)
			{
				newPos = toParse.IndexOf("#>", pos, StringComparison.OrdinalIgnoreCase);
				if (newPos >= 0) newPos += 2;
				if (pos > (prevPos + 1))
				{
					var sblock = toParse.Substring(prevPos, pos - prevPos);
					if (sblock.Length > 0) parsed.Add(sblock);
				}

				var block = toParse.Substring(pos, newPos - pos);
				if (block.Length > 0) parsed.Add(block);

				prevPos = Math.Max(pos, newPos);
				pos = toParse.IndexOf("<#", prevPos, StringComparison.OrdinalIgnoreCase);
				if (pos < 0)
				{
					var sblock = toParse.Substring(prevPos);
					if (sblock.Length > 0) parsed.Add(sblock);
				}
			}
			return parsed;
		}


		/// <summary>
		/// Setup block contents
		/// </summary>
		/// <param name="parsed"></param>
		/// <returns></returns>
		private IEnumerable<ParserBlock> GenerateBlocks(IEnumerable<string> parsed)
		{
			var elaborated = new List<ParserBlock>();
			var prevIsTyped = false;
			foreach (var block in parsed)
			{
				if (block.StartsWith("<#"))
				{
					var sblock = block.Substring(2, block.Length - 4);

					if (sblock.Length > 0)
					{
						var typed = BuildCodeType(sblock);
						elaborated.Add(typed);
						prevIsTyped = typed.Blocktype != ParserBlockType.Code;
					}
				}
				else
				{
					if (prevIsTyped)
					{
						prevIsTyped = false;
						if (block == "\r\n" || block == "\r\f" || block == "\n")
						{
							continue;
						}
					}
					var sblock = CleanStringBlock(block);
					if (sblock.Length > 0) elaborated.Add(new ParserBlock(sblock));
					// ReSharper disable once RedundantAssignment
					prevIsTyped = false;
				}
			}
			return elaborated;
		}

		/// <summary>
		/// Setup the usage of Write, to write the text
		/// </summary>
		/// <param name="block"></param>
		/// <returns></returns>
		private string CleanStringBlock(string block)
		{
			var toWrite = "\r\nWrite(\"";

			block = block.Replace("\\", "\\\\");
			block = block.Replace("\"", "\\\"");

			block = block.Replace("\n", "\\n");
			block = block.Replace("\r", "\\r");
			block = block.Replace("\f", "\\f");

			toWrite += block;

			return toWrite + "\");\r\n";
		}

		/// <summary>
		/// Identify special block types
		/// </summary>
		/// <param name="sblock"></param>
		/// <returns></returns>
		private ParserBlock BuildCodeType(string sblock)
		{
			var content = sblock;
			var blockType = ParserBlockType.Code;

			if (sblock.StartsWith("using#"))
			{
				content = sblock.Substring("using#".Length).Trim();
				blockType = ParserBlockType.Using;
			}
			else if (sblock.StartsWith("model#"))
			{
				content = sblock.Substring("model#".Length).Trim();
				blockType = ParserBlockType.Model;
			}
			else if (sblock.StartsWith("base#"))
			{
				content = sblock.Substring("base#".Length).Trim();
				blockType = ParserBlockType.Base;
			}
			return new ParserBlock(content, blockType);
		}
	}
}
