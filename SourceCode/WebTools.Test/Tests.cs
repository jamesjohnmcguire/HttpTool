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
				await client.Request(uri).ConfigureAwait(false);

			Assert.NotNull(response);
		}

	}
}
