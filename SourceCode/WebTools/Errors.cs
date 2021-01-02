/////////////////////////////////////////////////////////////////////////////
// <copyright file="Errors.cs" company="James John McGuire">
// Copyright © 2016 - 2020 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System;

namespace WebTools
{
	[Flags]
	public enum Errors
	{
		/// <summary>
		/// No errors
		/// </summary>
		None = 0,

		/// <summary>
		/// Unknown error
		/// </summary>
		UnknownError = 1,

		/// <summary>
		/// Invalide email address
		/// </summary>
		InvalidEmailAddress = 2,

		/// <summary>
		/// Password too short
		/// </summary>
		PasswordTooShort = 4,

		/// <summary>
		/// Passwords do not match
		/// </summary>
		PasswordsDoNotMatch = 8,

		/// <summary>
		/// Password does not have number
		/// </summary>
		PasswordDoesNotHaveNumber = 16,

		/// <summary>
		/// Unable to connect
		/// </summary>
		UnableToConnect = 32
	}
}
