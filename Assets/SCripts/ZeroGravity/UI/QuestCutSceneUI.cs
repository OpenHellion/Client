using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public class QuestCutSceneUI : MonoBehaviour
	{
		public Image Icon;

		public Text Content;

		public Text CharacterName;

		public AudioClip Audio;

		public bool AutoDestroy = true;

		public float Treshold;

		private float timer;

		public Animator Animator;

		private void Start()
		{
		}

		private void Update()
		{
			if (AutoDestroy)
			{
				timer += Time.deltaTime;
				if (timer > Treshold)
				{
					Animator.SetTrigger("Stop");
				}
			}
		}
	}
}
