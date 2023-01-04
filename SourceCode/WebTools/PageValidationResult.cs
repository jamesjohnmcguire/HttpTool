/////////////////////////////////////////////////////////////////////////////
// <copyright file="PageValidationResult.cs" company="James John McGuire">
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
	/// Represents a page validation result.
	/// </summary>
	public class PageValidationResult
	{
		/// <summary>
		/// Gets or sets the URI of the page.
		/// </summary>
		/// <value>The URI of the page.</value>
		public Uri Url { get; set; }

		/// <summary>
		/// Gets a list of messages.
		/// </summary>
		/// <value>A list of messages.</value>
		public IList<ValidationResult> Messages { get; }
	}
}
