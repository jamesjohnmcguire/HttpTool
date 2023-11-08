﻿/////////////////////////////////////////////////////////////////////////////
// <copyright file="SiteTest.cs" company="James John McGuire">
// Copyright © 2016 - 2023 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Abot2.Core;
using Abot2.Crawler;
using Abot2.Poco;
using Common.Logging;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;

[assembly: CLSCompliant(false)]

namespace WebTools
{
	/// <summary>
	/// Provides support for web site testing.
	/// </summary>
	public class SiteTest : IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly string[] ServerErrors =
		{
			"A PHP Error was encountered", "A Database Error Occurred",
			"Parse error", "データベースエラーが発生しました"
		};

		private static readonly string[] IgnoreTypes =
		{
			"GIF", "JPG", "JPEG", "PDF", "PNG"
		};

		private static readonly ResourceManager StringTable = new (
			"WebTools.Resources", Assembly.GetExecutingAssembly());

		private readonly HttpManager client;

		private readonly IDictionary<string, string> cookies;

		private readonly IList<string> imagesChecked;

		private readonly IList<string> pagesCrawed;

		private int pageCount;

		private Uri baseUri;

		/// <summary>
		/// Initializes a new instance of the <see cref="SiteTest"/> class.
		/// </summary>
		public SiteTest()
		{
			pagesCrawed = new List<string>();
			imagesChecked = new List<string>();
			client = new HttpManager();
			cookies = new Dictionary<string, string>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SiteTest"/> class.
		/// </summary>
		/// <param name="tests">The document checks to use.</param>
		public SiteTest(DocumentChecks tests)
			: this()
		{
			Tests = tests;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SiteTest"/> class.
		/// </summary>
		/// <param name="tests">The document checks to use.</param>
		/// <param name="uri">The URI to check.</param>
		public SiteTest(DocumentChecks tests, Uri uri)
			: this(tests)
		{
			baseUri = uri;
		}

		/// <summary>
		/// Gets or sets a value indicating whether to save the page or not.
		/// </summary>
		/// <value>Indicates whether to save the page or not.</value>
		public bool SavePage { get; set; }

		/// <summary>
		/// Gets or sets the document checks being used.
		/// </summary>
		/// <value>The document checks being used.</value>
		public DocumentChecks Tests { get; set; }

		/// <summary>
		/// Clear the current console line.
		/// </summary>
		public static void ClearCurrentConsoleLine()
		{
			int currentLineCursor = Console.CursorTop;
			Console.SetCursorPosition(0, Console.CursorTop);
			Console.Write('.');
			Console.Write(new string(' ', Console.WindowWidth - 2));
			Console.SetCursorPosition(0, currentLineCursor);
		}

		/// <summary>
		/// Adds the cookie.
		/// </summary>
		/// <param name="cookie">The cookie.</param>
		public void AddCookie(string cookie)
		{
			if (!string.IsNullOrWhiteSpace(cookie))
			{
				string[] parts = cookie.Split('=');
				string name = parts[0];
				string value = parts[1];

				cookies.Add(name, value);
			}
		}

		/// <summary>
		/// Disposes the object resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Test the given URI.
		/// </summary>
		/// <param name="uri">The URI to test.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous
		/// soperation.</returns>
		public async Task Test(Uri uri)
		{
			pageCount = 0;
			baseUri = uri;
			string message;

			CrawlConfiguration crawlConfiguration = new ();

			crawlConfiguration.MaxConcurrentThreads = 4;
			crawlConfiguration.UserAgentString =
				"Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
				"AppleWebKit/537.36 (KHTML, like Gecko) " +
				"Chrome/60.0.3112.113 Safari/537.36 bot";
			crawlConfiguration.MaxPagesToCrawl = 0;
			crawlConfiguration.DownloadableContentTypes =
				"text/html, text/plain, image/jpeg, image/pjpeg, image/png";
			crawlConfiguration.CrawlTimeoutSeconds = 0;
			crawlConfiguration.HttpRequestTimeoutInSeconds = 120;
			crawlConfiguration.MinCrawlDelayPerDomainMilliSeconds = 1000;
			crawlConfiguration.IsRespectRobotsDotTextEnabled = false;
			crawlConfiguration.IsSendingCookiesEnabled = true;

			WebContentExtractor contentExtractor = new ();
			CookieContainer cookieContainer = new ();

			using PageRequester pageRequester = new (
				crawlConfiguration,
				contentExtractor,
				cookieContainer);

			foreach (var cookie in cookies)
			{
				pageRequester.AddCookie(uri, cookie.Key, cookie.Value);
			}

			HyperLinkParser parser = new ();

			using PoliteWebCrawler crawler = new (
				crawlConfiguration,
				null,
				null,
				null,
				pageRequester,
				parser,
				null,
				null,
				null);

			crawler.PageCrawlStarting += ProcessPageCrawlStarted;
			crawler.PageCrawlCompleted += ProcessPageCrawlCompleted;

			CrawlResult result = await
				crawler.CrawlAsync(baseUri).ConfigureAwait(false);

			if (result.ErrorOccurred)
			{
				message = StringTable.GetString(
					"CRAWL_COMPLETE_ERROR",
					CultureInfo.InstalledUICulture);

				Log.InfoFormat(
					CultureInfo.InvariantCulture,
					message,
					result.RootUri.AbsoluteUri,
					result.ErrorException.Message);
			}
			else
			{
				message = StringTable.GetString(
					"CRAWL_COMPLETE_NO_ERROR",
					CultureInfo.InstalledUICulture);

				Log.InfoFormat(
					CultureInfo.InvariantCulture,
					message,
					result.RootUri.AbsoluteUri);
			}

			message = StringTable.GetString(
				"TOTAL_PAGES",
				CultureInfo.InstalledUICulture);
			Log.InfoFormat(
				CultureInfo.InvariantCulture,
				message,
				pageCount.ToString(CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Disposes of disposable resources.
		/// </summary>
		/// <param name="disposing">Indicates whether disposing is taking
		/// place.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// dispose managed resources
				client.Dispose();
			}

			// free native resources
		}

		private static string GetAbsoluteUrlString(string baseUrl, string url)
		{
			var uri = new Uri(url, UriKind.RelativeOrAbsolute);
			if (!uri.IsAbsoluteUri)
			{
				uri = new Uri(new Uri(baseUrl), uri);
			}

			return uri.ToString();
		}

		private static bool IsCrawlError(CrawledPage crawledPage)
		{
			bool error = false;

			string message = string.Empty;
			string url = crawledPage.Uri.AbsoluteUri;

			if (null == crawledPage.HttpResponseMessage)
			{
				message = string.Format(
					CultureInfo.InvariantCulture,
					"HttpResponseMessage is null: {0}",
					url);
				Log.Error(CultureInfo.InvariantCulture, m => m(
					message));
			}

			if (null != crawledPage.HttpRequestException)
			{
				message = string.Format(
					CultureInfo.InvariantCulture,
					"HttpRequestException: {0}",
					crawledPage.HttpRequestException.ToString());
				Log.Error(CultureInfo.InvariantCulture, m => m(
					message));
			}

			if (crawledPage.HttpResponseMessage != null &&
				crawledPage.HttpResponseMessage.StatusCode !=
					HttpStatusCode.OK)
			{
				HttpResponseMessage response =
					crawledPage.HttpResponseMessage;
				string statusCode =
					response.StatusCode.ToString();

				message = StringTable.GetString(
					"KEY_PAIR",
					CultureInfo.InstalledUICulture);
				Log.InfoFormat(
					CultureInfo.InvariantCulture,
					message,
					statusCode,
					url);
			}

			if ((null != crawledPage.HttpRequestException) ||
				(null == crawledPage.HttpResponseMessage) ||
				(crawledPage.HttpResponseMessage.StatusCode !=
					HttpStatusCode.OK))
			{
				error = true;

				message = string.Format(
					CultureInfo.InvariantCulture,
					"Error: {0}",
					url);
				Log.Error(CultureInfo.InvariantCulture, m => m(
					message));
			}

			return error;
		}

		private bool CheckContentErrors(CrawledPage crawledPage)
		{
			bool result = true;

			if (Tests.HasFlag(DocumentChecks.ContentErrors))
			{
				string url = crawledPage.Uri.AbsoluteUri;

				if (!IgnoreTypes.Any(url.EndsWith))
				{
					string text = crawledPage.Content.Text;

					if (ServerErrors.Any(text.Contains))
					{
						result = false;

						string message = string.Format(
							CultureInfo.InvariantCulture,
							"Page contains error messages: {0}",
							crawledPage.Uri.AbsoluteUri);
						Log.Error(CultureInfo.InvariantCulture, m => m(
							message));
					}
				}
			}

			return result;
		}

		private bool CheckForEmptyContent(CrawledPage crawledPage)
		{
			bool hasContent = true;
			if (Tests.HasFlag(DocumentChecks.EmptyContent))
			{
				string text = crawledPage.Content.Text;

				if (string.IsNullOrEmpty(text))
				{
					hasContent = false;

					if ((null != crawledPage) &&
						(null != crawledPage.HttpResponseMessage))
					{
						string message = string.Format(
							CultureInfo.InvariantCulture,
							"Page had no content {0}",
							crawledPage.Uri.AbsoluteUri);
						Log.Error(CultureInfo.InvariantCulture, m => m(
							message));
						message = string.Format(
							CultureInfo.InvariantCulture,
							"Parent: {0}",
							crawledPage.ParentUri.AbsoluteUri);
						Log.Error(CultureInfo.InvariantCulture, m => m(
							message));
					}
				}
			}

			return hasContent;
		}

		private async Task<bool> CheckImages(CrawledPage crawledPage)
		{
			bool result = true;

			if (Tests.HasFlag(DocumentChecks.ImagesExist))
			{
				HtmlDocument agilityPackHtmlDocument = new ();
				agilityPackHtmlDocument.LoadHtml(crawledPage.Content.Text);

				HtmlAgilityPack.HtmlNodeCollection nodes =
					agilityPackHtmlDocument.DocumentNode.SelectNodes(
					@"//img[@src]");

				if (null != nodes)
				{
					foreach (var image in nodes)
					{
						var source = image.Attributes["src"];
						string contents = source.Value;

						if (!imagesChecked.Contains(contents))
						{
							string baseUrl = string.Format(
								CultureInfo.InvariantCulture,
								"{0}://{1}",
								crawledPage.Uri.Scheme,
								crawledPage.Uri.Host);
							string imageUrl =
								GetAbsoluteUrlString(baseUrl, source.Value);

							Uri uri = new (imageUrl);
							bool exists = await
								UrlExists(uri).ConfigureAwait(false);

							if (false == exists)
							{
								result = false;

								string message = string.Format(
									CultureInfo.InvariantCulture,
									"image missing: {0} in {1}",
									imageUrl,
									crawledPage.Uri.AbsoluteUri);
								Log.Error(CultureInfo.InvariantCulture, m => m(
									message));
							}

							imagesChecked.Add(source.Value);
						}
					}
				}
			}

			return result;
		}

		private bool CheckParseErrors(CrawledPage crawledPage)
		{
			bool result = true;

			if (Tests.HasFlag(DocumentChecks.ParseErrors))
			{
				HtmlDocument agilityPackHtmlDocument = new ();
				agilityPackHtmlDocument.LoadHtml(crawledPage.Content.Text);

				IEnumerable<HtmlAgilityPack.HtmlParseError> parseErrors =
					agilityPackHtmlDocument.ParseErrors;

				if (null != parseErrors)
				{
					foreach (HtmlAgilityPack.HtmlParseError error in
						parseErrors)
					{
						// Ignoring error "End tag </option> is not required"
						// as it doesn't really seem like a problem
						if (error.Code != HtmlParseErrorCode.TagNotClosed)
						{
							result = false;

							string message = string.Format(
								CultureInfo.InvariantCulture,
								"HtmlAgilityPack: {0} in {1} at line: {2}",
								error.Reason,
								crawledPage.Uri.AbsoluteUri,
								error.Line);
							Log.Error(CultureInfo.InvariantCulture, m => m(
								message));
						}
					}
				}
			}

			return result;
		}

		private void CheckHostsDifferent(CrawledPage crawledPage)
		{
			if (!crawledPage.Uri.Host.Equals(
				baseUri.Host, StringComparison.OrdinalIgnoreCase))
			{
				string parentUrl = crawledPage.ParentUri.AbsoluteUri;
				string message = string.Format(
					CultureInfo.InvariantCulture,
					"Warning: Switching hosts from {0}",
					parentUrl);
				Log.Error(CultureInfo.InvariantCulture, m => m(
					message));
			}
		}

		private void CheckRedirects(CrawledPage crawledPage)
		{
			if (Tests.HasFlag(DocumentChecks.Redirect))
			{
				if (null != crawledPage.HttpResponseMessage)
				{
					string requestUri =
						crawledPage.HttpRequestMessage.RequestUri.AbsoluteUri;

					string responseUri =
						crawledPage.Uri.AbsoluteUri;

					SiteTests.IsRedirect(requestUri, responseUri);

					if (crawledPage.RedirectedFrom != null)
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

		/// <summary>
		/// The process page crawl completed event handler.
		/// </summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="arguments">The event arguments.</param>
		private async void ProcessPageCrawlCompleted(
			object sender, PageCrawlCompletedArgs arguments)
		{
			using SemaphoreSlim semaphoreSlim = new (1, 1);

			await semaphoreSlim.WaitAsync().ConfigureAwait(false);

			try
			{
				try
				{
					bool hasContent = true;
					bool contentErrors = false;
					bool imagesCheck = true;
					bool parseErrors = false;
					bool problemsFound = false;
					bool w3validation = true;
					string message;

					if (arguments != null)
					{
						CrawledPage crawledPage = arguments.CrawledPage;
						string url = crawledPage.Uri.AbsoluteUri;

						CheckHostsDifferent(crawledPage);

						pagesCrawed.Add(url);

						problemsFound = IsCrawlError(crawledPage);

						CheckRedirects(crawledPage);

						// if page has content and
						// it's not one of types we're ignoring
						if (!IgnoreTypes.Any(url.ToUpperInvariant().EndsWith))
						{
							hasContent = CheckForEmptyContent(crawledPage);

							if (true == hasContent)
							{
								contentErrors = !CheckContentErrors(crawledPage);
								parseErrors = !CheckParseErrors(crawledPage);
								imagesCheck = await CheckImages(crawledPage).ConfigureAwait(false);
#if NETSTANDARD2_0
								bool contains = url.Contains("localhost");
#else
								bool contains = url.Contains(
									"localhost",
									StringComparison.OrdinalIgnoreCase);
#endif

								if (contains == false)
								{
									w3validation = await ValidateFromW3Org(
										crawledPage.Uri.ToString()).
										ConfigureAwait(false);
								}
							}

							SaveDocument(crawledPage);
						}
					}
				}
				catch (Exception exception) when
					(exception is ArgumentException ||
					exception is ArgumentNullException ||
					exception is ArgumentOutOfRangeException ||
					exception is FileNotFoundException ||
					exception is IOException ||
					exception is NotSupportedException ||
					exception is NullReferenceException ||
					exception is ObjectDisposedException ||
					exception is FormatException ||
					exception is TaskCanceledException ||
					exception is UnauthorizedAccessException ||
					exception is WebException)
				{
					Log.Error(CultureInfo.InvariantCulture, m => m(
						exception.ToString()));
				}
			}
			finally
			{
				semaphoreSlim.Release();
			}

			pageCount++;
		}

		/// <summary>
		/// The process page crawl started event handler.
		/// </summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="arguments">The event arguments.</param>
		private void ProcessPageCrawlStarted(
			object sender, PageCrawlStartingArgs arguments)
		{
			if (arguments != null)
			{
				PageToCrawl page = arguments.PageToCrawl;

				string message = string.Format(
					CultureInfo.InvariantCulture,
					"Checking: {0}",
					page.Uri.AbsolutePath);
				Log.Info(CultureInfo.InvariantCulture, m => m(
					message));
			}
		}

		private void SaveDocument(CrawledPage crawledPage)
		{
			if (true == SavePage)
			{
				string text = crawledPage.Content.Text;
				string[] parts = crawledPage.Uri.LocalPath.Split(
					new char[] { '/' });
				string path = parts.Last() + crawledPage.Uri.Query;
#if NETSTANDARD2_0
				path = path.Replace("?", "__");
#else
				path = path.Replace(
					"?", "__", StringComparison.OrdinalIgnoreCase);
#endif
				path = path.Replace('\\', '-');
				File.WriteAllText(path, text);
			}
		}

		private async Task<bool> UrlExists(Uri url)
		{
			bool result = false;

			try
			{
				HttpClient httpClient = client.Client;

				// Remove query parameters.
#if NET5_0_OR_GREATER
				int index = url.AbsoluteUri.IndexOf('?', StringComparison.Ordinal);
#else
				int index = url.AbsoluteUri.IndexOf('?');
#endif

				if (index != -1)
				{
					string cleanUri = url.AbsoluteUri.Substring(0, index);
					url = new Uri(cleanUri);
				}

				// Do only Head request to avoid download full file.
				using HttpRequestMessage message = new (HttpMethod.Head, url);

				HttpResponseMessage response =
					await httpClient.SendAsync(message).ConfigureAwait(false);

				result = response.IsSuccessStatusCode;
			}
			catch (Exception exception) when
				(exception is ArgumentException ||
				exception is ArgumentNullException ||
				exception is ArgumentOutOfRangeException ||
				exception is FileNotFoundException ||
				exception is FormatException ||
				exception is IOException ||
				exception is HttpRequestException ||
				exception is JsonSerializationException ||
				exception is NotImplementedException ||
				exception is NotSupportedException ||
				exception is ObjectDisposedException ||
				exception is System.Security.SecurityException ||
				exception is UnauthorizedAccessException ||
				exception is UriFormatException)
			{
				result = false;

				using SemaphoreSlim semaphoreSlim = new (1, 1);

				await semaphoreSlim.WaitAsync().ConfigureAwait(false);

				try
				{
					Log.Error("Exception Processing: " + url.AbsoluteUri);
					Log.Error(exception.ToString());
				}
				finally
				{
					semaphoreSlim.Release();
				}
			}

			return result;
		}

		private async Task<bool> ValidateFromW3Org(string url)
		{
			bool succesCode = true;

			if (Tests.HasFlag(DocumentChecks.W3cValidation))
			{
				string validator = string.Format(
					CultureInfo.InvariantCulture,
					"http://validator.w3.org/nu/?doc={0}&out=json",
					url);
				Uri uri = new (validator);
				string response =
					await client.RequestUriBody(uri).ConfigureAwait(false);

				if (!string.IsNullOrWhiteSpace(response))
				{
					PageValidationResult pageResults =
						JsonConvert.DeserializeObject<PageValidationResult>(
							response);
					IList<ValidationResult> results = pageResults.Messages;

					if (results != null)
					{
						foreach (ValidationResult result in results)
						{
							succesCode = false;

							string message = string.Format(
								CultureInfo.InvariantCulture,
								"{0} {1} Result: {2}:{3} line: {4} - {5}",
								"W3 Validation for page ",
								url,
								result.Type,
								result.SubType,
								result.LastLine,
								result.Message);
							Log.Error(CultureInfo.InvariantCulture, m => m(
								message));
						}
					}
				}
			}

			return succesCode;
		}
	}
}
