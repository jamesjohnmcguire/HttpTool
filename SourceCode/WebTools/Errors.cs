/////////////////////////////////////////////////////////////////////////////
// <copyright file="Errors.cs" company="James John McGuire">
// Copyright © 2016 - 2020 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

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