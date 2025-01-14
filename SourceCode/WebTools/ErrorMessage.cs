/////////////////////////////////////////////////////////////////////////////
// <copyright file="ErrorMessage.cs" company="James John McGuire">
// Copyright © 2016 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace WebTools
{
	/// <summary>
	/// Provides support for supplying an error message.
	/// </summary>
	public class ErrorMessage : Message
	{
		/// <summary>
		/// Gets or sets the type of error.
		/// </summary>
		/// <value>The type of error.</value>
		public Errors ErrorType { get; set; }
	}
}
