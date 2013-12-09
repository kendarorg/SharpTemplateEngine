namespace SharpTemplate.Parsers
{
	/// <summary>
	/// The interface implemented by the parser
	/// </summary>
	public interface ISharpResult
	{
		void Execute(object modelAsObject);
		void Write(string text, bool addCrLf = false);
		string Content { get; set; }
	}
}
