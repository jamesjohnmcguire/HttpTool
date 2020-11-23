/////////////////////////////////////////////////////////////////////////////
// <copyright file="SiteTestLinkParser.cs" company="James John McGuire">
// Copyright © 2016 - 2020 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Abot2.Core;
using Abot2.Poco;
using HtmlAgilityPack;

namespace WebTools
{
	/// <summary>
	/// Parser that uses Html Agility Pack http://htmlagilitypack.codeplex.com/ to parse page links
	/// </summary>
	[Serializable]
	public class SiteTestLinkParser : HyperLinkParser
	{
		protected override string ParserType => throw new NotImplementedException();

		public SiteTestLinkParser()
			: base()
		{
		}

		protected override IEnumerable<string> GetHrefValues(CrawledPage crawledPage)
		{
			List<string> hrefValues = new List<string>();
			if (HasRobotsNoFollow(crawledPage))
				return hrefValues;

			//HtmlNodeCollection aTags = crawledPage.HtmlDocument.DocumentNode.SelectNodes("//a[@href]");
			//HtmlNodeCollection areaTags = crawledPage.HtmlDocument.DocumentNode.SelectNodes("//area[@href]");
			//HtmlNodeCollection canonicals = crawledPage.HtmlDocument.DocumentNode.SelectNodes("//link[@rel='canonical'][@href]");

			//hrefValues.AddRange(GetLinks(aTags));
			//hrefValues.AddRange(GetLinks(areaTags));
			//hrefValues.AddRange(GetLinks(canonicals));

			return hrefValues;
		}

		protected override string GetBaseHrefValue(CrawledPage crawledPage)
		{
			string hrefValue = "";
			//HtmlNode node = crawledPage.HtmlDocument.DocumentNode.SelectSingleNode("//base");

			////Must use node.InnerHtml instead of node.InnerText since "aaa<br />bbb" will be returned as "aaabbb"
			//if (node != null)
			//	hrefValue = node.GetAttributeValue("href", "").Trim();

			return hrefValue;
		}

		protected override string GetMetaRobotsValue(CrawledPage crawledPage)
		{
			string robotsMeta = null;
			//HtmlNode robotsNode = crawledPage.HtmlDocument.DocumentNode.SelectSingleNode("//meta[translate(@name,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='robots']");
			//if (robotsNode != null)
			//	robotsMeta = robotsNode.GetAttributeValue("content", "");

			return robotsMeta;
		}

		protected virtual List<string> GetLinks(HtmlNodeCollection nodes)
		{
			List<string> hrefs = new List<string>();

			if (nodes == null)
				return hrefs;

			string hrefValue = "";
			foreach (HtmlNode node in nodes)
			{
				if (HasRelNoFollow(node))
					continue;

				hrefValue = node.Attributes["href"].Value;
				if ((!string.IsNullOrWhiteSpace(hrefValue)) &&
					(!hrefValue.Equals("#")))
				{
					hrefValue = DeEntitize(hrefValue);
					hrefs.Add(hrefValue);
				}
			}

			return hrefs;
		}

		protected virtual string DeEntitize(string hrefValue)
		{
			string dentitizedHref = hrefValue;

			try
			{
				dentitizedHref = HtmlEntity.DeEntitize(hrefValue);
			}
			catch (Exception e)
			{
				Console.WriteLine(
					"Error dentitizing uri: {0} This usually means that it contains unexpected characters",
					hrefValue);
			}

			return dentitizedHref;
		}

		protected virtual bool HasRelNoFollow(HtmlNode node)
		{
			HtmlAttribute attr = node.Attributes["rel"];
			return this.Config.IsRespectAnchorRelNoFollowEnabled &&
				(attr != null && attr.Value.ToLower().Trim() == "nofollow");
		}
	}
}
