namespace OpenHellion.Networking.MainServerMessage
{
	public enum ResponseResult {
		Success = 0,
		Error = 1,
		WrongPassword = 3,
		AlreadyLoggedInError = 4,
		ClientVersionError = 5,
		ServerNotFound = 6,
		RequestInvalid = 7,
		UserNotFound = 8,
		UserAlreadyExists = 9
	}
}
