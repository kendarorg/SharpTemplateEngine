using System;
using System.Collections.Generic;
using SharpTemplate.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTemplate.Test.Parsers;

namespace NameSpaceName
{
public class ClassName : SharpParserTest, ISharpResult
{
public string Content { get; set; }
public void Write(string toWrite, bool plusCrLf = false)
{
Content += toWrite;
if (plusCrLf) Content += "\r\n";
}
public void Execute(object modelAsObject)
{
var model = modelAsObject as System.DateTime;
if(model==null) throw new InvalidCastException("Model must be of type 'System.DateTime'");

	if(model.Now()==null){
		throw new Exception("An exception");
	}else{

Write("aaa");

	}

Write("\n");

}
}
}
