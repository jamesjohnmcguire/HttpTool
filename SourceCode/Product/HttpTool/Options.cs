using CommandLine;
using CommandLine.Text;

namespace HttpTool
{
	public class Options
	{
		[Option("standard", Default = false, HelpText = "Run standard tests (default).")]
		public TestSubOptions TestVerb { get; set; }

		[Option("testall", HelpText = "Run all tests.")]
		public TestSubOptions TestAllVerb { get; set; }

		[Option("enhanced", HelpText = "Run all enhanced tests.")]
		public TestSubOptions TestEnhancedVerb { get; set; }

		[Option("agilitypack", HelpText = "Run agilitypack tests.")]
		public TestSubOptions AgilityPackVerb { get; set; }

		[Option("empty", HelpText = "Run tests to check for empty pages.")]
		public TestSubOptions EmptyVerb { get; set; }

		[Option("images",
			HelpText = "Run tests to check for missing images.")]
		public TestSubOptions ImagesVerb { get; set; }

		[Option("redirects", HelpText = "Run all redirects tests.")]
		public TestSubOptions RedirectsVerb { get; set; }

		[Option("validate", HelpText = "Run W3.org validation tests.")]
		public TestSubOptions ValidateVerb { get; set; }

		//[Help]
		//public string GetUsage(string verb)
		//{
		//	return HelpText.AutoBuild<string>(CommandLine.Parser<verb>);
		//}
	}
}