/////////////////////////////////////////////////////////////////////////////
// <copyright file="SiteTests.cs" company="James John McGuire">
// Copyright © 2016 - 2023 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Common.Logging;
using System;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace WebTools
{
	/// <summary>
	/// Manages automated site testing.
	/// </summary>
	public static class SiteTests
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly ResourceManager StringTable = new (
			"WebTools.Resources", Assembly.GetExecutingAssembly());

		/// <summary>
		/// Determines whether the specified request URI is a redirect.
		/// </summary>
		/// <param name="requestUri">The request URI.</param>
		/// <param name="responseUri">The response URI.</param>
		/// <returns>
		///   <c>true</c> if the specified request URI is a redirect;
		///   otherwise, <c>false</c>.
		/// </returns>
		public static bool IsRedirect(string requestUri, string responseUri)
		{
			bool redirected = false;

			if (!string.IsNullOrWhiteSpace(requestUri) &&
				!string.IsNullOrWhiteSpace(responseUri))
			{
				if (!requestUri.Equals(
					responseUri, StringComparison.OrdinalIgnoreCase))
				{
					// This is a redirect
					string message = StringTable.GetString(
						"REDIRECTED",
						CultureInfo.InstalledUICulture);
					Log.InfoFormat(
						CultureInfo.InvariantCulture,
						message,
						requestUri,
						responseUri);

					redirected = true;
				}
			}

			return redirected;
		}

		/// <summary>
		/// Determines whether the specified request URI is a redirect.
		/// </summary>
		/// <param name="requestUri">The request URI.</param>
		/// <param name="responseUri">The response URI.</param>
		/// <returns>
		///   <c>true</c> if the specified request URI is a redirect;
		///   otherwise, <c>false</c>.
		/// </returns>
		public static bool IsRedirect(Uri requestUri, Uri responseUri)
		{
			bool redirected = false;

			if (requestUri != null && responseUri != null)
			{
				redirected = IsRedirect(
					requestUri.AbsoluteUri, responseUri.AbsoluteUri);
			}

			return redirected;
		}
	}
}
