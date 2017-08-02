using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTools
{
	public class ValidationResult
	{
		public string Type { get; set; }
		public string SubType { get; set; }
		public int LastLine { get; set; }
		public int FirstColumn { get; set; }
		public int LastColumn { get; set; }
		public string Message { get; set; }
		public string Extract { get; set; }
		public int HiliteStart { get; set; }
		public int HiliteLength { get; set; }
	}
}
