using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abot.Core;
using Abot.Poco;

namespace WebTools
{
	public class SiteTestPageRequester : PageRequester
	{
		public SiteTestPageRequester(CrawlConfiguration config) : base(config)
		{

		}

		public override CrawledPage MakeRequest(Uri uri, Func<CrawledPage, CrawlDecision> shouldDownloadContent)
		{
			return base.MakeRequest(uri, shouldDownloadContent);
		}
	}
}
