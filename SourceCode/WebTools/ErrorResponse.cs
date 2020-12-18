/////////////////////////////////////////////////////////////////////////////
// <copyright file="ErrorResponse.cs" company="James John McGuire">
// Copyright © 2016 - 2020 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json;

namespace WebTools
{
	public class ErrorResponse
	{
		/// <summary>
		/// Gets or sets the error code.
		/// </summary>
		/// <value>The error code or brief description.</value>
		[JsonProperty("error")]
		public string Error { get; set; }

		/// <summary>
		/// Gets or sets the error description.
		/// </summary>
		/// <value>The error description.</value>
		[JsonProperty("error_description")]
		public string ErrorDescription { get; set; }
	}
}
