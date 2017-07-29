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
		private static string[] errors = { "A PHP Error was encountered",
			"A Database Error Occurred", "Parse error",
			"データベースエラーが発生しました" };
		private static string[] fourOhfourPages = { "brand/brand21.html",
			"brand/brand10.html", "brand/brand25.html", "brand/brand9.html",
			"brand/brand11.html", "brand/brand19.html", "brand/brand18.html",
			"brand/product5_0.html", "brand/brand23.html",
			"brand/brand123.html", "brand/brand8.html", "brand/brand6.html",
			"style_pickup.html", "brand/product14_27.html",
			"brand/product12_20150306003645m09m65.html",
			"brand/product12_2.html", "brand/product13_3.html",
			"brand/product559.html", "brand/product354.html",
			"brand/product25_0.html", "brand/product13_1.html",
			"brand/product297.html", "brand/product27_1.html",
			"brand/product13_20150801153318vdxxi8.html",
			"brand/product340.html", "brand/product157.html",
			"brand/product577.html", "news/news88.html",
			"brand/product11_31.html", "brand/product316.html",
			"brand/product12_13.html", "brand/product8_5.html",
			"news/news73.html", "brand/product53.html",
			"brand/product9_10.html", "brand/product11_26.html",
			"brand/product24_20150306004008vfcf2v.html",
			"brand/product7_5.html",
			"brand/product11_20150306003840kv61xi.html",
			"news/news67.html", "brand/product204.html",
			"brand/product12_2015030600364561xi86.html", "brand/lighting.html",
			"brand/product21_9.html", "brand/product500.html",
			"brand/product23_20150306004113evie2f.html",
			"brand/product13_20150801160506vda7i4.html",
			"brand/product11_201503060038400efx75.html",
			"brand/product22_20150306004104fm328e.html",
			"brand/product15_20150306004151m3cada.html",
			"brand/product30_201504161446044ekm32.html",
			"brand/product5_2015030600094662ak8k.html",
			"brand/product10_201503060037293icxk8.html",
			"brand/product8_20150306003908v31d1x.html",
			"brand/product13_20160327183859m95avx.html",
			"brand/product29_2015030600414070tf3c.html",
			"brand/product8_201503060039085avtvt.html",
			"brand/product5_2016022707594643caca.html",
			"brand/product14_201503060041295t79md.html",
			"brand/product15_20150516151721vvi139.html",
			"brand/product15_20150306004151vdav10.html",
			"brand/product30_201503060034299cx1x9.html",
			"brand/product11_201503060038400icmic.html", "news/news100.html",
			"news/news111.html", "brand/product906.html", "news/news105.html",
			"brand/product893.html", "brand/brand137_8.html",
			"item_login.php?id=34", "brand/product1164.html", "service",
			"item_login.php", "inquiry.html", "ssl.php.bak",
			"showroom.html", "brand/brand7.html", "brand/brand16.html",
			"company.html", "brand/brand31.html", "brand/brand5.html",
			"brand/product613.html",
			"brand/product11_20150306003840d6ca6f.html",
			"brand/product871.html", "brand/brand136_5.html",
			"brand/product987.html", "brand/product1172.html",
			"brand/product955.html", "brand/product880.html",
			"turri/product81", "ahura/product746", "ahura/product744",
			"ahura/product738", "ahura/product1026" };
		private int pageCount = 0;
		private IList<string> pagesCrawed = null;
		private bool showGood = false;

		public SiteTest()
		{
			pagesCrawed = new List<string>();
		}

		public void Test(string url)
		{
			pageCount = 0;
			//CrawlConfiguration crawlConfig =
			//	AbotConfigurationSectionHandler.LoadFromXml().Convert();
			PoliteWebCrawler crawler = new PoliteWebCrawler();
			crawler.PageCrawlCompletedAsync += ProcessPageCrawlCompleted;

			crawler.ShouldCrawlPage((pageToCrawl, crawlContext) =>
			{
				string page = pageToCrawl.Uri.Host +
					pageToCrawl.Uri.PathAndQuery + pageToCrawl.Uri.Fragment;
				CrawlDecision decision = new CrawlDecision { Allow = true };
				if (pagesCrawed.Contains(pageToCrawl.Uri.AbsolutePath))
					return new CrawlDecision { Allow = false,
						Reason = "Don't want to repeat crawled pages" };

				return decision;
			});

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
			Console.WriteLine("Total Pages: {0}", pageCount.ToString());
		}

		public void ProcessPageCrawlCompleted(object sender,
			PageCrawlCompletedArgs e)
		{
			CrawledPage crawledPage = e.CrawledPage;

			string page = crawledPage.Uri.Host +
				crawledPage.Uri.PathAndQuery + crawledPage.Uri.Fragment;
			pagesCrawed.Add(page);

			if (crawledPage.WebException != null ||
				crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
			{
				Console.ForegroundColor = ConsoleColor.Red;
			}

			if ((true == showGood) || (crawledPage.WebException != null) ||
				(crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK))
			{
				Console.WriteLine("{0}: {1}",
				crawledPage.HttpWebResponse.StatusCode,
				crawledPage.Uri.AbsoluteUri);
			}

			Console.ForegroundColor = ConsoleColor.White;

			string text = crawledPage.Content.Text;
			if ((string.IsNullOrEmpty(text)) &&
				((!crawledPage.Uri.AbsoluteUri.EndsWith(".jpg")) &&
				(!crawledPage.Uri.AbsoluteUri.EndsWith(".pdf"))))
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Page had no content {0}", crawledPage.Uri.AbsoluteUri);
				Console.WriteLine("Parent: {0}", crawledPage.ParentUri.AbsoluteUri);
				Console.ForegroundColor = ConsoleColor.White;
			}

			if (errors.Any(text.Contains))
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Page has errors: {0}", crawledPage.Uri.AbsoluteUri);
				Console.ForegroundColor = ConsoleColor.White;
			}

			if (fourOhfourPages.Any(text.Contains))
			{
				string output = fourOhfourPages.Single(text.Contains).ToString();
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Page has 404 link: {0}", crawledPage.Uri.AbsoluteUri);
				Console.WriteLine("404 link: {0}", output);
				Console.ForegroundColor = ConsoleColor.White;
			}
			//var htmlAgilityPackDocument = crawledPage.HtmlDocument; //Html Agility Pack parser
			//var angleSharpHtmlDocument = crawledPage.AngleSharpHtmlDocument; //AngleSharp parser
			pageCount++;
		}
	}
}
