/////////////////////////////////////////////////////////////////////////////
// <copyright file="PageValidationResult.cs" company="James John McGuire">
// Copyright © 2016 - 2020 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTools
{
	public class PageValidationResult
	{
		public Uri Uri { get; set; }

		public IList<ValidationResult> Messages { get; }
	}
}
