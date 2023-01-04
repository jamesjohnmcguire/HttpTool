/////////////////////////////////////////////////////////////////////////////
// <copyright file="ValidationResult.cs" company="James John McGuire">
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
	/// Represents a validation result.
	/// </summary>
	public class ValidationResult
	{
		/// <summary>
		/// Gets or sets the type of validtion result.
		/// </summary>
		/// <value>The type of validtion result.</value>
		public string Type { get; set; }

		/// <summary>
		/// Gets or sets the sub-type of the valitions result.
		/// </summary>
		/// <value>The sub-type of the valitions result.</value>
		public string SubType { get; set; }

		/// <summary>
		/// Gets or sets the last line of the validation result.
		/// </summary>
		/// <value>The last line of the validation result.</value>
		public int LastLine { get; set; }

		/// <summary>
		/// Gets or sets the first column of the validation result.
		/// </summary>
		/// <value>The first column of the validation result.</value>
		public int FirstColumn { get; set; }

		/// <summary>
		/// Gets or sets the last column of the validation result.
		/// </summary>
		/// <value>The last column of the validation result.</value>
		public int LastColumn { get; set; }

		/// <summary>
		/// Gets or sets the message of the validation result.
		/// </summary>
		/// <value>The message of the validation result.</value>
		public string Message { get; set; }

		/// <summary>
		/// Gets or sets the extract of the validation result.
		/// </summary>
		/// <value>The extract of the validation result.</value>
		public string Extract { get; set; }

		/// <summary>
		/// Gets or sets the start of hilight of the validation result.
		/// </summary>
		/// <value>The start of hilight of the validation result.</value>
		public int HiliteStart { get; set; }

		/// <summary>
		/// Gets or sets the hilight length of the validation result.
		/// </summary>
		/// <value>The hilight length of the validation result.</value>
		public int HiliteLength { get; set; }
	}
}
