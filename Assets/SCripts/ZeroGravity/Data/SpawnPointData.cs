using System.Collections.Generic;

namespace ZeroGravity.Data
{
	public class SpawnPointData : ISceneData
	{
		public int InSceneID;

		public TagAction TagAction;

		public string Tags;

		public SpawnPointType Type;

		public int ExecuterID;

		public int ExecuterStateID;

		public List<int> ExecuterOccupiedStateIDs;
	}
}
