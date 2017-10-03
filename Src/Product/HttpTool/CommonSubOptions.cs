using System;
using CommandLine;
using CommandLine.Text;

namespace HttpTool
{
	public abstract class CommonSubOptions
	{
		[Option('c', "configfile", HelpText = "Use this config file.")]
		public bool ConfigFile { get; set; }

		[Option('l', "logfile", HelpText = "Log message to this log file.")]
		public bool LogFile { get; set; }

		[HelpVerbOption]
		public string GetUsage(string verb)
		{
			return HelpText.AutoBuild(this, verb);
		}
	}
}
