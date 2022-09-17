/////////////////////////////////////////////////////////////////////////////
// <copyright file="Program.cs" company="James John McGuire">
// Copyright © 2016 - 2022 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Common.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using WebTools;

[assembly: CLSCompliant(true)]

namespace HttpTool
{
	internal class Program
	{
		private static readonly string[] Commands =
		{
			"agilitypack", "empty", "enhanced", "help", "images", "redirects",
			"standard", "testall", "validate"
		};

		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly ResourceManager StringTable =
			new ("HttpTool.Resources", Assembly.GetExecutingAssembly());

		private static FileVersionInfo GetAssemblyInformation()
		{
			FileVersionInfo fileVersionInfo = null;

			// Bacause single file apps have no assemblies, get the information
			// from the process.
			Process process = Process.GetCurrentProcess();

			string location = process.MainModule.FileName;

			if (!string.IsNullOrWhiteSpace(location))
			{
				fileVersionInfo = FileVersionInfo.GetVersionInfo(location);
			}

			return fileVersionInfo;
		}

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

		private static string GetVersion()
		{
			string assemblyVersion = string.Empty;

			FileVersionInfo fileVersionInfo = GetAssemblyInformation();

			if (fileVersionInfo != null)
			{
				assemblyVersion = fileVersionInfo.FileVersion;
			}

			return assemblyVersion;
		}

		private static void LogInitialization()
		{
			string applicationDataDirectory = @"DigitalZenWorks\HttpTool";
			string baseDataDirectory = Environment.GetFolderPath(
				Environment.SpecialFolder.ApplicationData,
				Environment.SpecialFolderOption.Create) + @"\" +
				applicationDataDirectory;

			string logFilePath = baseDataDirectory + "\\HttpTool.log";
			string outputTemplate =
				"[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] " +
				"{Message:lj}{NewLine}{Exception}";

			LoggerConfiguration configuration = new ();
			LoggerSinkConfiguration sinkConfiguration = configuration.WriteTo;
			sinkConfiguration.Console(LogEventLevel.Verbose, outputTemplate);
			sinkConfiguration.File(
				logFilePath, LogEventLevel.Verbose, outputTemplate);
			Serilog.Log.Logger = configuration.CreateLogger();

			LogManager.Adapter =
				new Common.Logging.Serilog.SerilogFactoryAdapter();
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
				LogInitialization();
				string version = GetVersion();

				Log.Info("Starting HttpTools Version: " + version);

				result = ValidateArguments(arguments);

				if (true == result)
				{
					string command = GetCommand(arguments);
					string url = GetUrl(arguments);
					DocumentChecks tests = GetTests(command);

					using SiteTest tester = new (tests);

					string message = StringTable.GetString(
						"RUNNING_TESTS",
						CultureInfo.InstalledUICulture);
					Log.InfoFormat(CultureInfo.CurrentCulture, message, url);

					Uri uri = new (url);
					tester.Test(uri);
				}
				else
				{
					ShowHelp(null);
				}
			}
			catch (Exception exception)
			{
				Log.Error(CultureInfo.InvariantCulture, m => m(
					exception.ToString()));

				throw;
			}

			return result;
		}

		private static void ShowHelp(string additionalMessage)
		{
			FileVersionInfo fileVersionInfo = GetAssemblyInformation();

			if (fileVersionInfo != null)
			{
				string assemblyVersion = fileVersionInfo.FileVersion;
				string companyName = fileVersionInfo.CompanyName;
				string copyright = fileVersionInfo.LegalCopyright;
				string name = fileVersionInfo.FileName;

				string header = string.Format(
					CultureInfo.CurrentCulture,
					"{0} {1} {2} {3}",
					name,
					assemblyVersion,
					copyright,
					companyName);
				Log.Info(header);
			}

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
