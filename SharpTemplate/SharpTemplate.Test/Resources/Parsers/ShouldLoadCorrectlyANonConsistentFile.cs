using System;
using System.Collections.Generic;
using SharpTemplate.Parsers;

namespace NameSpaceName
{
	public class ClassName : ISharpResult
	{
		public string Content { get; set; }
		public void Write(string toWrite, bool plusCrLf = false)
		{
			Content += toWrite;
			if (plusCrLf) Content += "\r\n";
		}
		public void Execute(object modelAsObject)
		{
			var model = modelAsObject;
			aaa
			Write("bbb");
			cccc
			Write("\r\n");
			ddd 
			Write("\r\neee\r\nfff");
		}
	}
}
