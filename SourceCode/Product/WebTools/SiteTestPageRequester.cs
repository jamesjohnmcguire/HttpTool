using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Abot.Core;
using Abot.Poco;
using System.Net;
using System.Net.Http;
using System.Collections.Specialized;

namespace WebTools
{
	public class SiteTestPageRequester : PageRequester
	{
		public RestClient RestClient { get; set; }

		private static CrawlConfiguration crawlConfig = new CrawlConfiguration
		{
			UserAgentString = "Mozilla/5.0 (Windows NT 10.0; WOW64; " +
			"rv:53.0) Gecko/20100101 Firefox/53.0" };

		public SiteTestPageRequester(RestClient restClient) : base(crawlConfig)
		{
			RestClient = restClient;
		}

		public SiteTestPageRequester(CrawlConfiguration config) : base(config)
		{

		}

		public override CrawledPage MakeRequest(Uri uri,
			Func<CrawledPage, CrawlDecision> shouldDownloadContent)
		{
			if (uri == null)
				throw new ArgumentNullException("uri");

			CrawledPage crawledPage = new CrawledPage(uri);

			try
			{
				crawledPage.RequestStarted = DateTime.Now;
				HttpResponseMessage response =
					RestClient.RequestGetResponse(uri.AbsoluteUri);
				crawledPage.DownloadContentStarted = DateTime.Now;
				PageContent pageContent = new PageContent();
				Stream stream = response.Content.ReadAsStreamAsync().Result;
				MemoryStream memory = new MemoryStream();
				stream.CopyTo(memory);
				pageContent.Bytes = memory.ToArray();
				pageContent.Charset = response.Content.Headers.ContentType.CharSet;
				foreach(string contentEncoding in
					response.Content.Headers.ContentEncoding)
				{
					pageContent.Encoding = GetEncoding(contentEncoding);
				}

				if (null == pageContent.Encoding)
				{
					pageContent.Encoding = Encoding.UTF8;
				}

				pageContent.Text = response.Content.ReadAsStringAsync().Result;
				crawledPage.DownloadContentCompleted = DateTime.Now;

				// complete the page properties
				crawledPage.HttpWebRequest = (HttpWebRequest)WebRequest.Create(
					RestClient.RequestMessage.RequestUri);

				Byte[] byteArray =
					RestClient.Response.Content.ReadAsByteArrayAsync().Result;
				NameValueCollection myCol = new NameValueCollection();

				foreach (var pair in RestClient.Response.Headers)
				{
					myCol.Add(pair.Key, pair.Value.First<string>());
				}
				HttpWebResponseWrapper responseWrapper =
					new HttpWebResponseWrapper(RestClient.Response.StatusCode,
					RestClient.Response.Content.Headers.ContentType.ToString(),
					byteArray, myCol);
				responseWrapper.ResponseUri =
					RestClient.ResponseMessage.RequestUri;
				crawledPage.HttpWebResponse = responseWrapper;
				crawledPage.Content = pageContent;
			}
			catch (WebException exception)
			{
				crawledPage.WebException = exception;
			}
			catch (Exception exception)
			{
				throw;
			}

			return crawledPage;
		}

		protected virtual Encoding GetEncoding(string charset)
		{
			Encoding e = Encoding.UTF8;
			if (charset != null)
			{
				try
				{
					e = Encoding.GetEncoding(charset);
				}
				catch { }
			}

			return e;
		}
	}
}
