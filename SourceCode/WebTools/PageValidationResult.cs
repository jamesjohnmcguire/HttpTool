/////////////////////////////////////////////////////////////////////////////
// <copyright file="PageValidationResult.cs" company="James John McGuire">
// Copyright © 2016 - 2021 James John McGuire. All Rights Reserved.
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
		public Uri Url { get; set; }

		public IList<ValidationResult> Messages { get; set; }
	}
}
