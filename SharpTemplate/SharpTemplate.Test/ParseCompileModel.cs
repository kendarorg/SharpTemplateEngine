namespace SharpTemplate.Test
{
	public class ParseCompileModel
	{
		public ParseCompileModel(string className,string methodName,int upTo)
		{
			ClassName = className;
			MethodName = methodName;
			UpTo = upTo;
		}

		public string ClassName { get; private set; }
		public string MethodName { get; private set; }
		public int UpTo { get; private set; }
	}
}
