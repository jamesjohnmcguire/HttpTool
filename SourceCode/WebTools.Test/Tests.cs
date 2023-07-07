namespace WebTools.Test
{
	public class Tests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void Test1()
		{
			Assert.Pass();
		}

		[Test]
		public async Task TestHttpMinerBasic()
		{
			HttpClientExtended client = new HttpClientExtended();

			Uri uri = new("https://www.digitalzenworks.com");
			string response =
				await client.RequestUriBody(uri).ConfigureAwait(false);

			Assert.NotNull(response);
		}

		[Test]
		public async Task TestHttpMinerExtended()
		{
			HttpClientExtended client = new HttpClientExtended();

			Uri uri = new("https://www.digitalzenworks.com");

			IList<KeyValuePair<string, string>> parameters =
				new List<KeyValuePair<string, string>>();

			KeyValuePair<string, string> pair = new("key1", "value1");
			parameters.Add(pair);
			pair = new("key2", "value2");
			parameters.Add(pair);

			string response = await client.Request(
				HttpMethod.Post, uri, parameters).ConfigureAwait(false);

			Assert.NotNull(response);
		}

		[Test]
		public void TestHttpMinerSimple()
		{
			HttpClientExtended client = new HttpClientExtended();

			Uri uri = new("https://www.digitalzenworks.com");
			HttpResponseMessage response = client.RequestGetResponse(uri);

			Assert.NotNull(response);
		}

		[Test]
		public async Task TestHttpMinerExtendedUploadFile()
		{
			HttpClientExtended client = new HttpClientExtended();

			string temporaryPath = Path.GetTempFileName();

			string response = await client.UploadFile(
				"https://www.digitalzenworks.com",
				"test",
				temporaryPath).ConfigureAwait(false);

			Assert.NotNull(response);
		}
	}
}
