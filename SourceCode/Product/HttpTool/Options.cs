using CommandLine;
using CommandLine.Text;

namespace HttpTool
{
	public class Options
	{
		[VerbOption("standard", HelpText = "Run standard tests (default).")]
		public TestSubOptions TestVerb { get; set; }

		[VerbOption("testall", HelpText = "Run all tests.")]
		public TestSubOptions TestAllVerb { get; set; }

		[VerbOption("enhanced", HelpText = "Run all enhanced tests.")]
		public TestSubOptions TestEnhancedVerb { get; set; }

		[VerbOption("agilitypack", HelpText = "Run agilitypack tests.")]
		public TestSubOptions AgilityPackVerb { get; set; }

		[VerbOption("empty", HelpText = "Run tests to check for empty pages.")]
		public TestSubOptions EmptyVerb { get; set; }

		[VerbOption("images",
			HelpText = "Run tests to check for missing images.")]
		public TestSubOptions ImagesVerb { get; set; }

		[VerbOption("redirects", HelpText = "Run all redirects tests.")]
		public TestSubOptions RedirectsVerb { get; set; }

		[VerbOption("validate", HelpText = "Run W3.org validation tests.")]
		public TestSubOptions ValidateVerb { get; set; }

		[HelpVerbOption]
		public string GetUsage(string verb)
		{
			return HelpText.AutoBuild(this, verb);
		}
	}
}