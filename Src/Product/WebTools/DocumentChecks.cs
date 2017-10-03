using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTools
{
	[Flags]
	public enum DocumentChecks
	{
		None = 0,
		Basic = 1,
		Redirect = 2,
		EmptyContent = 4,
		ContentErrors = 8,
		ParseErrors = 16,
		ImagesExist = 32,
		W3cValidation = 64
	}
}
