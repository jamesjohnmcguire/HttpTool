/////////////////////////////////////////////////////////////////////////////
// <copyright file="HttpManager.cs" company="James John McGuire">
// Copyright © 2016 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Common.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
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
	/// Represents an extended HTTP client for communicating with web servers.
	/// </summary>
	public class HttpManager : IDisposable, INotifyPropertyChanged
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly HttpClient client;
		private readonly CookieContainer cookieJar;
		private readonly IList<KeyValuePair<string, string>>
			defaultParameters = new List<KeyValuePair<string, string>>();

		private ServerMessage error;
		private bool trySecondChance;

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="HttpManager"/> class.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage(
			"Reliability",
			"CA2000:Dispose objects before losing scope",
			Justification = "HttpClientHandler is exceptional case")]
		public HttpManager()
		{
			HttpClientHandler clientHandler = new ();

			clientHandler.AllowAutoRedirect = true;
			clientHandler.CheckCertificateRevocationList = true;

			cookieJar = new CookieContainer();
			clientHandler.CookieContainer = cookieJar;

			client = new HttpClient(clientHandler);

			client.Timeout = TimeSpan.FromMinutes(2);
			string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
				"AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 " +
				"Safari/537.36";
			client.DefaultRequestHeaders.Add("User-Agent", userAgent);

			IsError = false;
		}

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="HttpManager"/> class.
		/// </summary>
		/// <param name="host">The default host URI.</param>
		public HttpManager(string host)
			: this()
		{
			Host = host;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpManager"/>
		/// class with with headers and default parameters.
		/// </summary>
		/// <param name="headers">The additional headers to add.</param>
		/// <param name="defaultParameters">A list of default parameters for
		/// the client to use on each call to the server.</param>
		public HttpManager(
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
		/// Gets or sets an error message.
		/// </summary>
		/// <value>An error message.</value>
		public ServerMessage Error { get => error; set => error = value; }

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
		/// Is valid json method.
		/// </summary>
		/// <param name="test">The string to test.</param>
		/// <returns>A value indicating whether the text is valid json
		/// or not.</returns>
		public static bool IsValidJson(string test)
		{
			bool isValid = false;

			try
			{
				ServerMessage response =
					JsonConvert.DeserializeObject<ServerMessage>(test);

				isValid = true;
			}
			catch (JsonException)
			{
			}

			return isValid;
		}

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
		/// Submit an HTTP request.
		/// </summary>
		/// <param name="method">The HTTP method to use.</param>
		/// <param name="uri">The URI to request.</param>
		/// <param name="body">The body of parameters to send.</param>
		/// <returns>The response message of the request.</returns>
		public virtual async Task<string> Request(
			HttpMethod method,
			string url,
			string body)
		{
			string responseContent = null;

			try
			{
				if (url != null && body != null)
				{
					Uri uri = new (url);

					StringContent content =
						new (body, Encoding.UTF8, "application/json");

					content.Headers.ContentType =
						new ("application/json");

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
					}

					responseContent = await GetResponse(
						method, requestUrl, content).ConfigureAwait(false);

					if (trySecondChance == true)
					{
						trySecondChance = false;

						responseContent = await GetResponse(
							method, requestUrl, content).ConfigureAwait(false);
					}
				}
			}
			catch (Exception exception) when
				(exception is ArgumentException ||
				exception is ArgumentNullException ||
				exception is ArgumentOutOfRangeException ||
				exception is FileNotFoundException ||
				exception is IOException ||
				exception is JsonException ||
				exception is NotSupportedException ||
				exception is ObjectDisposedException ||
				exception is System.FormatException ||
				exception is TaskCanceledException ||
				exception is UnauthorizedAccessException)
			{
				IsError = true;

				Log.Error(exception.ToString());

				responseContent = SetExceptionResponse(exception);
			}
			catch (Exception exception)
			{
				IsError = true;

				Log.Error(exception.ToString());

				throw;
			}

			return responseContent;
		}

		/// <summary>
		/// Submit an HTTP request.
		/// </summary>
		/// <param name="method">The HTTP method to use.</param>
		/// <param name="uri">The URI to request.</param>
		/// <param name="parameters">The parameters to send.</param>
		/// <returns>The response message of the request.</returns>
		public virtual async Task<string> Request(
			HttpMethod method,
			Uri uri,
			IList<KeyValuePair<string, string>> parameters)
		{
			string responseContent = null;

			try
			{
				if (uri != null && parameters != null)
				{
					using FormUrlEncodedContent content = new (parameters);

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
					}

					responseContent = await GetResponse(
						method, requestUrl, content).ConfigureAwait(false);

					if (trySecondChance == true)
					{
						trySecondChance = false;

						responseContent = await GetResponse(
							method, requestUrl, content).ConfigureAwait(false);
					}
				}
			}
			catch (Exception exception) when
				(exception is ArgumentException ||
				exception is ArgumentNullException ||
				exception is ArgumentOutOfRangeException ||
				exception is FileNotFoundException ||
				exception is IOException ||
				exception is JsonException ||
				exception is NotSupportedException ||
				exception is ObjectDisposedException ||
				exception is System.FormatException ||
				exception is TaskCanceledException ||
				exception is UnauthorizedAccessException)
			{
				IsError = true;

				Log.Error(exception.ToString());

				responseContent = SetExceptionResponse(exception);
			}
			catch (Exception exception)
			{
				IsError = true;

				Log.Error(exception.ToString());

				throw;
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
		public async Task<string> Request(
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
			return await
				Request(method, uri, parameters).ConfigureAwait(false);
		}

		/// <summary>
		/// Requests the body of the given URI.
		/// </summary>
		/// <param name="uri">The uri of web page.</param>
		/// <returns>The response message of the request.</returns>
		public async Task<string> RequestUriBody(Uri uri)
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
				exception is FormatException ||
				exception is HttpRequestException ||
				exception is InvalidOperationException ||
				exception is ObjectDisposedException ||
				exception is TaskCanceledException)
			{
				IsError = true;

				Log.Error(exception.ToString());

				responseContent = SetExceptionResponse(exception);
			}

			return responseContent;
		}

		/// <summary>
		/// Request the URI response.
		/// </summary>
		/// <param name="uri">The URI of the request.</param>
		/// <returns>The response of the request.</returns>
		public HttpResponseMessage RequestUriResponse(Uri uri)
		{
			HttpResponseMessage response = client.GetAsync(uri).Result;

			return response;
		}

		/// <summary>
		/// Request the URI response as a string.
		/// </summary>
		/// <param name="uri">The URI of the request.</param>
		/// <returns>The response of the request.</returns>
		public string RequestUriResponseAsString(Uri uri)
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
		/// Upload a file to the server.
		/// </summary>
		/// <param name="endPoint">The end point API to sent file to.</param>
		/// <param name="fieldName">The name of form field.</param>
		/// <param name="fileName">The filename to send.</param>
		/// <param name="parameters">The parameters to send.</param>
		/// <returns>A message body of the response.</returns>
		public virtual async Task<string> UploadFile(
			string endPoint,
			string fieldName,
			string fileName,
			IList<KeyValuePair<string, string>> parameters)
		{
			string response = null;

			if (!File.Exists(fileName))
			{
				Log.Error("UploadFile File doesn't exist!: " + fileName);
			}
			else
			{
				Log.Info("UploadFile Sending data: " + fileName);
			}

			string requestUrl = string.Format(
				CultureInfo.InvariantCulture,
				@"{0}{1}",
				Host,
				endPoint);

			// make up the form data
			using (MultipartFormDataContent content = new ())
			{
				content.Headers.ContentType.MediaType = "multipart/form-data";

				// add the file
				using FileStream fileStream = new (fileName, FileMode.Open);
				using StreamContent stream = new (fileStream);
				content.Add(stream, fieldName, fileName);

				IList<StringContent> contents =
					new List<StringContent>();

				try
				{
					if (parameters != null)
					{
						foreach (KeyValuePair<string, string> keypair in
							parameters)
						{
							// gets disposed in finally
#pragma warning disable CA2000 // Dispose objects before losing scope
							StringContent parameter = new (keypair.Value);
#pragma warning restore CA2000 // Dispose objects before losing scope
							contents.Add(parameter);

							content.Add(parameter, keypair.Key);
						}
					}

					Uri uri = new (requestUrl);

					using HttpResponseMessage httpResponseMessage =
						await Client.PostAsync(
							uri, content).ConfigureAwait(false);
					response = await httpResponseMessage.Content.
						ReadAsStringAsync().ConfigureAwait(false);
				}
				finally
				{
					foreach (StringContent param in contents.ToList())
					{
						// contents.Remove(param);
						param.Dispose();
					}
				}
			}

			return response;
		}

		/// <summary>
		/// Posts a file.
		/// </summary>
		/// <param name="endPoint">The end point to send to.</param>
		/// <param name="fieldName">The name of field.</param>
		/// <param name="filePath">The path of the file.</param>
		/// <returns>The response of the request.</returns>
		public async Task<string> UploadFile(
			string endPoint, string fieldName, string filePath)
		{
			string response = null;

			if (!File.Exists(filePath))
			{
				Log.Error("UploadFile File doesn't exist!: " + filePath);
			}
			else
			{
				Log.Info("UploadFile Sending data: " + filePath);

				client.DefaultRequestHeaders.ConnectionClose = true;

				var stream = File.OpenRead(filePath);

				using HttpContent fileStreamContent =
					new StreamContent(stream);

				using var formData = new MultipartFormDataContent();

				formData.Add(fileStreamContent, fieldName, filePath);

				Uri uri = new (endPoint);
				HttpResponseMessage responseMessage = await client.PostAsync(
					uri, formData).ConfigureAwait(false);

				if (!responseMessage.IsSuccessStatusCode)
				{
					Log.Error("UploadFile failed");
				}

				response = await responseMessage.Content.ReadAsStringAsync().
					ConfigureAwait(false);

				string fileName = Path.GetFileName(filePath);
				string message = string.Format(
					CultureInfo.InvariantCulture,
					"{0} - Server response: {1}",
					fileName,
					response);

				Log.Info(message);
			}

			return response;
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
				exception is FileNotFoundException ||
				exception is FormatException ||
				exception is HttpRequestException ||
				exception is InvalidOperationException ||
				exception is IOException ||
				exception is NotSupportedException ||
				exception is NullReferenceException ||
				exception is ObjectDisposedException ||
				exception is System.Net.WebException ||
				exception is UnauthorizedAccessException)
			{
				if (exception is HttpRequestException ||
					exception is IOException ||
					exception is System.Net.WebException ||
					exception is UnauthorizedAccessException)
				{
					trySecondChance = true;
				}

				IsError = true;

				Log.Error(CultureInfo.InvariantCulture, m => m(
					exception.ToString()));
			}

			return responseContent;
		}

		private async Task<string> GetResponse(
			HttpMethod method,
			string requestUrl,
			FormUrlEncodedContent content)
		{
			string responseContent = null;
			IsError = false;

			try
			{
				if (method == HttpMethod.Post)
				{
					HttpResponseMessage response = null;
					Uri uri = new (requestUrl);

					// save this as clients may want know this
					RequestMessage = new HttpRequestMessage();
					RequestMessage.RequestUri = uri;
					RequestMessage.Method = method;

					using (response = await client.PostAsync(uri, content).
						ConfigureAwait(false))
					{
						responseContent = await
							response.Content.ReadAsStringAsync().
							ConfigureAwait(false);

						int statusCode = (int)response.StatusCode;

						// We want to handle redirects ourselves so that we can
						// determine the final redirect Location (via header)
						if (statusCode >= 300 && statusCode <= 399)
						{
							var redirectUri = response.Headers.Location;
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

							Log.Info(CultureInfo.InvariantCulture, m => m(
								message));

							responseContent = await
								GetResponse(method, requestUrl, content).
								ConfigureAwait(false);
						}
						else if (!ResponseMessage.IsSuccessStatusCode)
						{
							Log.Error(CultureInfo.InvariantCulture, m => m(
								"status code not ok"));
						}
					}

					responseContent = await
						GetResponse(method, requestUrl, content).
						ConfigureAwait(false);

					if (false == response.IsSuccessStatusCode)
					{
						IsError = true;

						responseContent = HandleError(response, responseContent);

						Log.ErrorFormat(
							CultureInfo.InvariantCulture,
							"error: {0}",
							ResponseMessage.ReasonPhrase);
					}
				}
			}
			catch (Exception exception) when (exception is ArgumentException ||
				exception is ArgumentNullException ||
				exception is ArgumentOutOfRangeException ||
				exception is FileNotFoundException ||
				exception is FormatException ||
				exception is HttpRequestException ||
				exception is InvalidOperationException ||
				exception is IOException ||
				exception is JsonSerializationException ||
				exception is NullReferenceException ||
				exception is NotSupportedException ||
				exception is ObjectDisposedException ||
				exception is System.Net.WebException ||
				exception is UnauthorizedAccessException)
			{
				IsError = true;

				Log.Error(exception.ToString());

				if (exception is HttpRequestException ||
					exception is IOException ||
					exception is System.Net.WebException ||
					exception is UnauthorizedAccessException)
				{
					trySecondChance = true;
				}

				responseContent = SetExceptionResponse(exception);
			}

			return responseContent;
		}

		private async Task<string> GetPostResponse(
			Uri uri,
			StringContent content)
		{
			string responseContent = null;
			IsError = false;

			using HttpResponseMessage response =
				await client.PostAsync(uri, content).ConfigureAwait(false);

			responseContent = await response.Content.ReadAsStringAsync().
				ConfigureAwait(false);

			if (response.IsSuccessStatusCode == false)
			{
				IsError = true;

				Log.Error("status code not ok");

				responseContent = HandleError(response, responseContent);

				Log.ErrorFormat(
					CultureInfo.InvariantCulture,
					"error: {0}",
					ResponseMessage.ReasonPhrase);
			}
			else
			{
				int statusCode = (int)response.StatusCode;

				// We want to handle redirects ourselves so that we can
				// determine the final redirect Location (via header)
				if (statusCode >= 300 && statusCode <= 399)
				{
					var redirectUri = response.Headers.Location;
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

					Log.Info(message);

					responseContent = await GetPostResponse(uri, content).
						ConfigureAwait(false);
				}
			}

			return responseContent;
		}

		private async Task<string> GetResponse(
			HttpMethod method,
			string requestUrl,
			StringContent content)
		{
			string responseContent = null;
			IsError = false;

			try
			{
				if (method == HttpMethod.Post)
				{
					Uri uri = new (requestUrl);

					responseContent = await GetPostResponse(uri, content).
						ConfigureAwait(false);
				}
			}
			catch (Exception exception) when (exception is ArgumentException ||
				exception is ArgumentNullException ||
				exception is ArgumentOutOfRangeException ||
				exception is FileNotFoundException ||
				exception is FormatException ||
				exception is HttpRequestException ||
				exception is InvalidOperationException ||
				exception is IOException ||
				exception is JsonSerializationException ||
				exception is NullReferenceException ||
				exception is NotSupportedException ||
				exception is ObjectDisposedException ||
				exception is System.Net.WebException ||
				exception is UnauthorizedAccessException)
			{
				IsError = true;

				Log.Error(exception.ToString());

				if (exception is HttpRequestException ||
					exception is IOException ||
					exception is System.Net.WebException ||
					exception is UnauthorizedAccessException)
				{
					trySecondChance = true;
				}

				responseContent = SetExceptionResponse(exception);
			}

			return responseContent;
		}

		private async Task<string> GetResponse(
			HttpMethod method,
			string requestUrl,
			IList<KeyValuePair<string, string>> parameters)
		{
			string responseContent = null;
			IsError = false;

			using FormUrlEncodedContent content = new (parameters);

			responseContent = await GetResponse(method, requestUrl, content).
				ConfigureAwait(false);

			return responseContent;
		}

		private string HandleError(
			HttpResponseMessage response, string content)
		{
			string errorResponse;
			IsError = true;

			Log.Error(response.ReasonPhrase);

			bool validJson = IsValidJson(content);

			if (validJson == true)
			{
				errorResponse = content;

				error = JsonConvert.DeserializeObject<ServerMessage>(content);
			}
			else
			{
				string errorStatus = response.StatusCode.ToString();
				string details = JsonConvert.SerializeObject(content);

				string json = "{{ \"error\":" +
				"\"{0}\",\"error_description\":\"{1}\"}}";
				errorResponse = string.Format(
					CultureInfo.InvariantCulture,
					json,
					errorStatus,
					details);

				error = JsonConvert.DeserializeObject<ServerMessage>(
					errorResponse);
			}

			return errorResponse;
		}
	}
}
