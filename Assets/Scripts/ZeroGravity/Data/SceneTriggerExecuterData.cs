using System.Collections.Generic;

namespace ZeroGravity.Data
{
	public class SceneTriggerExecuterData : ISceneData
	{
		public int InSceneID;

		public int DefaultStateID;

		public TagAction TagAction;

		public string Tags;

		public List<SceneTriggerExecuterStateData> States;

		public List<SceneTriggerProximityData> ProximityTriggers;
	}
}
