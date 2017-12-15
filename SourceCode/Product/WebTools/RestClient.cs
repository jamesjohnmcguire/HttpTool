#define USE_HTTPWebRequest
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace WebTools
{
	public class RestClient
	{
		private HttpClient client = null;
		private CookieContainer cookieJar = null;

		public RestClient()
		{
			cookieJar = new CookieContainer();
			HttpClientHandler clientHandler = new HttpClientHandler();
			clientHandler.CookieContainer = cookieJar;

			// actually doesn't seem to work
			//NetworkCredential credentials =
			//new NetworkCredential(clientName, clientKey);
			//clientHandler.Credentials = credentials;

			client = new HttpClient(clientHandler);
			client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
				"Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) " +
				"Gecko/20100101 Firefox/19.0");
			client.Timeout = TimeSpan.FromMinutes(1);
		}

		public void AddCookie(string name, string value)
		{
			Cookie cookie = new Cookie(name, value);
			cookieJar.Add(cookie);
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

		public string Request(HttpMethod method, string requestUrl,
			IList<KeyValuePair<string, string>> parameters)
		{
#if (USE_HTTPWebRequest)
			string responseContent = null;

			try
			{
				FormUrlEncodedContent content =
					new FormUrlEncodedContent(parameters);

				responseContent = GetResponse(method, requestUrl, content);
			}
			catch (Exception exception) when (exception is ArgumentException ||
				exception is ArgumentNullException ||
				exception is ArgumentOutOfRangeException ||
				exception is IOException ||
				exception is FileNotFoundException ||
				exception is NotSupportedException ||
				exception is ObjectDisposedException ||
				exception is System.FormatException ||
				exception is UnauthorizedAccessException)
			{
				//IsError = true;
				System.Diagnostics.Debug.WriteLine(exception.ToString());
				//Log.Error(exception.ToString());

				//Error = new Error();
				//Error.ErrorType = Errors.UnknownError;
				//Error.Description = exception.ToString();

				responseContent = string.Format("{ \"error\":\"exception\"," +
					"\"error_description\":\"{0}\"}", exception.ToString());
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
				new List<KeyValuePair<string, string>>();

			var items = keys.Zip(values, (key, value) =>
				new { Key = key, Value = value });

			foreach (var item in items)
			{
				KeyValuePair<string, string> pair =
					new KeyValuePair<string, string>(item.Key, item.Value);
				parameters.Add(pair);

			}

			return Request(HttpMethod.Post, query, parameters);
		}

		private string GetResponse(HttpMethod method, string requestUrl,
					FormUrlEncodedContent content)
		{
			string responseContent = null;

			try
			{
				HttpResponseMessage response = null;

				if (method == HttpMethod.Post)
				{
					response = client.PostAsync(requestUrl, content).Result;
				}

				responseContent = response.Content.ReadAsStringAsync().Result;

				if (responseContent.IndexOf("error",
					StringComparison.OrdinalIgnoreCase) > -1)
				{
					//IsError = true;
				}
			}
			catch (Exception exception) when (exception is ArgumentException ||
				exception is ArgumentNullException ||
				exception is ArgumentOutOfRangeException ||
				exception is IOException ||
				exception is FileNotFoundException ||
				exception is NotSupportedException ||
				exception is ObjectDisposedException ||
				exception is System.FormatException ||
				exception is UnauthorizedAccessException)
			{
				//IsError = true;
				System.Diagnostics.Debug.WriteLine(exception.ToString());
				//Log.Error(exception.ToString());

				//Error = new Error();
				//Error.ErrorType = Errors.UnknownError;
				//Error.Description = exception.ToString();

				responseContent = string.Format("{ \"error\":\"exception\"," +
					"\"error_description\":\"{0}\"}", exception.ToString());
			}

			return responseContent;
		}
	}
}
