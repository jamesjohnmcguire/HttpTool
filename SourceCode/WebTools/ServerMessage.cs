/////////////////////////////////////////////////////////////////////////////
// <copyright file="ServerMessage.cs" company="James John McGuire">
// Copyright © 2016 - 2023 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json;
using System;

namespace WebTools
{
	/// <summary>
	/// Represents a basic message from the server.
	/// </summary>
	public class ServerMessage
	{
		/// <summary>
		/// Gets or sets the json encoded data item of the message.
		/// </summary>
		/// <value>The json encoded data item of the message.</value>
		public string Data { get; set; }

		/// <summary>
		/// Gets or sets the description of message.
		/// </summary>
		/// <value>The description of the message.</value>
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the additional details of the message.
		/// </summary>
		/// <value>The additional details of the message.</value>
		public string Details { get; set; }

		/// <summary>
		/// Gets or sets the error code.
		/// </summary>
		/// <value>The error code or brief description.</value>
		[JsonProperty("error")]
		public string Error { get; set; }

		/// <summary>
		/// Gets or sets the error description.
		/// </summary>
		/// <value>The error description.</value>
		[JsonProperty("error_description")]
		public string ErrorDescription { get; set; }

		/// <summary>
		/// Gets or sets the error type.
		/// </summary>
		/// <value>The type of error.</value>
		public Errors ErrorType { get; set; }

		/// <summary>
		/// Gets a value indicating whether the message status code is successful.
		/// </summary>
		/// <value>Whether the message status code is successful.</value>
		public bool IsSuccessStatusCode
		{
			get
			{
				return Status >= 200 && Status <= 299;
			}
		}

		/// <summary>
		/// Gets or sets the message.
		/// </summary>
		/// <value>The message.</value>
		public string Message { get; set; }

		/// <summary>
		/// Gets or sets the message code.
		/// </summary>
		/// <value>The message code.</value>
		[JsonProperty("message_code")]
		public string MessageCode { get; set; }

		/// <summary>
		/// Gets or sets the message status code.
		/// </summary>
		/// <value>The message status code.</value>
		public int Status { get; set; }

		/// <summary>
		/// Gets or sets the sub message.
		/// </summary>
		/// <value>The sub message.</value>
		[JsonProperty("sub_message")]
		public string SubMessage { get; set; }

		/// <summary>
		/// Gets or sets the sub message code.
		/// </summary>
		/// <value>The sub message code.</value>
		[JsonProperty("sub_message_code")]
		public string SubMessageCode { get; set; }
	}
}
