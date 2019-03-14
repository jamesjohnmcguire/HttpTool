using Abot.Crawler;
using Abot.Poco;
using DigitalZenWorks.Common.Utils;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebTools
{
	public class SiteTest
	{
		private RestClient client = null;

		private static string[] errors = { "A PHP Error was encountered",
			"A Database Error Occurred", "Parse error",
			"データベースエラーが発生しました" };

		private static string[] ignoreTypes =
			{ "GIF", "JPG", "JPEG", "PDF", "PNG" };

		private IList<string> imagesChecked = null;
		private int pageCount = 0;
		private IList<string> pagesCrawed = null;
		private bool showGood = false;
		private static object thisLock = new object();
		private Uri baseUri = null;

		public bool LogOn { get; set; }
		public bool SavePage { get; set; }
		public DocumentChecks Tests { get; set; }

		public SiteTest()
		{
			pagesCrawed = new List<string>();
			imagesChecked = new List<string>();
			LogOn = true;
			client = new RestClient();
		}

		public void Test(string url)
		{
			pageCount = 0;
			SiteTestPageRequester pageRequester = null;
			baseUri = new Uri(url);

			if (true == LogOn)
			{
				pageRequester = new SiteTestPageRequester(client);
			}

			PoliteWebCrawler crawler = new PoliteWebCrawler(
				null,
				null,
				null,
				null,
				pageRequester,
				null,
				null,
				null,
				null);

			crawler.PageCrawlStarting += ProcessPageCrawlStarted;
			crawler.PageCrawlCompletedAsync += ProcessPageCrawlCompleted;

			crawler.ShouldCrawlPage((pageToCrawl, crawlContext) =>
			{
				return CrawlPage(pageToCrawl);
			});

			if (true == LogOn)
			{
				Login("https://www.euro-casa.co.jp/mariner/product/27",
				//Login("https://www.euro-casa.co.jp/test/mariner/product/27",
				//Login("http://euro.localhost/mariner/product/27",
					"jamesjohnmcguire@gmail.com",
					"jamesjohnmcguire@gmail.com");
			}

			CrawlResult result = crawler.Crawl(baseUri);

			if (result.ErrorOccurred)
			{
				Console.WriteLine("Crawl of {0} completed with error: {1}",
					result.RootUri.AbsoluteUri, result.ErrorException.Message);
			}
			else
			{
				Console.WriteLine("Crawl of {0} completed without error.",
					result.RootUri.AbsoluteUri);
			}

			Console.WriteLine("Total Pages: {0}", pageCount.ToString());
		}

		public async void ProcessPageCrawlCompleted(
			object sender, PageCrawlCompletedArgs e)
		{
			try
			{
				bool hasContent = true;
				bool contentErrors = false;
				bool imagesCheck = true;
				bool parseErrors = false;
				bool problemsFound = false;
				bool w3validation = true;

				CrawledPage crawledPage = e.CrawledPage;
				string url = crawledPage.Uri.AbsoluteUri;

				if (!crawledPage.Uri.Host.Equals(baseUri.Host))
				{
					string parentUrl = crawledPage.ParentUri.AbsoluteUri;
					string message = string.Format(
						CultureInfo.InvariantCulture,
						"Warning: Switching hosts from {0}",
						parentUrl);
					WriteError(message);
				}
				else
				{
					pagesCrawed.Add(url);

					if (IsHttpError(crawledPage))
					{
						problemsFound = true;

						string message = string.Format(
							CultureInfo.InvariantCulture, "Error: {0}", url);
						WriteError(message);

						if (null == crawledPage.HttpWebResponse)
						{
							message = string.Format(
								CultureInfo.InvariantCulture,
								"crawledPage.HttpWebResponse is null: {0}",
								url);
							WriteError(message);
						}
						else
						{
							string statusCode =
							crawledPage.HttpWebResponse.StatusCode.ToString();
							Console.WriteLine("{0}: {1}", statusCode, url);
						}
					}
					else if (true == showGood)
					{
						Console.WriteLine(
							"{0}: {1}",
							crawledPage.HttpWebResponse.StatusCode.ToString(),
							url);
					}

					CheckRedirects(crawledPage);

					// if page has content and
					// it's not one of types we're ignoring
					if (!ignoreTypes.Any(url.ToUpperInvariant().EndsWith))
					{
						hasContent = CheckForEmptyContent(crawledPage);

						if (true == hasContent)
						{
							contentErrors = !CheckContentErrors(crawledPage);
							parseErrors = !CheckParseErrors(crawledPage);
							imagesCheck = CheckImages(crawledPage);

							if ((!IsHttpError(crawledPage)) &&
								(!url.Contains("localhost")))
							{
								w3validation = await ValidateFromW3Org(
									crawledPage.Uri.ToString()).
									ConfigureAwait(false);
							}
						}

						SaveDocument(crawledPage);
					}

					if ((problemsFound == true) || (hasContent == false) ||
						(contentErrors == true) || (parseErrors == true) ||
						(imagesCheck == false) || (w3validation == false))
					{
						string message = string.Format(
							CultureInfo.InvariantCulture,
							"Problems found on: {0} (from: {1})",
							url,
							crawledPage.ParentUri.AbsolutePath);
						WriteError(message);
					}
				}
			}
			catch (Exception exception)
			{
				WriteError(exception.ToString());
			}

			pageCount++;
		}

		public void ProcessPageCrawlStarted(object sender,
			PageCrawlStartingArgs e)
		{
			PageToCrawl page = e.PageToCrawl;

			string message = string.Format(
				CultureInfo.InvariantCulture,
				"Checking: {0}",
				page.Uri.AbsolutePath);
			WriteStatus(message);
		}

		private bool CheckContentErrors(CrawledPage crawledPage)
		{
			bool result = true;

			if (Tests.HasFlag(DocumentChecks.ContentErrors))
			{
				string url = crawledPage.Uri.AbsoluteUri;

				if (!ignoreTypes.Any(url.EndsWith))
				{
					string text = crawledPage.Content.Text;

					if (errors.Any(text.Contains))
					{
						result = false;

						string message = string.Format(
							CultureInfo.InvariantCulture,
							"Page contains error messages: {0}",
							crawledPage.Uri.AbsoluteUri);
						WriteError(message);
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

					if ((null != crawledPage) && (null != crawledPage.HttpWebResponse) &&
						(!crawledPage.HttpWebResponse.ContentType.Equals(
						"application/rss+xml; charset=UTF-8")))
					{
						string message = string.Format(
							CultureInfo.InvariantCulture,
							"Page had no content {0}",
							crawledPage.Uri.AbsoluteUri);
						WriteError(message);
						message = string.Format(
							CultureInfo.InvariantCulture,
							"Parent: {0}",
							crawledPage.ParentUri.AbsoluteUri);
						WriteError(message);
					}
				}
			}

			return hasContent;
		}

		private bool CheckImages(CrawledPage crawledPage)
		{
			bool result = true;

			if (Tests.HasFlag(DocumentChecks.ImagesExist))
			{
				HtmlDocument htmlAgilityPackDocument =
				crawledPage.HtmlDocument;
				HtmlAgilityPack.HtmlNodeCollection nodes =
					htmlAgilityPackDocument.DocumentNode.SelectNodes(
					@"//img[@src]");

				if (null != nodes)
				{
					foreach (HtmlAgilityPack.HtmlNode image in nodes)
					{
						HtmlAttribute source = image.Attributes["src"];
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

							bool exists = URLExists(imageUrl);

							if (false == exists)
							{
								result = false;

								string message = string.Format(
									CultureInfo.InvariantCulture,
									"image missing: {0} in {1}",
									imageUrl,
									crawledPage.Uri.AbsoluteUri);
								WriteError(message);
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
				HtmlDocument htmlAgilityPackDocument =
					crawledPage.HtmlDocument;
				//var angleSharpHtmlDocument =
				//	crawledPage.AngleSharpHtmlDocument;

				HtmlNode.ElementsFlags.Remove("option");
				IEnumerable<HtmlAgilityPack.HtmlParseError> parseErrors =
					htmlAgilityPackDocument.ParseErrors;

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
							WriteError(message);
						}
					}
				}
			}

			return result;
		}

		private void CheckRedirects(CrawledPage crawledPage)
		{
			if (Tests.HasFlag(DocumentChecks.Redirect))
			{
				string requestUri =
					crawledPage.HttpWebRequest.RequestUri.AbsoluteUri;

				if (null != crawledPage.HttpWebResponse)
				{
					string responseUri =
						crawledPage.HttpWebResponse.ResponseUri.AbsoluteUri;
					if (!requestUri.Equals(responseUri))
					{
						//This is a redirect
						ClearCurrentConsoleLine();
						Console.WriteLine("Redirected from:{0} to: {1}",
							crawledPage.HttpWebRequest.RequestUri.AbsoluteUri,
							crawledPage.HttpWebResponse.ResponseUri.AbsoluteUri);
					}
				}
			}
		}

		public static void ClearCurrentConsoleLine()
		{
			int currentLineCursor = Console.CursorTop;
			Console.SetCursorPosition(0, Console.CursorTop);
			Console.Write('.');
			Console.Write(new string(' ', Console.WindowWidth - 2));
			Console.SetCursorPosition(0, currentLineCursor);
		}

		private CrawlDecision CrawlPage(PageToCrawl page)
		{
			CrawlDecision doCrawl = new CrawlDecision();
			doCrawl.Allow = true;

			if (pagesCrawed.Contains(page.Uri.AbsolutePath))
			{
				doCrawl.Reason = "Don't want to repeat crawled pages";
				doCrawl.Allow = false;
			}

			return doCrawl;
		}

		private static string GetAbsoluteUrlString(string baseUrl, string url)
		{
			var uri = new Uri(url, UriKind.RelativeOrAbsolute);
			if (!uri.IsAbsoluteUri)
				uri = new Uri(new Uri(baseUrl), uri);
			return uri.ToString();
		}

		private static bool IsHttpError(CrawledPage crawledPage)
		{
			bool error = false;

			if ((null != crawledPage.WebException) ||
				((null != crawledPage.HttpWebResponse) &&
				(crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)))
			{
				error = true;
			}

			return error;
		}

		private void Login(string url, string username, string password)
		{
			string[] keys = { "mode", "id", "section", "section_id", "email",
				"email_check", "submit" };
			string[] values = { "login", "27", "brand", "6", username,
				password, "送信" };

			client.Request(HttpMethod.Post, url, keys, values);
		}

		private void SaveDocument(CrawledPage crawledPage)
		{
			if (true == SavePage)
			{
				string text = crawledPage.Content.Text;
				string[] parts = crawledPage.Uri.LocalPath.Split(
					new char[] { '/' });
				string path = parts.Last() + crawledPage.Uri.Query;
				path = path.Replace("?", "__");
				path = path.Replace('\\', '-');
				FileUtils.SaveFile(text, path);
			}
		}

		private static bool URLExists(string url)
		{
			bool result = true;
			HttpWebResponse response = null;

			try
			{
				WebRequest webRequest = WebRequest.Create(url);
				webRequest.Timeout = 1200; // miliseconds
				webRequest.Method = "HEAD";
				response = (HttpWebResponse)webRequest.GetResponse();
			}
			catch
			{
				result = false;
			}
			finally
			{
				if (response != null)
				{
					response.Close();
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
				string response =
					await client.Request(validator).ConfigureAwait(false);

				if (!string.IsNullOrWhiteSpace(response))
				{
					PageValidationResult pageResults =
						JsonConvert.DeserializeObject<PageValidationResult>(
							response);
					IList<ValidationResult> results = pageResults.Messages;

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
						WriteError(message);
					}
				}
			}

			return succesCode;
		}

		private static void WriteError(string message)
		{
			lock (thisLock)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				ClearCurrentConsoleLine();
				Console.WriteLine(message);
				Console.ForegroundColor = ConsoleColor.White;
			}
		}

		private static void WriteStatus(string message)
		{
			lock (thisLock)
			{
				ClearCurrentConsoleLine();
				Console.Write(message);
			}
		}
	}
}