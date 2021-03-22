/////////////////////////////////////////////////////////////////////////////
// <copyright file="CookieStore.cs" company="James John McGuire">
// Copyright © 2016 - 2021 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Globalization;
using System.Net;

namespace WebTools
{
	/// <summary>
	/// Scalvaged from the internet:
	/// https://stackoverflow.com/questions/18998354/httpwebrequest-headers-addcookie-value-vs-httpwebrequest-cookiecontainer
	/// https://snipplr.com/view/4427.
	/// </summary>
	public class CookieStore
	{
		public static CookieCollection GetAllCookiesFromHeader(
			string header, string host)
		{
			ArrayList cookieList;
			CookieCollection cookieCollection = new ();

			if (header != string.Empty)
			{
				cookieList = ConvertCookieHeaderToArrayList(header);
				cookieCollection =
					ConvertCookieArraysToCookieCollection(cookieList, host);
			}

			return cookieCollection;
		}

		private static ArrayList ConvertCookieHeaderToArrayList(
			string cookHeader)
		{
			cookHeader = cookHeader.Replace("\r", string.Empty);
			cookHeader = cookHeader.Replace("\n", string.Empty);

			string[] cookieParts = cookHeader.Split(',');

			ArrayList cookieList = new ();
			int index = 0;

			while (index < cookieParts.Length)
			{
				string subCookie = cookieParts[index];
				string subCookieNext = cookieParts[index + 1];
				int subIndex = subCookie.IndexOf(
					"expires=", StringComparison.OrdinalIgnoreCase);

				if (subIndex > 0)
				{
					string newCookie = string.Format(
						CultureInfo.InvariantCulture,
						"{0},{1}",
						subCookie,
						subCookieNext);

					cookieList.Add(newCookie);
					index++;
				}
				else
				{
					cookieList.Add(subCookie);
				}

				index++;
			}

			return cookieList;
		}

		private static CookieCollection ConvertCookieArraysToCookieCollection(
			ArrayList cookieList, string host)
		{
			CookieCollection cc = new ();

			string[] cookieParts;

			for (int index = 0; index < cookieList.Count; index++)
			{
				object cookie = cookieList[index];
				string cookieText = cookie.ToString();

				cookieParts = cookieText.Split(';');

				string strCNameAndCValue;
				string strPNameAndPValue;
				string[] nameValuePairTemp;
				Cookie cookTemp = new ();

				for (int subIndex = 0; subIndex < cookieParts.Length;
					subIndex++)
				{
					if (subIndex == 0)
					{
						strCNameAndCValue = cookieParts[subIndex];
						if (strCNameAndCValue != string.Empty)
						{
							int firstEqual = strCNameAndCValue.IndexOf("=");
							string firstName =
								strCNameAndCValue.Substring(0, firstEqual);
							string allValue = strCNameAndCValue.Substring(
								firstEqual + 1, strCNameAndCValue.Length - (firstEqual + 1));
							cookTemp.Name = firstName;
							cookTemp.Value = allValue;
						}

						continue;
					}

					if (cookieParts[subIndex].Contains("path", StringComparison.OrdinalIgnoreCase))
					{
						strPNameAndPValue = cookieParts[subIndex];
						if (strPNameAndPValue != string.Empty)
						{
							nameValuePairTemp = strPNameAndPValue.Split('=');
							if (nameValuePairTemp[1] != string.Empty)
							{
								cookTemp.Path = nameValuePairTemp[1];
							}
							else
							{
								cookTemp.Path = "/";
							}
						}

						continue;
					}

					if (cookieParts[subIndex].Contains("domain", StringComparison.OrdinalIgnoreCase))
					{
						strPNameAndPValue = cookieParts[subIndex];
						if (strPNameAndPValue != string.Empty)
						{
							nameValuePairTemp = strPNameAndPValue.Split('=');

							if (nameValuePairTemp[1] != string.Empty)
							{
								cookTemp.Domain = nameValuePairTemp[1];
							}
							else
							{
								cookTemp.Domain = host;
							}
						}

						continue;
					}
				}

				if (cookTemp.Path == string.Empty)
				{
					cookTemp.Path = "/";
				}

				if (cookTemp.Domain == string.Empty)
				{
					cookTemp.Domain = host;
				}

				cc.Add(cookTemp);
			}

			return cc;
		}
	}
}
