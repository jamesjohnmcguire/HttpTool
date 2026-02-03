/////////////////////////////////////////////////////////////////////////////
// <copyright file="ErrorResponse.cs" company="James John McGuire">
// Copyright © 2016 - 2026 James John McGuire.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace WebTools
{
	using Newtonsoft.Json;

	/// <summary>
	/// Represents an error response.
	/// </summary>
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
