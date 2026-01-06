/////////////////////////////////////////////////////////////////////////////
// Copyright © 2016 - 2026 by James John McGuire
// All rights reserved.
/////////////////////////////////////////////////////////////////////////////

namespace WebTools.Test;

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
		HttpManager client = new HttpManager();

		Uri uri = new ("https://www.digitalzenworks.com");
		string response =
			await client.RequestUriBody(uri).ConfigureAwait(false);

		Assert.That(response, Is.Not.Null);
	}

	[Test]
	public async Task TestHttpMinerExtended()
	{
		HttpManager client = new HttpManager();

		Uri uri = new ("https://www.digitalzenworks.com");

		IList<KeyValuePair<string, string>> parameters =
			new List<KeyValuePair<string, string>>();

		KeyValuePair<string, string> pair = new ("key1", "value1");
		parameters.Add(pair);
		pair = new ("key2", "value2");
		parameters.Add(pair);

		string response = await client.Request(
			HttpMethod.Post, uri, parameters).ConfigureAwait(false);

		Assert.That(response, Is.Not.Null);
	}

	[Test]
	public void TestHttpMinerSimple()
	{
		HttpManager client = new HttpManager();

		Uri uri = new ("https://www.digitalzenworks.com");
		HttpResponseMessage response = client.RequestUriResponse(uri);

		Assert.That(response, Is.Not.Null);
	}

	[Test]
	public async Task TestHttpMinerExtendedUploadFile()
	{
		HttpManager client = new HttpManager();

		string temporaryPath = Path.GetTempFileName();

		string response = await client.UploadFile(
			"https://www.digitalzenworks.com",
			"test",
			temporaryPath).ConfigureAwait(false);

		Assert.That(response, Is.Not.Null);
	}
}
