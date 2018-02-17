using CommandLine;

namespace HttpTool
{
	public class TestSubOptions : CommonSubOptions
	{
		[ValueOption(1)]
		public string Url { get; set; }
	}
}