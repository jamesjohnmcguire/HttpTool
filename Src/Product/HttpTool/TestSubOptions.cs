using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace HttpTool
{
	public class TestSubOptions : CommonSubOptions
	{
		[ValueOption(1)]
		public string Url { get; set; }
	}
}
