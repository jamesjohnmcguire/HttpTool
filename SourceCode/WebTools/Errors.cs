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