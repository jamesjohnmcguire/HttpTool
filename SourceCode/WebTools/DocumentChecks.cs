/////////////////////////////////////////////////////////////////////////////
// <copyright file="DocumentChecks.cs" company="James John McGuire">
// Copyright © 2016 - 2023 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTools
{
	/// <summary>
	/// Provides an enumeration for types of checks.
	/// </summary>
	[Flags]
	public enum DocumentChecks
	{
		/// <summary>
		/// No document checks
		/// </summary>
		None = 0,

		/// <summary>
		/// Basic document checks
		/// </summary>
		Basic = 1,

		/// <summary>
		/// Redirect checks
		/// </summary>
		Redirect = 2,

		/// <summary>
		/// Empty content checks
		/// </summary>
		EmptyContent = 4,

		/// <summary>
		/// Content error checks
		/// </summary>
		ContentErrors = 8,

		/// <summary>
		/// Parse error checks
		/// </summary>
		ParseErrors = 16,

		/// <summary>
		/// Image exists checks
		/// </summary>
		ImagesExist = 32,

		/// <summary>
		/// W3c validation checks
		/// </summary>
		W3cValidation = 64
	}
}
