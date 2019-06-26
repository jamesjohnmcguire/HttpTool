/////////////////////////////////////////////////////////////////////////////
// <copyright file="Program.cs" company="James John McGuire">
// Copyright © 2016 - 2019 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using CommandLine;
using System;
using WebTools;

namespace HttpTool
{
	internal class Program
	{
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
		/// The main processing function.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		private bool Run(string[] arguments)
		{
			bool result = false;

			try
			{
				ParserResult<object> commandLine =
					Parser.Default.ParseArguments<AgilityPack, Empty, Enhanced,
					Images, Redirects, Standard, TestAll, Validate>(arguments);

				result = ValidateArguments(commandLine);

				if (true == result)
				{
					Action<object> action = Parsing;

					Parsed<object> parsed =
						(Parsed<object>)commandLine.WithParsed<object>(
							action);

					SiteTest tester = new SiteTest();

					tester.Tests = GetTests(parsed);

					Options option = (Options)parsed.Value;
					string url = option.Url.AbsoluteUri;

					Console.WriteLine("Running tests on: {0}", url);

					tester.Test(url);
				}
			}
			catch (Exception exception)
			{
				int code = (int)CommandLine.ParserResultType.NotParsed;
				Console.WriteLine(exception.ToString());
				Environment.Exit(code);
			}

			return result;
		}

		private static DocumentChecks GetTests(Parsed<object> parsed)
		{
			DocumentChecks tests = DocumentChecks.Basic;

			switch (parsed.Value)
			{
				case Standard standard:
				{
					tests = DocumentChecks.Basic |
						DocumentChecks.ContentErrors;
					break;
				}

				case TestAll testall:
				{
					tests = DocumentChecks.Basic |
						DocumentChecks.ContentErrors |
						DocumentChecks.EmptyContent |
						DocumentChecks.ImagesExist |
						DocumentChecks.ParseErrors |
						DocumentChecks.Redirect |
						DocumentChecks.W3cValidation;
					break;
				}

				case Enhanced enhanced:
				{
					tests = DocumentChecks.Basic |
						DocumentChecks.ContentErrors |
						DocumentChecks.EmptyContent |
						DocumentChecks.ImagesExist |
						DocumentChecks.ParseErrors |
						DocumentChecks.Redirect;
					break;
				}

				case AgilityPack agilitypack:
				{
					tests = DocumentChecks.Basic |
						DocumentChecks.ContentErrors |
						DocumentChecks.ParseErrors;
					break;
				}

				case Empty empty:
				{
					tests = DocumentChecks.Basic |
						DocumentChecks.ContentErrors |
						DocumentChecks.EmptyContent;
					break;
				}

				case Images images:
				{
					tests = DocumentChecks.Basic |
						DocumentChecks.ContentErrors |
						DocumentChecks.EmptyContent |
						DocumentChecks.ImagesExist;
					break;
				}

				case Redirects redirects:
				{
					tests = DocumentChecks.Basic |
						DocumentChecks.ContentErrors |
						DocumentChecks.Redirect;
					break;
				}

				case Validate validate:
				{
					tests = DocumentChecks.Basic |
						DocumentChecks.ContentErrors |
						DocumentChecks.EmptyContent |
						DocumentChecks.W3cValidation;
					break;
				}
			}

			return tests;
		}

		private static void Parsing(object options)
		{
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Summary for ValidateArguments.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		private static bool ValidateArguments(ParserResult<object> commandLine)
		{
			bool result = false;

			if (commandLine.Tag == ParserResultType.Parsed)
			{
				result = true;
			}

			return result;
		}
	}
}
