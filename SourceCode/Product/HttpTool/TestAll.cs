/////////////////////////////////////////////////////////////////////////////
// $Id$
// <copyright file="TestAll.cs" company="James John McGuire">
// Copyright © 2016 - 2018 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using CommandLine;

namespace HttpTool
{
	[Verb("testall", HelpText = "Run all tests.")]
	public class TestAll : Options
	{
	}
}
