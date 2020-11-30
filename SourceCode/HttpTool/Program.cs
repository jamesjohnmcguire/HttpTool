/////////////////////////////////////////////////////////////////////////////
// <copyright file="Program.cs" company="James John McGuire">
// Copyright © 2016 - 2020 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using WebTools;

namespace HttpTool
{
	internal class Program
	{
		private static readonly string[] Commands =
		{
			"agilitypack", "empty", "enhanced", "help", "images", "redirects",
			"standard", "testall", "validate"
		};

		private static readonly ResourceManager StringTable = new
			ResourceManager(
				"HttpTool.Resources",
				Assembly.GetExecutingAssembly());

		private static string GetCommand(string[] arguments)
		{
			string command;

			if (arguments.Length < 2)
			{
				command = "standard";
			}
			else
			{
				command = arguments[0];
			}

			return command;
		}

		private static DocumentChecks GetTests(string command)
		{
			DocumentChecks tests = command switch
			{
				"testall" => DocumentChecks.Basic |
					DocumentChecks.ContentErrors |
					DocumentChecks.EmptyContent |
					DocumentChecks.ImagesExist |
					DocumentChecks.ParseErrors |
					DocumentChecks.Redirect |
					DocumentChecks.W3cValidation,
				"enhanced" => DocumentChecks.Basic |
					DocumentChecks.ContentErrors |
					DocumentChecks.EmptyContent |
					DocumentChecks.ImagesExist |
					DocumentChecks.ParseErrors |
					DocumentChecks.Redirect,
				"agilitypack" => DocumentChecks.Basic |
					DocumentChecks.ContentErrors |
					DocumentChecks.ParseErrors,
				"empty" => DocumentChecks.Basic |
					DocumentChecks.ContentErrors |
					DocumentChecks.EmptyContent,
				"images" => DocumentChecks.Basic |
					DocumentChecks.ContentErrors |
					DocumentChecks.EmptyContent |
					DocumentChecks.ImagesExist,
				"redirects" => DocumentChecks.Basic |
					DocumentChecks.ContentErrors |
					DocumentChecks.Redirect,
				"validate" => DocumentChecks.Basic |
					DocumentChecks.ContentErrors |
					DocumentChecks.EmptyContent |
					DocumentChecks.W3cValidation,
				_ => DocumentChecks.Basic | DocumentChecks.ContentErrors,
			};

			return tests;
		}

		private static string GetUrl(string[] arguments)
		{
			string url;

			if (arguments.Length < 2)
			{
				url = arguments[0];
			}
			else
			{
				url = arguments[1];
			}

			return url;
		}

		private static int Main(string[] arguments)
		{
			int returnCode = -1;

			bool resultCode = Run(arguments);

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
		private static bool Run(string[] arguments)
		{
			bool result;

			try
			{
				result = ValidateArguments(arguments);

				if (true == result)
				{
					string command = GetCommand(arguments);
					string url = GetUrl(arguments);
					DocumentChecks tests = GetTests(command);

					using SiteTest tester = new SiteTest(tests);

					string message = StringTable.GetString(
						"RUNNING_TESTS",
						CultureInfo.InstalledUICulture);
					Console.WriteLine(message, url);

					tester.Test(url);
				}
				else
				{
					ShowHelp(null);
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.ToString());

				throw;
			}

			return result;
		}

		private static void ShowHelp(string additionalMessage)
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			string location = assembly.Location;

			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(location);

			string companyName = versionInfo.CompanyName;
			string copyright = versionInfo.LegalCopyright;

			AssemblyName assemblyName = assembly.GetName();
			string name = assemblyName.Name;
			Version version = assemblyName.Version;
			string assemblyVersion = version.ToString();

			string header = string.Format(
				CultureInfo.CurrentCulture,
				"{0} {1} {2} {3}",
				name,
				assemblyVersion,
				copyright,
				companyName);
			Console.WriteLine(header);

			if (!string.IsNullOrWhiteSpace(additionalMessage))
			{
				Console.WriteLine(additionalMessage);
			}

			string message = StringTable.GetString(
				"USAGE",
				CultureInfo.InstalledUICulture);
			Console.WriteLine(message);

			message = StringTable.GetString(
				"HTTPTOOL",
				CultureInfo.InstalledUICulture);
			Console.WriteLine(message);

			message = StringTable.GetString(
				"COMMANDS",
				CultureInfo.InstalledUICulture);
			Console.WriteLine(message);

			message = StringTable.GetString(
				"AGILITYPACK",
				CultureInfo.InstalledUICulture);
			Console.WriteLine(message);

			message = StringTable.GetString(
				"EMPTY",
				CultureInfo.InstalledUICulture);
			Console.WriteLine(message);

			message = StringTable.GetString(
				"ENHANCED",
				CultureInfo.InstalledUICulture);
			Console.WriteLine(message);

			message = StringTable.GetString(
				"IMAGES",
				CultureInfo.InstalledUICulture);
			Console.WriteLine(message);

			message = StringTable.GetString(
				"REDIRECTS",
				CultureInfo.InstalledUICulture);
			Console.WriteLine(message);

			message = StringTable.GetString(
				"STANDARD",
				CultureInfo.InstalledUICulture);
			Console.WriteLine(message);

			message = StringTable.GetString(
				"TESTALL",
				CultureInfo.InstalledUICulture);
			Console.WriteLine(message);

			message = StringTable.GetString(
				"VALIDATE",
				CultureInfo.InstalledUICulture);
			Console.WriteLine(message);

			message = StringTable.GetString(
				"HELP",
				CultureInfo.InstalledUICulture);
			Console.WriteLine(message);
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Validate the command line arguments.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		private static bool ValidateArguments(string[] arguments)
		{
			bool result = false;

			if (arguments.Length > 0)
			{
				string command;
				string url;

				if (arguments.Length < 2)
				{
					command = "standard";
					url = arguments[0];
				}
				else
				{
					command = arguments[0];
					url = arguments[1];
				}

				if (Commands.Contains(command))
				{
					if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
					{
						result = true;
					}
				}
			}

			return result;
		}
	}
}
