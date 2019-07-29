using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTools
{
	public class PageValidationResult
	{
		public string Url { get; set; }
		public IList<ValidationResult> Messages { get; set; }
	}
}
