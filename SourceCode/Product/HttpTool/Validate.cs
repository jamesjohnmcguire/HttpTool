/////////////////////////////////////////////////////////////////////////////
// $Id: $
// <copyright file="Validate.cs" company="James John McGuire">
// Copyright © 2016 - 2018 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using CommandLine;

namespace HttpTool
{
	[Verb("validate", HelpText = "Run w3c HTML validation tests.")]
	public class Validate : Options
	{
	}
}
