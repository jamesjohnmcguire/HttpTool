using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebTools;

namespace HttpTool
{
	internal class Program
	{
		static int Main(string[] args)
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
					Console.WriteLine("Starting...");
					SiteTest tester = new SiteTest();
					tester.SavePage = true;
					tester.Test(arguments[0]);
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.ToString());
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
			Console.WriteLine("usage: HttpTool <URL>");
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Summary for ValidateArguments.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		protected internal bool ValidateArguments(string[] arguments)
		{
			bool result = false;

			// Ensure we have a URL
			if (arguments.Length > 0)
			{
				result = true;
			}

			return result;
		}
	}
}
