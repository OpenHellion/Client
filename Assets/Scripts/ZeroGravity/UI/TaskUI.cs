using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class TaskUI : MonoBehaviour
	{
		public Text Name;

		public Text Description;

		public Animator Animator;

		public GameObject CompletedObject;

		public QuestTracker QuestTracker;

		public QuestTrigger NextTrigger;

		public void OnCompletedTask()
		{
			gameObject.SetActive(false);
			QuestTracker.RefreshBatch();
		}

		public void OnHideTask()
		{
			gameObject.SetActive(false);
		}
	}
}
