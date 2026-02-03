/////////////////////////////////////////////////////////////////////////////
// <copyright file="Program.cs" company="James John McGuire">
// Copyright © 2016 - 2026 James John McGuire.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

[assembly: System.CLSCompliant(true)]

namespace HttpTool
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Reflection;
	using System.Resources;
	using System.Threading.Tasks;
	using Common.Logging;
	using DigitalZenWorks.CommandLine.Commands;
	using Serilog;
	using Serilog.Configuration;
	using Serilog.Events;
	using WebTools;

	internal sealed class Program
	{
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

		private static IList<Command> GetCommands()
		{
			IList<Command> commands = new List<Command>();

			Command help = new ("help");
			help.Description = "Show this information";
			commands.Add(help);

			IList<CommandOption> options = new List<CommandOption>();

			CommandOption cookie = new ("c", "cookie", false);
			options.Add(cookie);

			Command agilityPack = new ("agilitypack", options, 0, "Run agility pack tests");
			commands.Add(agilityPack);

			Command empty = new ("empty", options, 0, "Run empty page tests");
			commands.Add(empty);

			Command enhanced = new ("enhanced", options, 0, "Run all enhanced tests");
			commands.Add(enhanced);

			Command images = new ("images", options, 0, "Run missing images tests");
			commands.Add(images);

			Command redirects = new ("redirects", options, 0, "Run redirect tests");
			commands.Add(redirects);

			Command standard = new ("standard", options, 0, "Run standard tests (default)");
			commands.Add(standard);

			Command testall = new ("testall", options, 0, "Run all tests");
			commands.Add(testall);

			Command validate = new ("validate", options, 0, "Run w3c HTML validation tests");
			commands.Add(validate);

			return commands;
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
			sinkConfiguration.Console(
				LogEventLevel.Verbose,
				outputTemplate,
				CultureInfo.InvariantCulture);
			sinkConfiguration.File(
				logFilePath,
				LogEventLevel.Verbose,
				outputTemplate,
				CultureInfo.InvariantCulture);
			Serilog.Log.Logger = configuration.CreateLogger();

			LogManager.Adapter =
				new Common.Logging.Serilog.SerilogFactoryAdapter();
		}

		private static async Task<int> Main(string[] arguments)
		{
			int returnCode = -1;

			bool resultCode = await Run(arguments).ConfigureAwait(false);

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
		private static async Task<bool> Run(string[] arguments)
		{
			bool result = false;

			try
			{
				LogInitialization();
				string version = GetVersion();

				Log.Info("Starting HttpTools Version: " + version);

				IList<Command> commands = GetCommands();

				CommandLineArguments commandLine = new (commands, arguments);

				arguments = UpdateArguments(arguments);

				if (commandLine.ValidArguments == false)
				{
					Log.Error(commandLine.ErrorMessage);
					ShowHelp(null);
				}
				else
				{
					Command command = commandLine.Command;

					string url = GetUrl(arguments);
					DocumentChecks tests = GetTests(command.Name);

					using SiteTest tester = new (tests);

					bool hasCookie = command.DoesOptionExist(
						"c", "cookie");

					if (hasCookie == true)
					{
						tester.AddCookie(command.Parameters[1]);
					}

					string message = StringTable.GetString(
						"RUNNING_TESTS",
						CultureInfo.InstalledUICulture);
					Log.InfoFormat(CultureInfo.CurrentCulture, message, url);

					Uri uri = new (url);
					await tester.Test(uri).ConfigureAwait(false);

					result = true;
				}
			}
			catch (Exception exception) when
				(exception is ArgumentNullException ||
				exception is ArgumentException ||
				exception is InvalidOperationException ||
				exception is NullReferenceException)
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

			Console.WriteLine(message);

			message = StringTable.GetString(
				"COOKIE",
				CultureInfo.InstalledUICulture);
			Console.WriteLine(message);
		}

		private static string[] UpdateArguments(string[] arguments)
		{
			if (arguments.Length > 0 && arguments.Length < 2)
			{
				string[] newArguments = new string[arguments.Length + 1];
				int index = arguments.Length;

				while (index >= 1)
				{
					newArguments[index] = arguments[index - 1];
				}

				arguments[0] = "standard";
			}

			return arguments;
		}
	}
}
