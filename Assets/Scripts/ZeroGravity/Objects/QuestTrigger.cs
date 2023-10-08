using System.Linq;
using OpenHellion;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	public class QuestTrigger
	{
		public uint ID;

		public uint BatchID;

		public QuestTriggerType Type;

		public long StationMainVesselGUID;

		public CelestialBodyGUID CelestialGUID;

		public QuestStatus Status;

		public uint DependencyBatchID;

		public QuestTriggerDependencyTpe DependencyTpe;

		private bool checkStation;

		private bool checkCelestial;

		private bool checkTag;

		public Quest Quest;

		public QuestTaskObject TaskObject;

		public string Tag
		{
			get
			{
				if (TaskObject != null)
				{
					return SceneTagObject.TagsToString(TaskObject.Tags);
				}

				return string.Empty;
			}
		}

		public string Name
		{
			get
			{
				if (TaskObject != null)
				{
					return Localization.GetLocalizedField(TaskObject.NameTag, useDefault: true);
				}

				Dbg.Error("Quest trigger has missing TaskObject (Quest ID: " + Quest.ID + ", QuestTriggerID: " + ID);
				return string.Empty;
			}
		}

		public string Description
		{
			get
			{
				if (TaskObject != null)
				{
					return Localization.GetLocalizedField(TaskObject.DescriptionTag, useDefault: true);
				}

				Dbg.Error("Quest trigger has missing TaskObject (Quest ID: " + Quest.ID + ", QuestTriggerID: " + ID);
				return string.Empty;
			}
		}

		public QuestTrigger(QuestCollectionObject questCollection, Quest quest, QuestTriggerData data)
		{
			TaskObject = questCollection.Tasks.FirstOrDefault((QuestTaskObject m) =>
				m.QuestID == quest.ID && m.QuestTriggerID == data.ID);
			ID = data.ID;
			Quest = quest;
			BatchID = data.BatchID;
			Type = data.Type;
			if (data.Station != null && data.Station != string.Empty)
			{
				checkStation = true;
			}

			if (Tag != string.Empty)
			{
				checkTag = true;
			}

			CelestialGUID = data.Celestial;
			checkCelestial = CelestialGUID != CelestialBodyGUID.None;
			if (Type == QuestTriggerType.Activate)
			{
				Status = QuestStatus.Active;
			}

			DependencyBatchID = data.DependencyBatchID;
			DependencyTpe = data.DependencyTpe;
			TaskObject.Quest = Quest;
			TaskObject.QuestTrigger = this;
		}

		public void SetDetails(QuestTriggerDetails details, bool showNotifications = true)
		{
			Status = details.Status;
			StationMainVesselGUID = details.StationMainVesselGUID;
		}

		public bool CheckLocation(SpaceObjectVessel vessel)
		{
			return (!checkStation || (checkStation && StationMainVesselGUID == vessel.MainVessel.GUID)) &&
			       (!checkCelestial || (checkCelestial && vessel.ParentCelesitalBody.GUID == (long)CelestialGUID)) &&
			       (!checkTag || (checkTag && SceneHelper.CompareTags(vessel.VesselData.Tag, Tag)));
		}
	}
}
