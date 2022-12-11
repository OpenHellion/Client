using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using ZeroGravity.Data;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class SceneQuestTrigger : MonoBehaviour
	{
		[Tooltip("Used only if Task is not assigned")]
		public uint QuestID;

		[Tooltip("Used only if Task is not assigned")]
		public uint QuestTriggerID;

		public SceneQuestTriggerEvent TriggerEvent;

		private Quest quest;

		private QuestTrigger questTrigger;

		private SpaceObjectVessel parentVessel;

		private double lastActivationTime;

		public QuestTaskObject Task;

		public List<QuestTaskObject> AdditionalTasksToComplete;

		public QuestIndicatorVisibility Visibility = QuestIndicatorVisibility.AlwaysVisible;

		public float ProximityDistance = 10f;

		public Vector3 QuestIndicatorPosition = new Vector3(0f, 1f, 0f);

		public GameObject QuestTriggerObject;

		public UnityEvent PostTriggerEvent;

		public UnityEvent ActiveTriggerEvent;

		public UnityEvent CompleteTriggerEvent;

		public Collider Collider;

		public string QuickTooltip;

		public QuestTrigger QuestTrigger => questTrigger;

		public SpaceObjectVessel ParentVessel => GetComponentInParent<GeometryRoot>().MainObject as SpaceObjectVessel;

		private void Start()
		{
			if (Collider != null)
			{
				Collider.enabled = false;
			}
			if (Task != null)
			{
				QuestID = Task.QuestID;
				QuestTriggerID = Task.QuestTriggerID;
				quest = Client.Instance.Quests.FirstOrDefault((Quest m) => m.ID == QuestID);
				questTrigger = quest.QuestTriggers.FirstOrDefault((QuestTrigger m) => m.ID == QuestTriggerID);
				parentVessel = base.gameObject.GetComponentInParent<SpaceObjectVessel>();
				OnQuestTriggerUpdate();
				UpdateQuestTriggerMarker();
				UpdateAvailableQuestMarker();
				if (TriggerEvent == SceneQuestTriggerEvent.EnterTrigger || TriggerEvent == SceneQuestTriggerEvent.ExitTrigger)
				{
					base.gameObject.SetLayerRecursively("Triggers");
				}
				if (TriggerEvent == SceneQuestTriggerEvent.Interact)
				{
					base.gameObject.SetLayerRecursively("InteractiveTriggers");
				}
			}
			else
			{
				Dbg.Error("Scene Quest Trigger " + base.gameObject.name + " has no Task assigned!");
			}
		}

		private void OnDestroy()
		{
			Client.Instance.CanvasManager.CanvasUI.QuestIndicators.StopQuestIndicator(this);
		}

		private void OnDisable()
		{
			Client.Instance.CanvasManager.CanvasUI.QuestIndicators.StopQuestIndicator(this);
		}

		public void TriggerEvents()
		{
			PostTriggerEvent.Invoke();
			if (QuickTooltip != string.Empty)
			{
				Client.Instance.CanvasManager.QuickTip(Localization.GetLocalizedField(QuickTooltip));
			}
		}

		public void Activate()
		{
			if ((double)Time.realtimeSinceStartup - lastActivationTime < 1.0 || (quest.DependencyQuests != null && Client.Instance.Quests.FirstOrDefault((Quest m) => quest.DependencyQuests.Contains(m.ID) && m.Status != QuestStatus.Completed) != null))
			{
				return;
			}
			lastActivationTime = Time.realtimeSinceStartup;
			if (quest.IsFinished || questTrigger == null || questTrigger.Status != QuestStatus.Active || !questTrigger.CheckLocation(ParentVessel))
			{
				return;
			}
			Client.Instance.NetworkController.SendToGameServer(new QuestTriggerMessage
			{
				QuestID = QuestID,
				TriggerID = QuestTriggerID
			});
			foreach (QuestTaskObject item in AdditionalTasksToComplete)
			{
				Client.Instance.NetworkController.SendToGameServer(new QuestTriggerMessage
				{
					QuestID = item.QuestID,
					TriggerID = item.QuestTriggerID
				});
			}
		}

		public void UpdateQuestTriggerMarker()
		{
			Quest quest = null;
			if (Client.Instance.CanvasManager.CanvasUI.CurrentTrackingQuest != null)
			{
				quest = Client.Instance.CanvasManager.CanvasUI.CurrentTrackingQuest;
			}
			if (quest != null)
			{
				if (this.quest == quest && questTrigger.Status == QuestStatus.Active && SceneHelper.CompareTags(ParentVessel.VesselData.Tag, SceneTagObject.TagsToString(Task.Tags)))
				{
					Client.Instance.CanvasManager.CanvasUI.QuestIndicators.AddQuestIndicator(this);
				}
				else if ((questTrigger.Type != QuestTriggerType.Activate || questTrigger.Status != QuestStatus.Active) && SceneHelper.CompareTags(ParentVessel.VesselData.Tag, SceneTagObject.TagsToString(Task.Tags)))
				{
					Client.Instance.CanvasManager.CanvasUI.QuestIndicators.StopQuestIndicator(this);
				}
			}
			else if (questTrigger.Type != QuestTriggerType.Activate || questTrigger.Status != QuestStatus.Active)
			{
				Client.Instance.CanvasManager.CanvasUI.QuestIndicators.StopQuestIndicator(this);
			}
		}

		public void UpdateAvailableQuestMarker()
		{
			bool flag = true;
			if (Task.Tags.Count > 0)
			{
				flag = SceneHelper.CompareTags(ParentVessel.VesselData.Tag, SceneTagObject.TagsToString(Task.Tags));
			}
			if (questTrigger.Type != QuestTriggerType.Activate || questTrigger.Status != QuestStatus.Active || !flag)
			{
				return;
			}
			bool flag2 = true;
			if (questTrigger.Quest != null && questTrigger.Quest.DependencyQuests != null)
			{
				foreach (uint dependencyQuestID in questTrigger.Quest.DependencyQuests)
				{
					Quest quest = Client.Instance.Quests.FirstOrDefault((Quest m) => m.ID == dependencyQuestID);
					if (quest != null && quest.Status != QuestStatus.Completed)
					{
						flag2 = false;
						break;
					}
				}
			}
			if (flag2)
			{
				Client.Instance.CanvasManager.CanvasUI.QuestIndicators.AddAvailableQuestIndicator(this);
			}
		}

		public void OnQuestTriggerUpdate()
		{
			if (Collider != null)
			{
				Collider.enabled = false;
			}
			if (QuestTriggerObject != null)
			{
				QuestTriggerObject.SetActive(questTrigger.Status == QuestStatus.Active);
			}
			if (questTrigger.Status == QuestStatus.Active)
			{
				if (Collider != null)
				{
					Collider.enabled = true;
				}
				ActiveTriggerEvent.Invoke();
			}
			if (questTrigger.Status == QuestStatus.Completed)
			{
				CompleteTriggerEvent.Invoke();
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, base.transform.lossyScale);
			if (GetComponent<BoxCollider>() != null)
			{
				Gizmos.color = new Color(1f, 0f, 1f, 0.05f);
				Gizmos.DrawCube(GetComponent<BoxCollider>().center, GetComponent<BoxCollider>().size);
				Gizmos.color = new Color(1f, 0f, 1f, 0.3f);
				Gizmos.DrawWireCube(GetComponent<BoxCollider>().center, GetComponent<BoxCollider>().size);
			}
			if (Visibility != QuestIndicatorVisibility.AlwaysVisible && Visibility != QuestIndicatorVisibility.Proximity)
			{
				return;
			}
			if (Task != null)
			{
				if (Task.QuestTriggerID == 0)
				{
					Gizmos.DrawIcon(base.transform.TransformPoint(QuestIndicatorPosition), "NewQuestIndicator");
				}
				else
				{
					Gizmos.DrawIcon(base.transform.TransformPoint(QuestIndicatorPosition), "QuestTrigger");
				}
			}
			else
			{
				Gizmos.DrawIcon(base.transform.TransformPoint(QuestIndicatorPosition), "QuestTrigger");
			}
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, base.transform.lossyScale);
			if (Visibility == QuestIndicatorVisibility.Proximity)
			{
				Gizmos.DrawWireSphere(Vector3.zero, ProximityDistance);
			}
		}

		public static void Check(GameObject go, SceneQuestTriggerEvent triggerEvent)
		{
			if (go.GetComponents<SceneQuestTrigger>() == null)
			{
				return;
			}
			foreach (SceneQuestTrigger item in from m in go.GetComponents<SceneQuestTrigger>()
				where m.TriggerEvent == triggerEvent && m.QuestTrigger != null && m.QuestTrigger.Status == QuestStatus.Active
				select m)
			{
				item.Activate();
			}
		}

		public static void CheckInChildren(GameObject go, SceneQuestTriggerEvent triggerEvent)
		{
			if (go.GetComponentsInChildren<SceneQuestTrigger>() == null)
			{
				return;
			}
			foreach (SceneQuestTrigger item in from m in go.GetComponentsInChildren<SceneQuestTrigger>()
				where m.TriggerEvent == triggerEvent && m.QuestTrigger != null && m.QuestTrigger.Status == QuestStatus.Active
				select m)
			{
				item.Activate();
			}
		}
	}
}
