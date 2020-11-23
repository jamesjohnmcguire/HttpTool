/////////////////////////////////////////////////////////////////////////////
// <copyright file="Options.cs" company="James John McGuire">
// Copyright © 2016 - 2020 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using CommandLine;
using System;

namespace HttpTool
{
	public class Options
	{
		[Value(1, HelpText = "The URL of the site to test.")]
		public Uri Url { get; set; }
	}
}