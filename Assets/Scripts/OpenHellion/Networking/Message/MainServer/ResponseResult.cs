namespace OpenHellion.Networking.Message.MainServer
{
	public enum ResponseResult : byte {
		Success = 0,
		Error = 1,
		WrongPassword = 3,
		AlreadyLoggedInError = 4,
		ClientVersionError = 5,
		ServerNotFound = 6,
		RequestInvalid = 7,
		AccountNotFound = 8,
		AccountAlreadyExists = 9
	}
}
