/////////////////////////////////////////////////////////////////////////////
// <copyright file="Token.cs" company="James John McGuire">
// Copyright © 2016 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json;

namespace WebTools
{
	/// <summary>
	/// Represents a server response token.
	/// </summary>
	public class Token : ServerResponse
	{
		/// <summary>
		/// Gets or sets the access token.
		/// </summary>
		/// <value>The access token.</value>
		[JsonProperty("access_token")]
		public string AccessToken { get; set; }

		/// <summary>
		/// Gets or sets the time when the token expires.
		/// </summary>
		/// <value>The time when the token expires.</value>
		[JsonProperty("expires_in ")]
		public string ExpiresIn { get; set; }

		/// <summary>
		/// Gets or sets the refresh token.
		/// </summary>
		/// <value>The refresh token.</value>
		[JsonProperty("refresh_token")]
		public string RefreshToken { get; set; }

		/// <summary>
		/// Gets or sets the scope of the token.
		/// </summary>
		/// <value>The scope of the token.</value>
		[JsonProperty("scope")]
		public string Scope { get; set; }

		/// <summary>
		/// Gets or sets the token type.
		/// </summary>
		/// <value>The token type.</value>
		[JsonProperty("token_type")]
		public string TokenType { get; set; }
	}
}
