namespace SharpTemplate.Parsers
{
	/// <summary>
	/// Identifier of a block of data
	/// </summary>
	internal class ParserBlock
	{
		/// <summary>
		/// Content of the data
		/// </summary>
		public string Content { get; private set; }

		public ParserBlockType Blocktype { get; private set; }

		public ParserBlock(string content, ParserBlockType blocktype = ParserBlockType.Direct)
		{
			Content = content;
			Blocktype = blocktype;
		}

		public override string ToString()
		{
			return Content;
		}
	}
}