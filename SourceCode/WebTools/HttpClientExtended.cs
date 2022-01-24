/////////////////////////////////////////////////////////////////////////////
// <copyright file="HttpClientExtended.cs" company="James John McGuire">
// Copyright © 2016 - 2022 James John McGuire. All Rights Reserved.
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
using System.Threading.Tasks;

namespace WebTools
{
	/// <summary>
	/// Represents an extended HTTP client for communicating with web servers.
	/// </summary>
	public class HttpClientExtended : IDisposable, INotifyPropertyChanged
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly HttpClient client;
		private readonly CookieContainer cookieJar;
		private readonly IList<KeyValuePair<string, string>>
			defaultParameters = new List<KeyValuePair<string, string>>();

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="HttpClientExtended"/> class.
		/// </summary>
		public HttpClientExtended()
		{
			HttpClientHandler clientHandler = new ();

			clientHandler.AllowAutoRedirect = true;
			clientHandler.CheckCertificateRevocationList = true;

			cookieJar = new CookieContainer();
			clientHandler.CookieContainer = cookieJar;

			client = new HttpClient(clientHandler);

			client.Timeout = TimeSpan.FromMinutes(2);
			string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
				"AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 " +
				"Safari/537.36";
			client.DefaultRequestHeaders.Add("User-Agent", userAgent);

			IsError = false;
		}

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="HttpClientExtended"/> class.
		/// </summary>
		/// <param name="host">The default host URI.</param>
		public HttpClientExtended(string host)
			: this()
		{
			Host = host;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpClientExtended"/>
		/// class with with headers and default parameters.
		/// </summary>
		/// <param name="headers">The additional headers to add.</param>
		/// <param name="defaultParameters">A list of default parameters for
		/// the client to use on each call to the server.</param>
		public HttpClientExtended(
			IList<MediaTypeWithQualityHeaderValue> headers,
			IList<KeyValuePair<string, string>> defaultParameters)
			: this()
		{
			if (headers != null)
			{
				foreach (MediaTypeWithQualityHeaderValue header in headers)
				{
					client.DefaultRequestHeaders.Accept.Add(header);
				}
			}

			this.defaultParameters = defaultParameters;
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
		/// Gets or sets a value indicating whether the server is in
		/// an error state.
		/// </summary>
		/// <value>Whether the server is in an error state.</value>
		public bool IsError { get; set; }

		/// <summary>
		/// Gets or sets the request message.
		/// </summary>
		/// <value>The request message.</value>
		public HttpRequestMessage RequestMessage { get; set; }

		/// <summary>
		/// Gets or sets the response message.
		/// </summary>
		/// <value>The response message.</value>
		public HttpResponseMessage ResponseMessage { get; set; }

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
		/// <param name="domain">The domain of the cookie.</param>
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

		/// <summary>
		/// Gets the response body content of the given uri.
		/// </summary>
		/// <param name="uri">The uri of web page.</param>
		/// <returns>A <see cref="Task{TResult}"/> representing the result
		/// of the asynchronous operation.</returns>
		public async Task<string> GetWebPage(Uri uri)
		{
			string response = await Request(uri).ConfigureAwait(false);

			return response;
		}

		/// <summary>
		/// Posts a file.
		/// </summary>
		/// <param name="endPoint">The end point to send to.</param>
		/// <param name="fieldName">The name of field.</param>
		/// <param name="filePath">The path of the file.</param>
		/// <returns>The response of the request.</returns>
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

		/// <summary>
		/// Get the request's response.
		/// </summary>
		/// <param name="uri">The URI of the request.</param>
		/// <returns>The response of the request.</returns>
		public HttpResponseMessage RequestGetResponse(Uri uri)
		{
			HttpResponseMessage response = client.GetAsync(uri).Result;

			return response;
		}

		/// <summary>
		/// Get the request's response as a string.
		/// </summary>
		/// <param name="uri">The URI of the request.</param>
		/// <returns>The response of the request.</returns>
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

		/// <summary>
		/// Requests the body of the given url.
		/// </summary>
		/// <param name="uri">The uri of web page.</param>
		/// <returns>The response message of the request.</returns>
		public async Task<string> Request(Uri uri)
		{
			string responseContent = null;

			try
			{
				uri = GetCompleteUri(uri);

				if (uri != null)
				{
					responseContent = await client.GetStringAsync(uri).
						ConfigureAwait(false);
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

		/// <summary>
		/// Submit an HTTP request.
		/// </summary>
		/// <param name="method">The request method.</param>
		/// <param name="requestUri">The request URI.</param>
		/// <param name="parameters">The request parameters.</param>
		/// <returns>The response message of the request.</returns>
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

		/// <summary>
		/// Submit an HTTP request.
		/// </summary>
		/// <param name="method">The request method.</param>
		/// <param name="query">The request query.</param>
		/// <param name="keys">The request keys.</param>
		/// <param name="values">The request values.</param>
		/// <returns>The response message of the request.</returns>
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

		/// <summary>
		/// The on property changed event handler.
		/// </summary>
		/// <param name="name">The property name.</param>
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

		private Uri GetCompleteUri(Uri uri)
		{
			if (uri != null)
			{
				string requestUrl = uri.AbsoluteUri;
				bool isComplete = Uri.IsWellFormedUriString(
					requestUrl, UriKind.Absolute);

				if (false == isComplete)
				{
					requestUrl = string.Format(
						CultureInfo.InvariantCulture,
						@"{0}{1}",
						Host,
						requestUrl);

					uri = new (requestUrl);
				}
			}

			return uri;
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
				ResponseMessage = null;

				if (method == HttpMethod.Post)
				{
					Uri uri = new (requestUrl);

					// save this as clients may want know this
					RequestMessage = new HttpRequestMessage();
					RequestMessage.RequestUri = uri;
					RequestMessage.Method = method;

					ResponseMessage = client.PostAsync(uri, content).Result;
					int statusCode = (int)ResponseMessage.StatusCode;

					// We want to handle redirects ourselves so that we can
					// determine the final redirect Location (via header)
					if (statusCode >= 300 && statusCode <= 399)
					{
						var redirectUri = ResponseMessage.Headers.Location;
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
					else if (!ResponseMessage.IsSuccessStatusCode)
					{
						Log.Error(CultureInfo.InvariantCulture, m => m(
							"status code not ok"));
					}
				}

				responseContent = ResponseMessage.Content.ReadAsStringAsync().Result;

				if (false == ResponseMessage.IsSuccessStatusCode)
				{
					IsError = true;

					Log.ErrorFormat(
						CultureInfo.InvariantCulture,
						"error: {0}",
						ResponseMessage.ReasonPhrase);
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
