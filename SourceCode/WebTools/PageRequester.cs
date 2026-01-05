/////////////////////////////////////////////////////////////////////////////
// <copyright file="PageRequester.cs" company="James John McGuire">
// Copyright © 2016 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace WebTools
{
	using System;
	using System.Globalization;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using Abot2.Core;
	using Abot2.Poco;
	using Serilog;

	/// <summary>
	/// Represents a page request.
	/// </summary>
	/// <seealso cref="Abot2.Core.PageRequester" />
	public class PageRequester : Abot2.Core.PageRequester
	{
		private readonly CrawlConfiguration config;
		private readonly IWebContentExtractor contentExtractor;
		private readonly CookieContainer cookieContainer;
		private HttpClientHandler httpClientHandler;
		private HttpClient httpClient;

		/// <summary>
		/// Initializes a new instance of the <see cref="PageRequester"/> class.
		/// </summary>
		/// <param name="config">The configuration.</param>
		/// <param name="contentExtractor">The content extractor.</param>
		public PageRequester(
			CrawlConfiguration config,
			IWebContentExtractor contentExtractor)
			: base(config, contentExtractor)
		{
			this.config = config;
			this.contentExtractor = contentExtractor;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PageRequester"/> class.
		/// </summary>
		/// <param name="config">The configuration.</param>
		/// <param name="contentExtractor">The content extractor.</param>
		/// <param name="cookieContainer">The cookie container.</param>
		public PageRequester(
			CrawlConfiguration config,
			IWebContentExtractor contentExtractor,
			CookieContainer cookieContainer)
			: this(config, contentExtractor)
		{
			if (cookieContainer != null)
			{
				this.cookieContainer = cookieContainer;
			}
		}

		/// <summary>
		/// Add a cookie into the cookie jar.
		/// </summary>
		/// <param name="domain">The domain of the cookie.</param>
		/// <param name="name">The name of the cookie.</param>
		/// <param name="value">The value of the cookie.</param>
		public void AddCookie(Uri domain, string name, string value)
		{
			Cookie cookie = new (name, value);
			cookieContainer.Add(domain, cookie);
		}

		/// <summary>
		/// Makes the request asynchronous.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <returns>The crawled page.</returns>
		public override async Task<CrawledPage> MakeRequestAsync(Uri uri)
		{
			return await MakeRequestAsync(
				uri, (x) => new CrawlDecision { Allow = true }).
				ConfigureAwait(false);
		}

		/// <summary>
		/// Makes the request asynchronous.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <param name="shouldDownloadContent">Content of the should download.</param>
		/// <returns>The crawled page.</returns>
		/// <exception cref="System.ArgumentNullException">The missing uri.</exception>
		/// <exception cref="System.Net.Http.HttpRequestException">Server response was
		/// unsuccessful, returned [http {statusCode}].</exception>
		public override async Task<CrawledPage> MakeRequestAsync(
			Uri uri,
			Func<CrawledPage, CrawlDecision> shouldDownloadContent)
		{
			if (uri == null)
			{
				throw new ArgumentNullException(nameof(uri));
			}

			if (httpClient == null)
			{
				httpClientHandler = BuildHttpClientHandler(uri);
				httpClient = BuildHttpClient(httpClientHandler);
			}

			var crawledPage = new CrawledPage(uri);
			HttpResponseMessage response = null;
			try
			{
				crawledPage.RequestStarted = DateTime.Now;
				using (var requestMessage = BuildHttpRequestMessage(uri))
				{
					response = await httpClient.SendAsync(
						requestMessage, CancellationToken.None).ConfigureAwait(false);
				}

				var statusCode = Convert.ToInt32(
					response.StatusCode, CultureInfo.InvariantCulture);

				if (statusCode < 200 || statusCode > 399)
				{
					throw new HttpRequestException(
						$"Server response was unsuccessful, returned [http {statusCode}]");
				}
			}
			catch (HttpRequestException hre)
			{
				crawledPage.HttpRequestException = hre;
				Log.Debug("Error occurred requesting url [{0}] {@Exception}", uri.AbsoluteUri, hre);
			}
			catch (TaskCanceledException ex)
			{
				// https://stackoverflow.com/questions/10547895/how-can-i-tell-when-httpclient-has-timed-out
				crawledPage.HttpRequestException =
					new HttpRequestException("Request timeout occurred", ex);
				Log.Debug("Error occurred requesting url [{0}] {@Exception}", uri.AbsoluteUri, crawledPage.HttpRequestException);
			}
			catch (Exception e)
			{
				crawledPage.HttpRequestException =
					new HttpRequestException("Unknown error occurred", e);
				Log.Debug(
					"Error occurred requesting url [{0}] {@Exception}",
					uri.AbsoluteUri,
					crawledPage.HttpRequestException);

				throw;
			}
			finally
			{
				crawledPage.HttpRequestMessage = response?.RequestMessage;
				crawledPage.RequestCompleted = DateTime.Now;
				crawledPage.HttpResponseMessage = response;
				crawledPage.HttpClientHandler = httpClientHandler;

				try
				{
					if (response != null && shouldDownloadContent != null)
					{
						var shouldDownloadContentDecision = shouldDownloadContent(crawledPage);
						if (shouldDownloadContentDecision.Allow)
						{
							crawledPage.DownloadContentStarted = DateTime.Now;
							crawledPage.Content = await contentExtractor.GetContentAsync(response).ConfigureAwait(false);
							crawledPage.DownloadContentCompleted = DateTime.Now;
						}
						else
						{
							Log.Debug("Links on page [{0}] not crawled, [{1}]", crawledPage.Uri.AbsoluteUri, shouldDownloadContentDecision.Reason);
						}
					}
				}
				catch (Exception exception) when
					(exception is ArgumentNullException ||
					exception is ArgumentException ||
					exception is InvalidOperationException ||
					exception is NullReferenceException)
				{
					Log.Debug(
						"Error occurred finalizing requesting url [{0}] {@Exception}",
						uri.AbsoluteUri,
						exception);
				}
			}

			return crawledPage;
		}

		/// <summary>
		/// Builds the HTTP client handler.
		/// </summary>
		/// <param name="rootUri">The root URI.</param>
		/// <returns>The initialized HttpClientHandler.</returns>
		/// <exception cref="System.ArgumentNullException">The missing rootUri
		/// argument.</exception>
		protected override HttpClientHandler BuildHttpClientHandler(Uri rootUri)
		{
			if (rootUri == null)
			{
				throw new ArgumentNullException(nameof(rootUri));
			}

			var httpClientHandler = new HttpClientHandler
			{
				MaxAutomaticRedirections = config.HttpRequestMaxAutoRedirects,
				UseDefaultCredentials = config.UseDefaultCredentials
			};

			if (config.IsHttpRequestAutomaticDecompressionEnabled)
			{
				httpClientHandler.AutomaticDecompression =
					DecompressionMethods.GZip | DecompressionMethods.Deflate;
			}

			if (config.HttpRequestMaxAutoRedirects > 0)
			{
				httpClientHandler.AllowAutoRedirect = config.IsHttpRequestAutoRedirectsEnabled;
			}

			if (config.IsSendingCookiesEnabled)
			{
				httpClientHandler.CookieContainer = cookieContainer;
				httpClientHandler.UseCookies = true;
			}

			if (!config.IsSslCertificateValidationEnabled)
			{
				httpClientHandler.ServerCertificateCustomValidationCallback +=
					(sender, certificate, chain, sslPolicyErrors) => true;
			}

			if (config.IsAlwaysLogin)
			{
				// Added to handle redirects clearing auth headers which result in 401...
				// https://stackoverflow.com/questions/13159589/how-to-handle-authenticatication-with-httpwebrequest-allowautoredirect
				var cache = new CredentialCache();

				NetworkCredential credentials = new (
					config.LoginUser,
					config.LoginPassword);

				Uri uri = new ($"http://{rootUri.Host}");
				Uri uris = new ($"https://{rootUri.Host}");

				cache.Add(uri, "Basic", credentials);
				cache.Add(uris, "Basic", credentials);

				httpClientHandler.Credentials = cache;
			}

			return httpClientHandler;
		}
	}
}
