using System;
using WebTools;

namespace HttpTool
{
	internal class Program
	{
		private string command = string.Empty;
		private TestSubOptions options = null;

		private static int Main(string[] args)
		{
			int returnCode = -1;

			Program application = new Program();

			bool resultCode = application.Run(args);

			if (true == resultCode)
			{
				returnCode = 0;
			}

			return returnCode;
		}

		/////////////////////////////////////////////////////////////////////
		// Run
		/// <summary>
		/// The main processing function
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		protected internal bool Run(string[] arguments)
		{
			bool successCode = false;

			try
			{
				successCode = ValidateArguments(arguments);

				if (false == successCode)
				{
					ShowUsage();
				}
				else
				{
					SiteTest tester = new SiteTest();

					switch (command)
					{
					case "standard":
						{
							tester.Tests = DocumentChecks.Basic |
								DocumentChecks.ContentErrors;
							break;
						}
					case "testall":
						{
							tester.Tests = DocumentChecks.Basic |
								DocumentChecks.ContentErrors |
								DocumentChecks.EmptyContent |
								DocumentChecks.ImagesExist |
								DocumentChecks.ParseErrors |
								DocumentChecks.Redirect |
								DocumentChecks.W3cValidation;
							break;
						}
					case "enhanced":
						{
							tester.Tests = DocumentChecks.Basic |
								DocumentChecks.ContentErrors |
								DocumentChecks.EmptyContent |
								DocumentChecks.ImagesExist |
								DocumentChecks.ParseErrors |
								DocumentChecks.Redirect;
							break;
						}
					case "agilitypack":
						{
							tester.Tests = DocumentChecks.Basic |
								DocumentChecks.ContentErrors |
								DocumentChecks.ParseErrors;
							break;
						}
					case "empty":
						{
							tester.Tests = DocumentChecks.Basic |
								DocumentChecks.ContentErrors |
								DocumentChecks.EmptyContent;
							break;
						}
					case "images":
						{
							tester.Tests = DocumentChecks.Basic |
								DocumentChecks.ContentErrors |
								DocumentChecks.EmptyContent |
								DocumentChecks.ImagesExist;
							break;
						}
					case "redirects":
						{
							tester.Tests = DocumentChecks.Basic |
								DocumentChecks.ContentErrors |
								DocumentChecks.Redirect;
							break;
						}
					case "validate":
						{
							tester.Tests = DocumentChecks.Basic |
								DocumentChecks.ContentErrors |
								DocumentChecks.EmptyContent |
								DocumentChecks.W3cValidation;
							break;
						}
					}

					TestSubOptions subOptions = (TestSubOptions)this.options;
					Console.WriteLine("Running tests on: {0}", options.Url);

					tester.Test(options.Url);
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.ToString());
				Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
			}

			return successCode;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// ShowUsage - Displays a message on how to use this application.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		protected internal void ShowUsage()
		{
			Console.WriteLine("usage: HttpTool <command> <URL>");
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Summary for ValidateArguments.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		protected internal bool ValidateArguments(string[] arguments)
		{
			bool result = false;
			Options options = new Options();

			if (CommandLine.Parser.Default.ParseArguments(arguments, options,
				(verb, additionalOptions) =>
			{
				// if parsing succeeds the verb name and correct instance
				// will be passed to onVerbCommand delegate (string,object)
				command = verb;
				this.options = (TestSubOptions)additionalOptions;
			}))
			{
				result = true;
			}

			return result;
		}
	}
}