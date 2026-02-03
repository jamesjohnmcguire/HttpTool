/////////////////////////////////////////////////////////////////////////////
// <copyright file="CookieStore.cs" company="James John McGuire">
// Copyright © 2016 - 2026 James John McGuire.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace WebTools
{
	using System;
	using System.Collections;
	using System.Globalization;
	using System.Net;

	/// <summary>
	/// Provides support for cookie storage.
	/// </summary>
	/// <remarks>
	/// Scalvaged from the internet:
	/// https://stackoverflow.com/questions/18998354/httpwebrequest-headers-addcookie-value-vs-httpwebrequest-cookiecontainer
	/// https://snipplr.com/view/4427.
	/// </remarks>
	public static class CookieStore
	{
		/// <summary>
		/// Get all cookies from header.
		/// </summary>
		/// <param name="header">The header to check.</param>
		/// <param name="host">The host to use.</param>
		/// <returns>A cookie collection.</returns>
		public static CookieCollection GetAllCookiesFromHeader(
			string header, string host)
		{
			ArrayList cookieList;
			CookieCollection cookieCollection = new ();

			if (!string.IsNullOrWhiteSpace(header))
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
#if NETSTANDARD2_0
			cookHeader = cookHeader.Replace("\r", string.Empty);
			cookHeader = cookHeader.Replace("\n", string.Empty);
#else
			cookHeader = cookHeader.Replace(
				"\r", string.Empty, StringComparison.OrdinalIgnoreCase);
			cookHeader = cookHeader.Replace(
				"\n", string.Empty, StringComparison.OrdinalIgnoreCase);
#endif

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
				Cookie cookTemp = new ();

				for (int subIndex = 0; subIndex < cookieParts.Length;
					subIndex++)
				{
					if (subIndex == 0)
					{
						strCNameAndCValue = cookieParts[subIndex];
						if (!string.IsNullOrWhiteSpace(strCNameAndCValue))
						{
							int firstEqual = strCNameAndCValue.IndexOf(
								"=", StringComparison.Ordinal);
#if NETSTANDARD2_0
							string firstName =
								strCNameAndCValue.Substring(0, firstEqual);
							string allValue = strCNameAndCValue.Substring(
								firstEqual + 1,
								strCNameAndCValue.Length - (firstEqual + 1));
#else
							string firstName = strCNameAndCValue[..firstEqual];
							string allValue =
								strCNameAndCValue[(firstEqual + 1) ..];
#endif
							cookTemp.Name = firstName;
							cookTemp.Value = allValue;
						}

						continue;
					}

					cookTemp =
						ProcessCookiePart(cookieParts[subIndex], "path", "/");

					if (cookTemp != null)
					{
						continue;
					}

					cookTemp = ProcessCookiePart(
						cookieParts[subIndex], "domain", host);

					if (cookTemp != null)
					{
						continue;
					}
				}

				if (string.IsNullOrWhiteSpace(cookTemp.Path))
				{
					cookTemp.Path = "/";
				}

				if (string.IsNullOrWhiteSpace(cookTemp.Domain))
				{
					cookTemp.Domain = host;
				}

				cc.Add(cookTemp);
			}

			return cc;
		}

		private static Cookie ProcessCookiePart(string cookiePart, string key, string value)
		{
			Cookie cookTemp = null;

#if NETSTANDARD2_0
			bool contains = cookiePart.Contains(key);
#else
			bool contains = cookiePart.Contains(
				key, StringComparison.OrdinalIgnoreCase);
#endif

			if (contains == true)
			{
				if (!string.IsNullOrWhiteSpace(cookiePart))
				{
					cookTemp = new ();

					string[] nameValuePairTemp;

					nameValuePairTemp = cookiePart.Split('=');

					if (!string.IsNullOrWhiteSpace(nameValuePairTemp[1]))
					{
						cookTemp.Domain = nameValuePairTemp[1];
					}
					else
					{
						cookTemp.Domain = value;
					}
				}
			}

			return cookTemp;
		}
	}
}
