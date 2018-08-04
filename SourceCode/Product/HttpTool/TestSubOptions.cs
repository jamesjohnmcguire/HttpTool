using CommandLine;

namespace HttpTool
{
	public class TestSubOptions : CommonSubOptions
	{
		[Option("url")]
		public string Url { get; set; }
	}
}