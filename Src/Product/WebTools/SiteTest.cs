using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Abot.Core;
using Abot.Crawler;
using Abot.Poco;

namespace WebTools
{
	public class SiteTest
	{
		public static void Test(string url)
		{
			//CrawlConfiguration crawlConfig =
			//	AbotConfigurationSectionHandler.LoadFromXml().Convert();
			PoliteWebCrawler crawler = new PoliteWebCrawler();
			crawler.PageCrawlCompletedAsync += ProcessPageCrawlCompleted;

			Uri uri = new Uri(url);
			//This is synchronous, it will not go to the next line until the
			//crawl has completed
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
		}

		public static void ProcessPageCrawlCompleted(object sender,
			PageCrawlCompletedArgs e)
		{
			CrawledPage crawledPage = e.CrawledPage;

			if (crawledPage.WebException != null ||
				crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
			{
				Console.ForegroundColor = ConsoleColor.Red;
			}

			Console.WriteLine("{0}: {1}",
				crawledPage.HttpWebResponse.StatusCode,
				crawledPage.Uri.AbsoluteUri);

			Console.ForegroundColor = ConsoleColor.White;

			string text = crawledPage.Content.Text;
			if ((string.IsNullOrEmpty(text)) &&
				(!crawledPage.Uri.AbsoluteUri.EndsWith(".jpg")))
			{
				Console.WriteLine("Page had no content {0}", crawledPage.Uri.AbsoluteUri);
			}

			//var htmlAgilityPackDocument = crawledPage.HtmlDocument; //Html Agility Pack parser
			//var angleSharpHtmlDocument = crawledPage.AngleSharpHtmlDocument; //AngleSharp parser
		}
	}
}
