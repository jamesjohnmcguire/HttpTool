/////////////////////////////////////////////////////////////////////////////
// <copyright file="SiteTests.cs" company="James John McGuire">
// Copyright © 2016 - 2024 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Common.Logging;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Resources;

namespace WebTools
{
	/// <summary>
	/// Manages automated site testing.
	/// </summary>
	public static class SiteTests
	{
		private static readonly string[] IgnoreTypes =
		{
			"GIF", "JPG", "JPEG", "PDF", "PNG"
		};

		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly string[] ServerErrors =
		{
			"A PHP Error was encountered", "A Database Error Occurred",
			"Parse error", "データベースエラーが発生しました"
		};

		private static readonly ResourceManager StringTable = new (
			"WebTools.Resources", Assembly.GetExecutingAssembly());

		/// <summary>
		/// Checks the content for errors.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <param name="pageContent">Content of the page.</param>
		/// <returns>A value indicating whether the content was free of
		/// errors or not.</returns>
		public static bool CheckContentErrors(Uri uri, string pageContent)
		{
			bool result = true;

			if (uri != null)
			{
				string url = uri.AbsoluteUri;

				bool isIgnoreType =
					IgnoreTypes.Any(url.ToUpperInvariant().EndsWith);

				if (isIgnoreType == false && pageContent != null)
				{
					bool isServerErrors =
						ServerErrors.Any(pageContent.Contains);

					if (isServerErrors == true)
					{
						result = false;

						string message = string.Format(
							CultureInfo.InvariantCulture,
							"Page contains error messages: {0}",
							uri.AbsoluteUri);
						Log.Error(message);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Checks for empty content.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <param name="parentUri">The parent URI.</param>
		/// <param name="response">The response.</param>
		/// <param name="pageContent">Content of the page.</param>
		/// <returns>A value indicating whether the content has content
		/// or not.</returns>
		public static bool CheckForEmptyContent(
			Uri uri,
			Uri parentUri,
			HttpResponseMessage response,
			string pageContent)
		{
			bool hasContent = true;
			string message;

			if (string.IsNullOrWhiteSpace(pageContent))
			{
				hasContent = false;

				if (response != null)
				{
					if (uri != null)
					{
						message = string.Format(
							CultureInfo.InvariantCulture,
							"Page had no content {0}",
							uri.AbsoluteUri);
						Log.Error(message);
					}

					if (parentUri != null)
					{
						message = string.Format(
							CultureInfo.InvariantCulture,
							"Parent: {0}",
							parentUri.AbsoluteUri);
						Log.Error(message);
					}
				}
			}

			return hasContent;
		}

		/// <summary>
		/// Checks if the hosts are different.
		/// </summary>
		/// <param name="originalUrl">The original URL.</param>
		/// <param name="uri">The URI.</param>
		/// <param name="parentUri">The parent URI.</param>
		public static void CheckHostsDifferent(
			Uri originalUrl, Uri uri, Uri parentUri)
		{
			string originalHost = originalUrl.Host;
			string host = uri.Host;

			if (!host.Equals(originalHost, StringComparison.OrdinalIgnoreCase))
			{
				string parentUrl = parentUri.AbsoluteUri;
				string message = string.Format(
					CultureInfo.InvariantCulture,
					"Warning: Switching hosts from {0}",
					parentUrl);
				Log.Error(message);
			}
		}

		/// <summary>
		/// Checks for parse errors.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <param name="pageContent">Content of the page.</param>
		/// <returns>A value indicating whether the content was free from
		/// parse errors or not.</returns>
		public static bool CheckParseErrors(Uri uri, string pageContent)
		{
			bool result = true;

			HtmlDocument agilityPackHtmlDocument = new ();
			agilityPackHtmlDocument.LoadHtml(pageContent);

			IEnumerable<HtmlParseError> parseErrors =
				agilityPackHtmlDocument.ParseErrors;

			if (null != parseErrors)
			{
				foreach (HtmlParseError error in parseErrors)
				{
					// Ignoring error "End tag </option> is not required"
					// as it doesn't really seem like a problem
					if (uri != null &&
						error.Code != HtmlParseErrorCode.TagNotClosed)
					{
						result = false;

						string message = string.Format(
							CultureInfo.InvariantCulture,
							"HtmlAgilityPack: {0} in {1} at line: {2}",
							error.Reason,
							uri.AbsoluteUri,
							error.Line);
						Log.Error(message);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Checks for redirects.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <param name="request">The request.</param>
		/// <param name="redirectedFrom">The redirected from.</param>
		public static void CheckRedirects(
			Uri uri,
			HttpRequestMessage request,
			object redirectedFrom)
		{
			if (request != null)
			{
				string requestUri = request.RequestUri.AbsoluteUri;

				if (uri != null)
				{
					string responseUri = uri.AbsoluteUri;

					if (responseUri != null)
					{
						IsRedirect(requestUri, responseUri);

						if (redirectedFrom != null)
						{
							// Special case.
							string message = StringTable.GetString(
								"REDIRECTED",
								CultureInfo.InstalledUICulture);
							Log.InfoFormat(
								CultureInfo.InvariantCulture,
								message,
								requestUri,
								responseUri);
						}
					}
				}
			}
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
