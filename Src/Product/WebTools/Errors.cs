using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTools
{
	public enum Errors
	{
		None = 0,
		UnknownError = 1,
		InvalidEmailAddress = 2,
		PasswordTooShort = 4,
		PasswordsDoNotMatch = 8,
		PasswordDoesNotHaveNumber = 16,
		UnableToConnect = 32
	}
}
