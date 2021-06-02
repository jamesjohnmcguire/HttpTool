/////////////////////////////////////////////////////////////////////////////
// <copyright file="WebClient.cs" company="James John McGuire">
// Copyright © 2016 - 2021 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Common.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace WebTools
{
	/// <summary>
	/// Represents a web client for communicating with web servers.
	/// </summary>
	public class WebClient : IDisposable, INotifyPropertyChanged
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly string[] ServerErrors =
		{
			"A PHP Error was encountered",
			"A Database Error Occurred",
			"Parse error",
			"データベースエラーが発生しました",
		};

		private readonly HttpClient client;
		private readonly CookieContainer cookieJar;
		private readonly IList<KeyValuePair<string, string>>
			defaultParameters = new List<KeyValuePair<string, string>>();

		private bool trySecondChance;

		/// <summary>
		/// Initializes a new instance of the <see cref="WebClient"/> class.
		/// </summary>
		public WebClient()
		{
			IncludeSourceInError = true;
			IsError = false;

			cookieJar = new CookieContainer();
			HttpClientHandler clientHandler = new ();
			clientHandler.CookieContainer = cookieJar;
			clientHandler.AllowAutoRedirect = true;
			clientHandler.CheckCertificateRevocationList = true;

			string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
				"AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 " +
				"Safari/537.36";

			client = new HttpClient(clientHandler);
			client.DefaultRequestHeaders.Add("User-Agent", userAgent);
			client.Timeout = TimeSpan.FromMinutes(2);
		}

		public WebClient(string host)
			: this()
		{
			Host = host;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RestClient"/> class
		/// with explicitly setting the host, client id and client secret key.
		/// </summary>
		/// <param name="headers">The additional headers to add.</param>
		/// <param name="buildNumber">The build number of the
		/// application.</param>
		/// <param name="logger">The logging object.</param>
		public WebClient(
			IList<MediaTypeWithQualityHeaderValue> headers)
			: this()
		{
			if (headers != null)
			{
				foreach (MediaTypeWithQualityHeaderValue header in headers)
				{
					client.DefaultRequestHeaders.Accept.Add(header);
				}
			}
		}

		/// <summary>
		/// An event that gets triggered when the property changes
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Gets the HttpClient.
		/// </summary>
		/// <value>The HttpClient.</value>
		public HttpClient Client
		{
			get { return client; }
		}

		/// <summary>
		/// Gets the list of default parameters for the client.
		/// </summary>
		/// <value>
		/// The list of default parameters for the client.
		/// </value>
		public IList<KeyValuePair<string, string>> DefaultParameters
		{
			get
			{
				return defaultParameters;
			}
		}

		/// <summary>
		/// Gets or sets the host server.
		/// </summary>
		/// <value>The host server.</value>
		public string Host { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to include the server body
		/// content in an error message.
		/// </summary>
		/// <value>Whether to include the server source in an error.</value>
		public bool IncludeSourceInError { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the server is in
		/// an error state.
		/// </summary>
		/// <value>Whether the server is in an error state.</value>
		public bool IsError { get; set; }

		public HttpRequestMessage RequestMessage { get; set; }

		public HttpResponseMessage Response { get; set; }

		public HttpRequestMessage ResponseMessage { get; set; }

		/// <summary>
		/// Add a cookie into the cookie jar.
		/// </summary>
		/// <param name="name">The name of the cookie.</param>
		/// <param name="value">The value of the cookie.</param>
		public void AddCookie(string name, string value)
		{
			Cookie cookie = new (name, value);
			cookieJar.Add(cookie);
		}

		/// <summary>
		/// Add a cookie into the cookie jar.
		/// </summary>
		/// <param name="name">The name of the cookie.</param>
		/// <param name="value">The value of the cookie.</param>
		public void AddCookie(Uri domain, string name, string value)
		{
			Cookie cookie = new (name, value);
			cookieJar.Add(domain, cookie);
		}

		/// <summary>
		/// Disposes the object resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public async Task<string> GetWebPage(string url)
		{
			Uri uri = new (url);

			string response = await Request(uri).ConfigureAwait(false);

			return response;
		}

		public async Task<string> PostFile(
			string endPoint, string fieldName, string filePath)
		{
			string response = null;

			if (!File.Exists(filePath))
			{
				Log.Error("PostFile File doesn't exist!: " + filePath);
			}
			else
			{
				Log.Info("PostFile Sending data: " + filePath);

				client.DefaultRequestHeaders.ConnectionClose = true;

				var stream = File.OpenRead(filePath);

				HttpContent fileStreamContent = new StreamContent(stream);

				using var formData = new MultipartFormDataContent();

				formData.Add(fileStreamContent, fieldName, filePath);
				HttpResponseMessage responseMessage =
					await client.PostAsync(endPoint, formData);

				if (!responseMessage.IsSuccessStatusCode)
				{
					Log.Error("PostFile failed");
				}

				response = await responseMessage.Content.ReadAsStringAsync().
					ConfigureAwait(false);

				string fileName = Path.GetFileName(filePath);
				string message = string.Format(
					"{0} - Server response: {1}",
					fileName,
					response);

				Log.Info(message);
			}

			return response;
		}

		public HttpResponseMessage RequestGetResponse(Uri uri)
		{
			HttpResponseMessage response = client.GetAsync(uri).Result;

			return response;
		}

		public string RequestGetResponseAsString(Uri uri)
		{
			string response = string.Empty;
			try
			{
				response = client.GetStringAsync(uri).Result;
			}
			catch (TaskCanceledException exception)
			{
				Log.Error(CultureInfo.InvariantCulture, m => m(
					exception.ToString()));

				if (false ==
					exception.CancellationToken.IsCancellationRequested)
				{
					Log.Error(CultureInfo.InvariantCulture, m => m(
						"likely a time out"));
				}
			}

			return response;
		}

		public async Task<string> Request(Uri requestUri)
		{
			string responseContent = null;

			try
			{
				if (requestUri != null)
				{
					string requestUrl = requestUri.AbsoluteUri;
					bool isComplete = Uri.IsWellFormedUriString(
						requestUrl, UriKind.Absolute);

					if (false == isComplete)
					{
						requestUrl = string.Format(
							CultureInfo.InvariantCulture,
							@"{0}{1}",
							Host,
							requestUrl);
					}

					responseContent =
						await GetResponse(requestUrl).ConfigureAwait(false);
				}
			}
			catch (Exception exception) when
				(exception is ArgumentException ||
				exception is ArgumentNullException ||
				exception is ArgumentOutOfRangeException ||
				exception is FileNotFoundException ||
				exception is FormatException ||
				exception is HttpRequestException ||
				exception is IOException ||
				exception is JsonException ||
				exception is NotSupportedException ||
				exception is ObjectDisposedException ||
				exception is TaskCanceledException ||
				exception is UnauthorizedAccessException)
			{
				IsError = true;

				Log.Error(CultureInfo.InvariantCulture, m => m(
					exception.ToString()));

				responseContent = SetExceptionResponse(exception);
			}

			return responseContent;
		}

		public string Request(
			HttpMethod method,
			Uri requestUri,
			IList<KeyValuePair<string, string>> parameters)
		{
			string responseContent = null;

			try
			{
				if (requestUri != null)
				{
					using FormUrlEncodedContent content = new (parameters);

					string requestUrl = requestUri.AbsoluteUri;
					bool isComplete = Uri.IsWellFormedUriString(
						requestUrl, UriKind.Absolute);

					if (false == isComplete)
					{
						requestUrl = string.Format(
						CultureInfo.InvariantCulture,
						@"{0}{1}",
						Host,
						requestUrl);
					}

					responseContent = GetResponse(method, requestUrl, content);
				}
			}
			catch (Exception exception) when (exception is ArgumentException ||
				exception is ArgumentNullException ||
				exception is ArgumentOutOfRangeException ||
				exception is FileNotFoundException ||
				exception is IOException ||
				exception is JsonSerializationException ||
				exception is NotSupportedException ||
				exception is ObjectDisposedException ||
				exception is System.FormatException ||
				exception is UnauthorizedAccessException)
			{
				IsError = true;

				Log.Error(CultureInfo.InvariantCulture, m => m(
					exception.ToString()));

				responseContent = SetExceptionResponse(exception);
			}

			return responseContent;
		}

		public string Request(
			HttpMethod method,
			string query,
			string[] keys,
			string[] values)
		{
			IList<KeyValuePair<string, string>> parameters =
				new List<KeyValuePair<string, string>>();

			var items = keys.Zip(values, (key, value) =>
				new { Key = key, Value = value });

			foreach (var item in items)
			{
				KeyValuePair<string, string> pair = new (item.Key, item.Value);
				parameters.Add(pair);
			}

			Uri uri = new (query);
			return Request(method, uri, parameters);
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

		protected void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

		private static string SetErrorResponse(
			string error, string description)
		{
			string response = string.Format(
				CultureInfo.InvariantCulture,
				"{{ {0},\"error_description\":\"{1}\" }}",
				error,
				description);

			return response;
		}

		private static string SetExceptionResponse(Exception exception)
		{
			string error = "\"error\":\"exception\"";
			string details = exception.ToString();

			string response = string.Format(
				CultureInfo.InvariantCulture,
				"{{ {0},\"error_description\":\"{1}\" }}",
				error,
				details);

			return response;
		}

		private async Task<string> GetResponse(string requestUrl)
		{
			string responseContent = null;
			IsError = false;

			try
			{
				Uri uri = new (requestUrl);

				responseContent = await client.GetStringAsync(uri).
						ConfigureAwait(false);

				if (ServerErrors.Any(responseContent.Contains))
				{
					IsError = true;
				}
			}
			catch (Exception exception) when (exception is ArgumentException ||
				exception is ArgumentNullException ||
				exception is ArgumentOutOfRangeException ||
				exception is IOException ||
				exception is FileNotFoundException ||
				exception is FormatException ||
				exception is HttpRequestException ||
				exception is NotSupportedException ||
				exception is NullReferenceException ||
				exception is ObjectDisposedException ||
				exception is System.Net.WebException ||
				exception is UnauthorizedAccessException)
			{
				IsError = true;

				Log.Error(CultureInfo.InvariantCulture, m => m(
					exception.ToString()));
			}

			return responseContent;
		}

		private string GetResponse(
			HttpMethod method,
			string requestUrl,
			FormUrlEncodedContent content)
		{
			string responseContent;

			try
			{
				IsError = false;
				Response = null;

				if (method == HttpMethod.Post)
				{
					Uri uri = new (requestUrl);

					// save this as clients may want know this
					RequestMessage = new HttpRequestMessage();
					RequestMessage.RequestUri = uri;
					RequestMessage.Method = method;

					Response = client.PostAsync(uri, content).Result;
					int statusCode = (int)Response.StatusCode;

					// We want to handle redirects ourselves so that we can
					// determine the final redirect Location (via header)
					if (statusCode >= 300 && statusCode <= 399)
					{
						var redirectUri = Response.Headers.Location;
						if (!redirectUri.IsAbsoluteUri)
						{
							string authority =
								RequestMessage.RequestUri.GetLeftPart(
									UriPartial.Authority);
							redirectUri = new Uri(authority + redirectUri);
						}

						string message = string.Format(
							CultureInfo.InvariantCulture,
							"Redirecting to {0}",
							redirectUri);

						Log.Info(CultureInfo.InvariantCulture, m => m(
							message));

						responseContent =
							GetResponse(method, requestUrl, content);
					}
					else if (!Response.IsSuccessStatusCode)
					{
						Log.Error(CultureInfo.InvariantCulture, m => m(
							"status code not ok"));
					}
					else
					{
						ResponseMessage = new HttpRequestMessage();
						ResponseMessage.RequestUri = new Uri(requestUrl);
						ResponseMessage.Method = method;
					}
				}

				responseContent = Response.Content.ReadAsStringAsync().Result;

				if (false == Response.IsSuccessStatusCode)
				{
					IsError = true;

					Log.ErrorFormat(
						CultureInfo.InvariantCulture,
						"error: {0}",
						Response.ReasonPhrase);
				}

				if (ServerErrors.Any(responseContent.Contains))
				{
					IsError = true;
				}
			}
			catch (Exception exception) when (exception is ArgumentException ||
				exception is ArgumentNullException ||
				exception is ArgumentOutOfRangeException ||
				exception is FileNotFoundException ||
				exception is IOException ||
				exception is JsonSerializationException ||
				exception is NotSupportedException ||
				exception is ObjectDisposedException ||
				exception is System.FormatException ||
				exception is UnauthorizedAccessException)
			{
				IsError = true;

				Log.Error(CultureInfo.InvariantCulture, m => m(
					exception.ToString()));

				responseContent = SetExceptionResponse(exception);
			}

			return responseContent;
		}
	}
}
