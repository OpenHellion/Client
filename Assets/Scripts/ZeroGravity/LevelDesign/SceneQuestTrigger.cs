using System.Collections.Generic;
using System.Linq;
using OpenHellion.Net;
using UnityEngine;
using UnityEngine.Events;
using ZeroGravity.Data;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class SceneQuestTrigger : MonoBehaviour
	{
		[Tooltip("Used only if Task is not assigned"), ShowIf(nameof(Task), null)]
		public uint QuestID;

		[Tooltip("Used only if Task is not assigned"), ShowIf(nameof(Task), null)]
		public uint QuestTriggerID;

		[Tooltip("The event that should complete this quest.")]
		public SceneQuestTriggerEvent TriggerEvent;

		[Tooltip("The quest task to complete.")]
		public QuestTaskObject Task;

		[Tooltip("A list of additional quest tasks to complete if Task is completed."), HideIf(nameof(Task), null)]
		public List<QuestTaskObject> AdditionalTasksToComplete;

		[Tooltip("Wether or not the quest indicator is visible.")]
		public QuestIndicatorVisibility Visibility = QuestIndicatorVisibility.AlwaysVisible;

		[Tooltip("The maximum distance the quest indicator can be seen from."), ShowIf(nameof(Visibility), QuestIndicatorVisibility.Proximity)]
		public float ProximityDistance = 10f;

		[Tooltip("Position of the quest indicator relative to the object.")]
		public Vector3 QuestIndicatorPosition = new Vector3(0f, 1f, 0f);

		public GameObject QuestTriggerObject;

		[Tooltip("Collider enabled when quest is activated.")]
		public Collider Collider;

		[Tooltip("An optional localization key for a tooltip to be shown on the screen.")]
		public string QuickTooltip;

		public UnityEvent PostTriggerEvent;

		[Tooltip("Events executed when task becomes active.")]
		public UnityEvent ActiveTriggerEvent;

		[Tooltip("Events executed when task is completed.")]
		public UnityEvent CompleteTriggerEvent;

		private Quest m_quest;

		private QuestTrigger m_questTrigger;

		private SpaceObjectVessel m_parentVessel;

		private double m_lastActivationTime;

		public QuestTrigger QuestTrigger => m_questTrigger;

		public SpaceObjectVessel ParentVessel => m_parentVessel;

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
				m_quest = Client.Instance.Quests.FirstOrDefault((Quest m) => m.ID == QuestID);
				m_questTrigger = m_quest.QuestTriggers.FirstOrDefault((QuestTrigger m) => m.ID == QuestTriggerID);
				m_parentVessel = gameObject.GetComponentInParent<SpaceObjectVessel>();
				OnQuestTriggerUpdate();
				UpdateQuestTriggerMarker();
				UpdateAvailableQuestMarker();
				if (TriggerEvent == SceneQuestTriggerEvent.EnterTrigger || TriggerEvent == SceneQuestTriggerEvent.ExitTrigger)
				{
					gameObject.SetLayerRecursively("Triggers");
				}
				if (TriggerEvent == SceneQuestTriggerEvent.Interact)
				{
					gameObject.SetLayerRecursively("InteractiveTriggers");
				}
			}
			else
			{
				Dbg.Error("Scene Quest Trigger " + gameObject.name + " has no Task assigned!");
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

		public void CompleteQuest()
		{
			if ((double)Time.realtimeSinceStartup - m_lastActivationTime < 1.0 || (m_quest.DependencyQuests != null && Client.Instance.Quests.FirstOrDefault((Quest m) => m_quest.DependencyQuests.Contains(m.ID) && m.Status != QuestStatus.Completed) != null))
			{
				return;
			}
			m_lastActivationTime = Time.realtimeSinceStartup;
			if (m_quest.IsFinished || m_questTrigger == null || m_questTrigger.Status != QuestStatus.Active || !m_questTrigger.CheckLocation(ParentVessel))
			{
				return;
			}
			NetworkController.Instance.SendToGameServer(new QuestTriggerMessage
			{
				QuestID = QuestID,
				TriggerID = QuestTriggerID
			});
			foreach (QuestTaskObject item in AdditionalTasksToComplete)
			{
				NetworkController.Instance.SendToGameServer(new QuestTriggerMessage
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
				if (this.m_quest == quest && m_questTrigger.Status == QuestStatus.Active && SceneHelper.CompareTags(ParentVessel.VesselData.Tag, SceneTagObject.TagsToString(Task.Tags)))
				{
					Client.Instance.CanvasManager.CanvasUI.QuestIndicators.AddQuestIndicator(this);
				}
				else if ((m_questTrigger.Type != QuestTriggerType.Activate || m_questTrigger.Status != QuestStatus.Active) && SceneHelper.CompareTags(ParentVessel.VesselData.Tag, SceneTagObject.TagsToString(Task.Tags)))
				{
					Client.Instance.CanvasManager.CanvasUI.QuestIndicators.StopQuestIndicator(this);
				}
			}
			else if (m_questTrigger.Type != QuestTriggerType.Activate || m_questTrigger.Status != QuestStatus.Active)
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
			if (m_questTrigger.Type != QuestTriggerType.Activate || m_questTrigger.Status != QuestStatus.Active || !flag)
			{
				return;
			}
			bool flag2 = true;
			if (m_questTrigger.Quest != null && m_questTrigger.Quest.DependencyQuests != null)
			{
				foreach (uint dependencyQuestID in m_questTrigger.Quest.DependencyQuests)
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
				QuestTriggerObject.SetActive(m_questTrigger.Status == QuestStatus.Active);
			}
			if (m_questTrigger.Status == QuestStatus.Active)
			{
				if (Collider != null)
				{
					Collider.enabled = true;
				}
				ActiveTriggerEvent.Invoke();
			}
			if (m_questTrigger.Status == QuestStatus.Completed)
			{
				CompleteTriggerEvent.Invoke();
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
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
					Gizmos.DrawIcon(transform.TransformPoint(QuestIndicatorPosition), "NewQuestIndicator");
				}
				else
				{
					Gizmos.DrawIcon(transform.TransformPoint(QuestIndicatorPosition), "QuestTrigger", false);
				}
			}
			else
			{
				Gizmos.DrawIcon(transform.TransformPoint(QuestIndicatorPosition), "QuestTrigger", false);
			}
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
			if (Visibility == QuestIndicatorVisibility.Proximity)
			{
				Gizmos.DrawWireSphere(Vector3.zero, ProximityDistance);
			}
		}

		/// <summary>
		/// 	Checks if the quests on the provided gameobject can be completed. Completes them if the trigger event is the same.
		/// </summary>
		public static void OnTrigger(GameObject go, SceneQuestTriggerEvent triggerEvent)
		{
			if (go.GetComponents<SceneQuestTrigger>() == null)
			{
				return;
			}
			foreach (SceneQuestTrigger item in from m in go.GetComponents<SceneQuestTrigger>()
				where m.TriggerEvent == triggerEvent && m.QuestTrigger != null && m.QuestTrigger.Status == QuestStatus.Active
				select m)
			{
				item.CompleteQuest();
			}
		}

		/// <summary>
		/// 	Checks if the quests on children of the provided gameobject can be completed. Completes them if the trigger event is the same.
		/// </summary>
		public static void OnTriggerInChildren(GameObject go, SceneQuestTriggerEvent triggerEvent)
		{
			if (go.GetComponentsInChildren<SceneQuestTrigger>() == null)
			{
				return;
			}
			foreach (SceneQuestTrigger item in from m in go.GetComponentsInChildren<SceneQuestTrigger>()
				where m.TriggerEvent == triggerEvent && m.QuestTrigger != null && m.QuestTrigger.Status == QuestStatus.Active
				select m)
			{
				item.CompleteQuest();
			}
		}
	}
}
