using System;

namespace ZeroGravity.Data
{
	[Serializable]
	public class AuthorizedPerson : ISceneData
	{
		public AuthorizedPersonRank Rank;

		public string PlayerId;

		// Used to get avatar.
		public string PlayerNativeId;

		public string Name;

		public bool IsFriend;
	}
}
