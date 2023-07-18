/////////////////////////////////////////////////////////////////////////////
// <copyright file="HyperLinkParser.cs" company="James John McGuire">
// Copyright © 2016 - 2023 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Abot2.Core;
using Abot2.Poco;
using AngleSharp.Dom;
using System.Collections.Generic;
using System.Linq;

namespace WebTools
{
	/// <summary>
	/// Manages hyper link parsing.
	/// </summary>
	/// <seealso cref="Abot2.Core.AngleSharpHyperlinkParser" />
	public class HyperLinkParser : AngleSharpHyperlinkParser
	{
		/// <inheritdoc/>
		protected override IEnumerable<HyperLink> GetRawHyperLinks(CrawledPage crawledPage)
		{
			IEnumerable<HyperLink> links = base.GetRawHyperLinks(crawledPage);

			return links;
		}
	}
}
