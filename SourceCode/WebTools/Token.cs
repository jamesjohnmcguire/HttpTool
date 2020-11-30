/////////////////////////////////////////////////////////////////////////////
// <copyright file="Token.cs" company="James John McGuire">
// Copyright © 2016 - 2020 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace WebTools
{
	public class Token : ServerResponse
	{
		public string access_token { get; set; }

		public string expires_in { get; set; }

		public string token_type { get; set; }

		public string scope { get; set; }

		public string refresh_token { get; set; }
	}
}