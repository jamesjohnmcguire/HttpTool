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

namespace WebTools
{
	public class SiteTest
	{
		private RestClient client = null;
		private static string[] errors = { "A PHP Error was encountered",
			"A Database Error Occurred", "Parse error",
			"データベースエラーが発生しました" };
		private IList<string> imagesChecked = null;
		private int pageCount = 0;
		private IList<string> pagesCrawed = null;
		private bool showGood = false;

		public SiteTest()
		{
			pagesCrawed = new List<string>();
			imagesChecked = new List<string>();
			client = new RestClient();
		}

		public void Temp()
		{
			string response = client.GetRequest("http://dzw.localhost");
		}
		public void Test(string url)
		{
			pageCount = 0;
			//CrawlConfiguration crawlConfig =
			//	AbotConfigurationSectionHandler.LoadFromXml().Convert();
			PoliteWebCrawler crawler = new PoliteWebCrawler();
			crawler.PageCrawlStarting += ProcessPageCrawlStarted;
			crawler.PageCrawlCompletedAsync += ProcessPageCrawlCompleted;

			CrawlDecision doCrawl = new CrawlDecision();
			doCrawl.Allow = true;
			CrawlDecision dontCrawl = new CrawlDecision();
			dontCrawl.Allow = false;
			dontCrawl.Reason = "Don't want to repeat crawled pages";

			crawler.ShouldCrawlPage((pageToCrawl, crawlContext) =>
			{
				if (pagesCrawed.Contains(pageToCrawl.Uri.AbsolutePath))
				{
					return dontCrawl;
				}

				return doCrawl;
			});

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
				pagesCrawed.Add(crawledPage.Uri.AbsolutePath);

				if (crawledPage.WebException != null ||
					crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
				{
					Console.ForegroundColor = ConsoleColor.Red;
				}

				if ((true == showGood) || (crawledPage.WebException != null) ||
					(crawledPage.HttpWebResponse.StatusCode !=
					HttpStatusCode.OK))
				{
					string url = string.Empty;
					if (!string.IsNullOrEmpty(crawledPage.Uri.AbsoluteUri))
					{
						url = crawledPage.Uri.AbsoluteUri;
					}

					if (null == crawledPage.HttpWebResponse)
					{
						string message = string.Format(
							"crawledPage.HttpWebResponse is null: {0}", url);
						WriteError(message);
					}
					else
					{
						Console.WriteLine("{0}: {1}",
							crawledPage.HttpWebResponse.StatusCode, url);
					}

				}

				Console.ForegroundColor = ConsoleColor.White;
				if ((!crawledPage.Uri.AbsoluteUri.EndsWith(".jpg")) &&
					(!crawledPage.Uri.AbsoluteUri.EndsWith(".pdf")))
				{
					string text = crawledPage.Content.Text;

					if (string.IsNullOrEmpty(text))
					{
						Console.ForegroundColor = ConsoleColor.Red;
						string message = string.Format(
							"Page had no content {0}",
							crawledPage.Uri.AbsoluteUri);
						WriteError(message);
						message = string.Format("Parent: {0}",
							crawledPage.ParentUri.AbsoluteUri);
						WriteError(message);
					}
					else
					{
						var htmlAgilityPackDocument = crawledPage.HtmlDocument;
						//var angleSharpHtmlDocument = crawledPage.AngleSharpHtmlDocument;

						CheckContentErrors(crawledPage,
							htmlAgilityPackDocument, text);
						CheckImages(crawledPage, htmlAgilityPackDocument);

						ValidateFromW3Org(crawledPage.Uri.ToString());
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

			string message = string.Format("Checking: {0}",
				page.Uri.AbsolutePath);
			WriteStatus(message);
		}

		private static void CheckContentErrors(CrawledPage crawledPage,
			HtmlDocument document, string pageContent)
		{
			if (errors.Any(pageContent.Contains))
			{
				string message = string.Format("Page has errors: {0}",
					crawledPage.Uri.AbsoluteUri);
				WriteError(message);
			}

			//HtmlNode.ElementsFlags.Remove("option");

			IEnumerable<HtmlAgilityPack.HtmlParseError> parseErrors =
				document.ParseErrors;
			if (null != parseErrors)
			{
				foreach (HtmlAgilityPack.HtmlParseError error in parseErrors)
				{
					//HtmlParseErrorCode.TagNotClosed:
					if (!error.Reason.Equals(
						"End tag </option> is not required"))
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

		private void CheckImages(CrawledPage crawledPage,
			HtmlDocument document)
		{
			HtmlAgilityPack.HtmlNodeCollection nodes =
				document.DocumentNode.SelectNodes(@"//img[@src]");

			foreach (HtmlAgilityPack.HtmlNode image in nodes)
			{
				var source = image.Attributes["src"];
				if (!imagesChecked.Contains(source.Value))
				{
					if (!source.Value.Equals("/cms/upimg/kagu/"))
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
		}

		public static void ClearCurrentConsoleLine()
		{
			int currentLineCursor = Console.CursorTop;
			Console.SetCursorPosition(0, Console.CursorTop);
			Console.Write(new string(' ', Console.WindowWidth));
			Console.SetCursorPosition(0, currentLineCursor);
		}

		private void Login(string url, string username, string password)
		{
			client.AddCookie("EuroCasaEmail", "jamesjohnmcguire@gmail.com");

			IList<KeyValuePair<string, string>> parameters =
				new List<KeyValuePair<string, string>>();

			KeyValuePair<string, string> pair =
				new KeyValuePair<string, string>("email", username);
			parameters.Add(pair);

			pair =
				new KeyValuePair<string, string>("email_check", password);
			parameters.Add(pair);

			string response =  client.Request(HttpMethod.Post,
				@"https://www.euro-casa.co.jp/mariner/product/27", parameters);
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
			string response = client.GetRequest(validator);

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
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(message);
			Console.ForegroundColor = ConsoleColor.White;
		}

		private static void WriteStatus(string message)
		{
			ClearCurrentConsoleLine();
			//Console.SetCursorPosition(0, Console.CursorTop);
			Console.Write(message);
			//Console.WriteLine(message);
		}
	}
}
