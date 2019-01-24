#define USE_HTTPWebRequest

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace WebTools
{
	public delegate void LogMessage(string level, string message);

	public class RestClient : INotifyPropertyChanged
	{
		private HttpClient client = null;
		private string clientId = null;
		private string clientSecret = null;
		private CookieContainer cookieJar = null;

		private static string[] errors = { "A PHP Error was encountered",
			"A Database Error Occurred", "Parse error",
			"データベースエラーが発生しました" };

		public string AccessToken { get; set; }
		public bool Authenticate { get; set; }
		public string Host { get; set; }
		public bool IncludeSourceInError { get; set; }
		public bool IsError { get; set; }
		public LogMessage Logger { get; set; }
		public string RefreshToken { get; set; }
		public string RefreshTokenEndPoint { get; set; }
		public HttpRequestMessage RequestMessage { get; set; }
		public HttpResponseMessage Response { get; set; }
		public HttpRequestMessage ResponseMessage { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public RestClient()
		{
			Authenticate = false;
			IncludeSourceInError = true;
			IsError = false;

			cookieJar = new CookieContainer();
			HttpClientHandler clientHandler = new HttpClientHandler();
			clientHandler.CookieContainer = cookieJar;
			clientHandler.AllowAutoRedirect = true;

			// actually doesn't seem to work
			//NetworkCredential credentials =
			//new NetworkCredential(clientId, clientSecret);
			//clientHandler.Credentials = credentials;

			string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
				"AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 " +
				"Safari/537.36";

			client = new HttpClient(clientHandler);
			client.DefaultRequestHeaders.Add("User-Agent", userAgent);
			client.Timeout = TimeSpan.FromMinutes(1);
		}

		public RestClient(string host, string clientId, string clientSecret)
			: this()
		{
			Host = host;
			this.clientId = clientId;
			this.clientSecret = clientSecret;
		}

		protected void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

		public void AddCookie(string name, string value)
		{
			Cookie cookie = new Cookie(name, value);
			cookieJar.Add(cookie);
		}

		public IList<KeyValuePair<string, string>> GetClientParameters()
		{
			IList<KeyValuePair<string, string>> parameters =
				new List<KeyValuePair<string, string>>();

			KeyValuePair<string, string> pair =
				new KeyValuePair<string, string>("client_id", clientId);
			parameters.Add(pair);

			pair =
				new KeyValuePair<string, string>("client_secret", clientSecret);
			parameters.Add(pair);

			return parameters;
		}

		public HttpResponseMessage RequestGetResponse(string url)
		{
			HttpResponseMessage response = client.GetAsync(url).Result;

			return response;
		}

		public string RequestGetResponseAsString(string url)
		{
			string response = string.Empty;
			try
			{
				response = client.GetStringAsync(url).Result;
			}
			catch (TaskCanceledException exception)
			{
				System.Diagnostics.Debug.WriteLine(exception.ToString());
				if (false ==
					exception.CancellationToken.IsCancellationRequested)
				{
					System.Diagnostics.Debug.WriteLine("likely a time out");
				}
			}
			return response;
		}

		public async Task<string> Request(string requestUrl)
		{
			string responseContent = null;

			try
			{
				bool isComplete =
					Uri.IsWellFormedUriString(requestUrl, UriKind.Absolute);
				if (false == isComplete)
				{
					requestUrl = string.Format(@"{0}{1}", Host, requestUrl);
				}

				DoAuthentication();

				responseContent =
					await GetResponse(requestUrl).ConfigureAwait(false);
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
				if (null != Logger)
				{
					Logger("error", exception.ToString());
				}

				responseContent = string.Format(
					"{ \"error\":\"exception\",\"error_description\":\"{0}\"}",
					exception.ToString());
			}

			return responseContent;
		}

		public string Request(HttpMethod method, string requestUrl,
			IList<KeyValuePair<string, string>> parameters)
		{
#if (USE_HTTPWebRequest)
			string responseContent = null;

			try
			{
				FormUrlEncodedContent content =
					new FormUrlEncodedContent(parameters);

				bool isComplete =
					Uri.IsWellFormedUriString(requestUrl, UriKind.Absolute);
				if (false == isComplete)
				{
					requestUrl = string.Format(@"{0}{1}", Host, requestUrl);
				}

				DoAuthentication();

				responseContent = GetResponse(method, requestUrl, content);

				if (true == IsError)
				{
					// see if it just a matter of an expired token
					bool refreshed = RefeshToken(responseContent);

					if (true == refreshed)
					{
						// call again with new tokens
						responseContent =
							GetResponse(method, requestUrl, content);
					}
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
				if (null != Logger)
				{
					Logger("error", exception.ToString());
				}

				responseContent = string.Format(
					"{ \"error\":\"exception\",\"error_description\":\"{0}\"}",
					exception.ToString());
			}

			return responseContent;
#endif
#if (USE_RestSharp)
			RestClient client = new RestClient();
			client.BaseUrl = new Uri(host);
			// client.Authenticator = new HttpBasicAuthenticator(username, password);

			var request = new RestRequest("resource/{id}", Method.POST);

			// adds to POST or URL querystring based on Method
			foreach(RequestParameter parameter in parameters)
			{
				if (method == method.Post)
				{
					request.AddParameter(parameter.Variable, parameter.Value);
				}
				else
				{
					// replaces matching token in request.Resource
					request.AddUrlSegment(parameter.Variable, parameter.Value);
				}
			}

			// easily add HTTP Headers
			//request.AddHeader("header", "value");

			// add files to upload (works with compatible verbs)
			//request.AddFile(path);

			// execute the request
			IRestResponse response = client.Execute(request);
			string content = response.Content; // raw content as string

			// or automatically deserialize result
			// return content type is sniffed but can be explicitly set via RestClient.AddHandler();
			//RestResponse<Person> response2 = client.Execute<Person>(request);
			//var name = response2.Data.Name;

			// easy async support
			//client.ExecuteAsync(request, response => {
			//	Console.WriteLine(response.Content);
			//});

			// async with deserialization
			//var asyncHandle = client.ExecuteAsync<Person>(request, response => {
			//	Console.WriteLine(response.Data.Name);
			//});

			// abort the request on demand
			//asyncHandle.Abort();
		}

		public T Execute<T>(RestRequest request) where T : new()
		{
			var client = new RestClient();
			client.BaseUrl = new System.Uri(BaseUrl);
			client.Authenticator = new HttpBasicAuthenticator(_accountSid, _secretKey);
			request.AddParameter("AccountSid", _accountSid, ParameterType.UrlSegment); // used on every request
			var response = client.Execute<T>(request);

			if (response.ErrorException != null)
			{
				const string message = "Error retrieving response.  Check inner details for more info.";
				var twilioException = new ApplicationException(message, response.ErrorException);
				throw twilioException;
			}
			return response.Data;
#endif
		}

		public string Request(HttpMethod method, string query, string[] keys,
			string[] values)
		{
			IList<KeyValuePair<string, string>> parameters =
				GetClientParameters();

			var items = keys.Zip(values, (key, value) =>
				new { Key = key, Value = value });

			foreach (var item in items)
			{
				KeyValuePair<string, string> pair =
					new KeyValuePair<string, string>(item.Key, item.Value);
				parameters.Add(pair);
			}

			return Request(method, query, parameters);
		}

		private void DoAuthentication()
		{
			if (true == Authenticate)
			{
				string clientAuthorization =
					string.Format("{0}:{1}", clientId, clientSecret);

				// this works
				byte[] bytes = new UTF8Encoding().GetBytes(clientAuthorization);
				client.DefaultRequestHeaders.Authorization =
					new AuthenticationHeaderValue(
						"Basic",
						Convert.ToBase64String(bytes));

				Authenticate = false;
			}
			else
			{
				client.DefaultRequestHeaders.Authorization = null;
			}
		}

		private async Task<string> GetResponse(string requestUrl)
		{
			string responseContent = null;
			IsError = false;

			try
			{
				Uri uri = new Uri(requestUrl);

				responseContent = await client.GetStringAsync(uri).
						ConfigureAwait(false);

				if (errors.Any(responseContent.Contains))
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
			}

			return responseContent;
		}

		private string GetResponse(HttpMethod method, string requestUrl,
			FormUrlEncodedContent content)
		{
			string responseContent = null;

			try
			{
				IsError = false;
				Response = null;

				if (method == HttpMethod.Post)
				{
					// save this as clients may want know this
					RequestMessage = new HttpRequestMessage();
					RequestMessage.RequestUri = new Uri(requestUrl);
					RequestMessage.Method = method;

					Response = client.PostAsync(requestUrl, content).Result;
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

						string message = string.Format("Redirecting to {0}",
							redirectUri);
						System.Diagnostics.Debug.WriteLine(message);

						responseContent =
							GetResponse(method, requestUrl, content);
					}
					else if (!Response.IsSuccessStatusCode)
					{
						//IsError = true;
						System.Diagnostics.Debug.WriteLine(
							"status code not ok");
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

					Logger("error", Response.ReasonPhrase);

					if (!IsValidJsonError(responseContent))
					{
						string error = Response.StatusCode.ToString();
						if (true == IncludeSourceInError)
						{
							responseContent = string.Format("{{ \"error\":" +
								"\"{0}\",\"error_description\":\"{1}\"}}",
								error, responseContent);
						}
						else
						{
							responseContent = string.Format("{{ \"error\":" +
								"\"{0}\",\"error_description\":\"{1}\"}}",
								error,
								"An unidentified server error occurred");
						}
					}
				}

				if (errors.Any(responseContent.Contains))
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
				if (null != Logger)
				{
					Logger("error", exception.ToString());
				}

				responseContent = string.Format("{ \"error\":\"exception\"," +
					"\"error_description\":\"{0}\"}", exception.ToString());
			}

			return responseContent;
		}

		private bool IsValidJsonError(string test)
		{
			bool valid = false;

			try
			{
				ErrorResponse response =
					JsonConvert.DeserializeObject<ErrorResponse>(test);

				valid = true;
			}
			catch (Exception exception) when (exception is JsonReaderException)
			{
			}

			return valid;
		}

		private bool RefeshToken(string response)
		{
			bool updated = false;

			try
			{
				if (response.IndexOf("error",
					StringComparison.OrdinalIgnoreCase) >= 0)
				{
					string[] refreshErrors = { "expired_token" ,
						"Malformed auth header", "invalid_token"};
					Token serverResponse =
						JsonConvert.DeserializeObject<Token>(response);

					if (refreshErrors.Contains(serverResponse.error))
					{
						string refresh = RequestRefreshToken(
							RefreshTokenEndPoint, RefreshToken);
						if (refresh.IndexOf("error",
							StringComparison.OrdinalIgnoreCase) == -1)
						{
							Token refreshResponse =
							JsonConvert.DeserializeObject<Token>(response);

							AccessToken = refreshResponse.access_token;
							RefreshToken = refreshResponse.refresh_token;
							OnPropertyChanged("AccessToken");
							OnPropertyChanged("RefreshToken");

							updated = true;
						}
					}
				}
			}
			catch (Exception exception) when (exception is JsonReaderException)
			{
				IsError = true;
				if (null != Logger)
				{
					Logger("error", exception.ToString());
				}
			}

			return updated;
		}

		public string RequestRefreshToken(string endpoint,
			string refreshToken)
		{
			string[] keys = { "grant_type", "refresh_token", "client_id",
			"client_secret" };
			string[] values = { "refresh_token", refreshToken, clientId,
				clientSecret };

			Authenticate = true;

			string response = Request(HttpMethod.Post, endpoint, keys, values);

			return response;
		}
	}
}