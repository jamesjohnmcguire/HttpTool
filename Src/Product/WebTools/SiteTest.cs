using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Abot.Crawler;
using Abot.Poco;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using DigitalZenWorks.Common.Utils;

namespace WebTools
{
	public class SiteTest
	{
		private DocumentChecks checks = DocumentChecks.Basic |
			DocumentChecks.ContentErrors | DocumentChecks.EmptyContent |
			DocumentChecks.ImagesExist | DocumentChecks.ParseErrors |
			DocumentChecks.Redirect;
		private RestClient client = null;
		private static string[] errors = { "A PHP Error was encountered",
			"A Database Error Occurred", "Parse error",
			"データベースエラーが発生しました" };
		private static string[] ignoreTypes =
			{ "gif", "jpg", "jpeg", "pdf", "png" };
		private IList<string> imagesChecked = null;
		private int pageCount = 0;
		private IList<string> pagesCrawed = null;
		private bool showGood = false;
		private static Object thisLock = new Object();

		public bool SavePage { get; set; }

		public SiteTest()
		{
			pagesCrawed = new List<string>();
			imagesChecked = new List<string>();
			client = new RestClient();
		}

		public void Test(string url)
		{
			pageCount = 0;
			PoliteWebCrawler crawler = new PoliteWebCrawler();
			crawler.PageCrawlStarting += ProcessPageCrawlStarted;
			crawler.PageCrawlCompletedAsync += ProcessPageCrawlCompleted;

			crawler.ShouldCrawlPage((pageToCrawl, crawlContext) =>
			{
				return CrawlPage(pageToCrawl);
			});

			//Login("https://www.euro-casa.co.jp/mariner/product/27",
			//	"jamesjohnmcguire@gmail.com", "jamesjohnmcguire@gmail.com");

			Uri uri = new Uri(url);
			CrawlResult result = crawler.Crawl(uri);

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

		public void ProcessPageCrawlCompleted(object sender,
			PageCrawlCompletedArgs e)
		{
			try
			{
				CrawledPage crawledPage = e.CrawledPage;
				string url = crawledPage.Uri.AbsoluteUri;

				pagesCrawed.Add(url);

				if (IsHttpError(crawledPage))
				{
					string message = string.Format("Error: {0}", url);
					WriteError(message);

					if (null == crawledPage.HttpWebResponse)
					{
						message = string.Format(
							"crawledPage.HttpWebResponse is null: {0}", url);
						WriteError(message);
					}
					else
					{
						Console.WriteLine("{0}: {1}",
							crawledPage.HttpWebResponse.StatusCode, url);
					}
				}
				else if (true == showGood)
				{
					Console.WriteLine("{0}: {1}",
							crawledPage.HttpWebResponse.StatusCode, url);
				}

				CheckRedirects(crawledPage);
				bool hasContent = CheckForEmptyContent(crawledPage);

				// if page has content and it's not one of types we're ignoring
				if ((true == hasContent) && (!ignoreTypes.Any(url.EndsWith)))
				{
					CheckContentErrors(crawledPage);
					CheckParseErrors(crawledPage);
					CheckImages(crawledPage);
					//ValidateFromW3Org(crawledPage.Uri.ToString());
					SaveDocument(crawledPage);
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

			string message = string.Format("Checking: {0}",
				page.Uri.AbsolutePath);
			WriteStatus(message);
		}

		private void CheckContentErrors(CrawledPage crawledPage)
		{
			if (checks.HasFlag(DocumentChecks.ContentErrors))
			{
				string url = crawledPage.Uri.AbsoluteUri;

				if (!ignoreTypes.Any(url.EndsWith))
				{
					string text = crawledPage.Content.Text;

					if (errors.Any(text.Contains))
					{
						string message = string.Format("Page has errors: {0}",
							crawledPage.Uri.AbsoluteUri);
						WriteError(message);
					}
				}
			}
		}

		private bool CheckForEmptyContent(CrawledPage crawledPage)
		{
			bool hasContent = true;
			if (checks.HasFlag(DocumentChecks.EmptyContent) ||
				checks.HasFlag(DocumentChecks.ContentErrors) ||
				checks.HasFlag(DocumentChecks.ImagesExist) ||
				checks.HasFlag(DocumentChecks.ParseErrors) ||
				checks.HasFlag(DocumentChecks.W3cValidation))
			{
				string text = crawledPage.Content.Text;

				if (string.IsNullOrEmpty(text))
				{
					hasContent = false;

					string message = string.Format("Page had no content {0}",
						crawledPage.Uri.AbsoluteUri);
					WriteError(message);
					message = string.Format("Parent: {0}",
						crawledPage.ParentUri.AbsoluteUri);
					WriteError(message);
				}
			}

			return hasContent;
		}

		private void CheckImages(CrawledPage crawledPage)
		{
			HtmlDocument htmlAgilityPackDocument =
				crawledPage.HtmlDocument;
			HtmlAgilityPack.HtmlNodeCollection nodes =
				htmlAgilityPackDocument.DocumentNode.SelectNodes(
				@"//img[@src]");

			foreach (HtmlAgilityPack.HtmlNode image in nodes)
			{
				var source = image.Attributes["src"];
				if (!imagesChecked.Contains(source.Value))
				{
					string imageUrl = string.Format("{0}://{1}{2}",
						crawledPage.Uri.Scheme, crawledPage.Uri.Host,
						source.Value);

					bool exists = URLExists(imageUrl);

					if (false == exists)
					{
						string message = string.Format(
							"image missing: {0} in {1}",
							imageUrl, crawledPage.Uri.AbsoluteUri);
						WriteError(message);
					}

					imagesChecked.Add(source.Value);
				}
			}
		}

		private void CheckParseErrors(CrawledPage crawledPage)
		{
			if (checks.HasFlag(DocumentChecks.ParseErrors))
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
						if (error.Code != HtmlParseErrorCode.TagNotClosed)
						//if (!error.Reason.Equals(
						//	"End tag </option> is not required"))
						{
							string message = string.Format(
								"Page has error: {0} in {1} at line: {2}",
								error.Reason, crawledPage.Uri.AbsoluteUri,
								error.Line);
							WriteError(message);
						}
					}
				}
			}
		}

		private void CheckRedirects(CrawledPage crawledPage)
		{
			if (checks.HasFlag(DocumentChecks.Redirect))
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
			Console.Write(new string(' ', Console.WindowWidth));
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

		private static bool IsHttpError(CrawledPage crawledPage)
		{
			bool error = false;

			if (crawledPage.WebException != null ||
				crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
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

		private void ValidateFromW3Org(string url)
		{
			string validator = string.Format(
				"http://validator.w3.org/nu/?doc={0}&out=json", url);
			string response = client.RequestGetResponseAsString(validator);

			//IList<ValidationResult> results = JsonConvert.DeserializeObject<
			//	IList<ValidationResult>>(response);
			PageValidationResult pageResults = JsonConvert.DeserializeObject<
				PageValidationResult>(response);
			IList<ValidationResult> results = pageResults.Messages;

			foreach(ValidationResult result in results)
			{
				Console.WriteLine("{0}:{1} line: {2} - {3}", result.Type,
					result.SubType, result.LastLine, result.Message);
			}
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
