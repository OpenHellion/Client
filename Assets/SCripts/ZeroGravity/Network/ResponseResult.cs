namespace ZeroGravity.Network
{
	public enum ResponseResult : short
	{
		OwnershipIssue = -5,
		WrongPassword = -4,
		AlreadyLoggedInError = -3,
		ClientVersionError = -2,
		Error = -1,
		Success = 1
	}
}
