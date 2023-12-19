using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class BasketballScoreCounter : MonoBehaviour
	{
		private int _Counter;

		public int Counter
		{
			get => _Counter;
			set
			{
				_Counter = value;
				SetScoreText(_Counter.ToString());
			}
		}

		private void OnTriggerEnter(Collider coli)
		{
			GenericItem componentInParent = coli.gameObject.GetComponentInParent<GenericItem>();
			if (componentInParent != null && componentInParent.SubType == GenericItemSubType.BasketBall &&
			    componentInParent.DynamicObj.Master && coli.gameObject.name == "ScoreTrigger")
			{
				Counter++;
			}
		}

		private void SetScoreText(string text)
		{
			Item componentInParent = GetComponentInParent<Item>();
			if (componentInParent != null && componentInParent.AttachPoint != null)
			{
				SceneNameTag[] componentsInChildren =
					componentInParent.AttachPoint.GetComponentsInChildren<SceneNameTag>();
				foreach (SceneNameTag sceneNameTag in componentsInChildren)
				{
					sceneNameTag.SetNameTagText(text);
				}
			}
		}

		public void ResetCounter()
		{
			Counter = 0;
		}

		public void ClearCounter()
		{
			SetScoreText(" ");
		}
	}
}
