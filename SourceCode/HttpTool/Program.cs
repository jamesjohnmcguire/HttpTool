/////////////////////////////////////////////////////////////////////////////
// <copyright file="Program.cs" company="James John McGuire">
// Copyright © 2016 - 2020 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using CommandLine;
using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using WebTools;

namespace HttpTool
{
	internal class Program
	{
		private static readonly ResourceManager StringTable = new
			ResourceManager(
				"HttpTool.Resources",
				Assembly.GetExecutingAssembly());

		private static DocumentChecks GetTests(Parsed<object> parsed)
		{
			DocumentChecks tests = DocumentChecks.Basic;

			switch (parsed.Value)
			{
				case Standard:
					tests =
						DocumentChecks.Basic | DocumentChecks.ContentErrors;
					break;
				case TestAll:
					tests = DocumentChecks.Basic |
						DocumentChecks.ContentErrors |
						DocumentChecks.EmptyContent |
						DocumentChecks.ImagesExist |
						DocumentChecks.ParseErrors |
						DocumentChecks.Redirect |
						DocumentChecks.W3cValidation;
					break;
				case Enhanced:
					tests = DocumentChecks.Basic |
						DocumentChecks.ContentErrors |
						DocumentChecks.EmptyContent |
						DocumentChecks.ImagesExist |
						DocumentChecks.ParseErrors |
						DocumentChecks.Redirect;
					break;
				case AgilityPack:
					tests = DocumentChecks.Basic |
						DocumentChecks.ContentErrors |
						DocumentChecks.ParseErrors;
					break;
				case Empty:
					tests = DocumentChecks.Basic |
						DocumentChecks.ContentErrors |
						DocumentChecks.EmptyContent;
					break;
				case Images:
					tests = DocumentChecks.Basic |
						DocumentChecks.ContentErrors |
						DocumentChecks.EmptyContent |
						DocumentChecks.ImagesExist;
					break;
				case Redirects:
					tests = DocumentChecks.Basic |
						DocumentChecks.ContentErrors |
						DocumentChecks.Redirect;
					break;
				case Validate:
					tests = DocumentChecks.Basic |
						DocumentChecks.ContentErrors |
						DocumentChecks.EmptyContent |
						DocumentChecks.W3cValidation;
					break;
			}

			return tests;
		}

		private static int Main(string[] args)
		{
			int returnCode = -1;

			bool resultCode = Run(args);

			if (true == resultCode)
			{
				returnCode = 0;
			}

			return returnCode;
		}

		private static void Parsing(object options)
		{
		}

		/////////////////////////////////////////////////////////////////////
		// Run
		/// <summary>
		/// The main processing function.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		[System.Diagnostics.CodeAnalysis.SuppressMessage(
			"Style",
			"IDE0017:Simplify object initialization",
			Justification = "Don't agree with this rule.")]
		private static bool Run(string[] arguments)
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

					using SiteTest tester = new SiteTest();

					tester.Tests = GetTests(parsed);

					Options option = (Options)parsed.Value;
					string url = option.Url.AbsoluteUri;

					string message = StringTable.GetString(
						"RUNNING_TESTS",
						CultureInfo.InstalledUICulture);
					Console.WriteLine(message, url);

					tester.Test(url);
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.ToString());

				throw;
			}

			return result;
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
