/////////////////////////////////////////////////////////////////////////////
// <copyright file="Standard.cs" company="James John McGuire">
// Copyright © 2016 - 2019 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using CommandLine;

namespace HttpTool
{
	[Verb("standard", HelpText = "Run standard tests (default).")]
	public class Standard : Options
	{
	}
}
