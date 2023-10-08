using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public class GuiAnimations : MonoBehaviour
	{
		public enum SwitchOrder
		{
			Altogether = 0,
			Forward = 1,
			Backward = 2,
			Random = 3
		}

		public enum GuiAnimationType
		{
			Transparency = 0,
			Translation = 1,
			Rotation = 2,
			ScaleX = 3,
			ScaleY = 4
		}

		[Serializable]
		private class GuiAnimationData
		{
			public AnimationCurve Curve;

			[Tooltip("Time in seconds")] public float Time = 1f;

			[Tooltip("Time in seconds")] public float TransitionTime;

			public SwitchOrder Order = SwitchOrder.Forward;

			public int RandomOrderSeed = 1;
		}

		[Serializable]
		private class GuiAnimationItem
		{
			public Image Image;

			public Vector3 StartPosition = Vector3.zero;

			public Text Text;

			public Text Text2;

			public Material Material;

			public void Transparency(float multiplier)
			{
				if (!(Image != null))
				{
					return;
				}

				Color color = Image.color;
				color.a = multiplier;
				Image.color = color;
				if (Text != null)
				{
					Color color2 = Text.color;
					color2.a = multiplier;
					Text.color = color2;
					if (Text2 != null)
					{
						Text2.color = color2;
					}
				}
			}

			public void Translate(float multiply, float x, float y)
			{
				if (StartPosition == Vector3.zero)
				{
					StartPosition = Image.transform.localPosition;
				}

				Vector3 b = new Vector3(StartPosition.x + x, StartPosition.y + y, StartPosition.z);
				Image.transform.localPosition = Vector3.Lerp(StartPosition, b, multiply);
			}

			public void Rotate(float multiply, float from, float to)
			{
				Image.transform.eulerAngles =
					Vector3.Lerp(new Vector3(0f, 0f, from), new Vector3(0f, 0f, to), multiply);
			}

			public void ScaleX(float multiply, float from, float to, float lastKnownY)
			{
				Image.transform.localScale = Vector3.Lerp(new Vector3(from, lastKnownY, 1f),
					new Vector3(to, lastKnownY, 1f), multiply);
			}

			public void ScaleY(float multiply, float from, float to, float lastKnownX)
			{
				Image.transform.localScale = Vector3.Lerp(new Vector3(lastKnownX, from, 1f),
					new Vector3(lastKnownX, to, 1f), multiply);
			}
		}

		public Transform AnimationEndTransfrom;

		[SerializeField] private List<GuiAnimationItem> guiAnimItems;

		public bool Loop;

		public bool IsRunning;

		public bool DoTransparency;

		private bool DoTransparencyAnimation;

		[SerializeField] private GuiAnimationData animationTransparency;

		private float currentCurveTimeTransparency;

		private int currentIndexTransparency;

		private List<int> currentOrderIndexListTransparency;

		public bool DoTranslation;

		private bool DoTranslationAnimation;

		[SerializeField] private GuiAnimationData animationTranslation;

		private float currentCurveTimeTranslation;

		private int currentIndexTranslation;

		private List<int> currentOrderIndexListTranslation;

		private float xMovement;

		private float yMovement;

		public bool DoRotation;

		private bool DoRotationAnimation;

		[SerializeField] private GuiAnimationData animationRotation;

		private float currentCurveTimeRotation;

		private int currentIndexRotation;

		private List<int> currentOrderIndexListRotation;

		private float zRotFrom;

		private float zRotTo;

		public bool DoScalingX;

		private bool DoScalingXAnimation;

		[SerializeField] private GuiAnimationData animationScalingX;

		private float currentCurveTimeScalingX;

		private int currentIndexScalingX;

		private List<int> currentOrderIndexListScalingX;

		private float scalingXFrom;

		private float scalingXTo;

		private float lastKnownYScale = 1f;

		public bool DoScalingY;

		private bool DoScalingYAnimation;

		[SerializeField] private GuiAnimationData animationScalingY;

		private float currentCurveTimeScalingY;

		private int currentIndexScalingY;

		private List<int> currentOrderIndexListScalingY;

		private float scalingYFrom;

		private float scalingYTo;

		private float lastKnownXScale = 1f;

		private Material materijal;

		private bool hasCalculated;

		public Material DefaultMaterial;

		private void Start()
		{
			FillSwitchIndexOrder(ref currentOrderIndexListTransparency, animationTransparency.Order,
				animationTransparency.RandomOrderSeed);
			FillSwitchIndexOrder(ref currentOrderIndexListTranslation, animationTranslation.Order,
				animationTranslation.RandomOrderSeed);
			FillSwitchIndexOrder(ref currentOrderIndexListRotation, animationRotation.Order,
				animationRotation.RandomOrderSeed);
			FillSwitchIndexOrder(ref currentOrderIndexListScalingX, animationScalingX.Order,
				animationScalingX.RandomOrderSeed);
			FillSwitchIndexOrder(ref currentOrderIndexListScalingY, animationScalingY.Order,
				animationScalingY.RandomOrderSeed);
		}

		private void Update()
		{
			if (DoTransparencyAnimation)
			{
				UpdateAnimation(animationTransparency, GuiAnimationType.Transparency, ref currentCurveTimeTransparency,
					ref currentOrderIndexListTransparency, ref currentIndexTransparency);
			}

			if (DoTranslationAnimation)
			{
				UpdateAnimation(animationTranslation, GuiAnimationType.Translation, ref currentCurveTimeTranslation,
					ref currentOrderIndexListTranslation, ref currentIndexTranslation);
			}

			if (DoRotationAnimation)
			{
				UpdateAnimation(animationRotation, GuiAnimationType.Rotation, ref currentCurveTimeRotation,
					ref currentOrderIndexListRotation, ref currentIndexRotation);
			}

			if (DoScalingXAnimation)
			{
				UpdateAnimation(animationScalingX, GuiAnimationType.ScaleX, ref currentCurveTimeScalingX,
					ref currentOrderIndexListScalingX, ref currentIndexScalingX);
			}

			if (DoScalingYAnimation)
			{
				UpdateAnimation(animationScalingY, GuiAnimationType.ScaleY, ref currentCurveTimeScalingY,
					ref currentOrderIndexListScalingY, ref currentIndexScalingY);
			}
		}

		public void SetSwitchOrder(SwitchOrder order)
		{
			FillSwitchIndexOrder(ref currentOrderIndexListTransparency, order, animationTransparency.RandomOrderSeed);
			FillSwitchIndexOrder(ref currentOrderIndexListTranslation, order, animationTranslation.RandomOrderSeed);
			FillSwitchIndexOrder(ref currentOrderIndexListRotation, order, animationRotation.RandomOrderSeed);
			FillSwitchIndexOrder(ref currentOrderIndexListScalingX, order, animationScalingX.RandomOrderSeed);
			FillSwitchIndexOrder(ref currentOrderIndexListScalingY, order, animationScalingY.RandomOrderSeed);
		}

		private void FillSwitchIndexOrder(ref List<int> list, SwitchOrder order, int randomSeed)
		{
			list = new List<int>(new int[guiAnimItems.Count]);
			switch (order)
			{
				case SwitchOrder.Forward:
				case SwitchOrder.Random:
				{
					for (int i = 0; i < guiAnimItems.Count; i++)
					{
						list[i] = i;
					}

					break;
				}
				case SwitchOrder.Backward:
				{
					int num = 0;
					for (int num2 = guiAnimItems.Count - 1; num2 >= 0; num2--)
					{
						list[num++] = num2;
					}

					break;
				}
			}

			if (order == SwitchOrder.Random)
			{
				System.Random random = new System.Random(randomSeed);
				for (int num3 = guiAnimItems.Count - 1; num3 >= 0; num3--)
				{
					int index = random.Next(0, guiAnimItems.Count);
					int value = list[index];
					list[index] = list[num3];
					list[num3] = value;
				}
			}
		}

		private void UpdateAnimation(GuiAnimationData animationData, GuiAnimationType animType,
			ref float currentCurveTime, ref List<int> indexList, ref int currentIndex)
		{
			currentCurveTime += 1f / animationData.Time * Time.deltaTime;
			float num = animationData.Curve.Evaluate(Mathf.Clamp01(currentCurveTime));
			if (animationData.Order == SwitchOrder.Altogether)
			{
				foreach (GuiAnimationItem guiAnimItem in guiAnimItems)
				{
					switch (animType)
					{
						case GuiAnimationType.Transparency:
							guiAnimItem.Transparency(num);
							break;
						case GuiAnimationType.Translation:
							guiAnimItem.Translate(num, xMovement, yMovement);
							break;
						case GuiAnimationType.Rotation:
							guiAnimItem.Rotate(num, zRotFrom, zRotTo);
							break;
						case GuiAnimationType.ScaleX:
							guiAnimItem.ScaleX(num, scalingXFrom, scalingXTo,
								guiAnimItems[0].Image.transform.localScale.y);
							break;
						case GuiAnimationType.ScaleY:
							guiAnimItem.ScaleY(num, scalingYFrom, scalingYTo,
								guiAnimItems[0].Image.transform.localScale.x);
							break;
					}
				}
			}
			else
			{
				switch (animType)
				{
					case GuiAnimationType.Transparency:
						guiAnimItems[indexList[currentIndex]].Transparency(num);
						break;
					case GuiAnimationType.Translation:
						guiAnimItems[indexList[currentIndex]].Translate(num, xMovement, yMovement);
						break;
					case GuiAnimationType.Rotation:
						guiAnimItems[indexList[currentIndex]].Rotate(num, zRotFrom, zRotTo);
						break;
					case GuiAnimationType.ScaleX:
						guiAnimItems[indexList[currentIndex]].ScaleX(num, scalingXFrom, scalingXTo,
							guiAnimItems[0].Image.transform.localScale.y);
						break;
					case GuiAnimationType.ScaleY:
						guiAnimItems[indexList[currentIndex]].ScaleY(num, scalingYFrom, scalingYTo,
							guiAnimItems[0].Image.transform.localScale.x);
						break;
				}

				if (animationData.TransitionTime > 0.0001f && currentIndex < guiAnimItems.Count - 1)
				{
					float num2 = animationData.TransitionTime / animationData.Time;
					float num3 = currentCurveTime - (1f - num2);
					if (num3 >= 0f)
					{
						AnimateOtherItemRecursive(animationData, animType, currentIndex, num3, ref indexList);
					}
				}
			}

			if (!(currentCurveTime >= 1f))
			{
				return;
			}

			currentCurveTime = animationData.TransitionTime / animationData.Time + currentCurveTime % 1f;
			currentIndex++;
			if (animationData.Order == SwitchOrder.Altogether || currentIndex == guiAnimItems.Count)
			{
				switch (animType)
				{
					case GuiAnimationType.Transparency:
						DoTransparencyAnimation = Loop;
						break;
					case GuiAnimationType.Translation:
						DoTranslationAnimation = Loop;
						break;
					case GuiAnimationType.Rotation:
						DoRotationAnimation = Loop;
						break;
					case GuiAnimationType.ScaleX:
						DoScalingXAnimation = Loop;
						break;
					case GuiAnimationType.ScaleY:
						DoScalingYAnimation = Loop;
						break;
				}

				currentIndex = 0;
				currentCurveTime = 0f;
			}
		}

		private void AnimateOtherItemRecursive(GuiAnimationData animationData, GuiAnimationType animType,
			int currentIndex, float tmpTransitionTime, ref List<int> indexList)
		{
			int num = currentIndex + 1;
			float num2 = animationData.Curve.Evaluate(tmpTransitionTime);
			switch (animType)
			{
				case GuiAnimationType.Transparency:
					guiAnimItems[indexList[num]].Transparency(num2);
					break;
				case GuiAnimationType.Translation:
					guiAnimItems[indexList[num]].Translate(num2, xMovement, yMovement);
					break;
				case GuiAnimationType.Rotation:
					guiAnimItems[indexList[num]].Rotate(num2, zRotFrom, zRotTo);
					break;
				case GuiAnimationType.ScaleX:
					guiAnimItems[indexList[num]].ScaleX(num2, scalingXFrom, scalingXTo,
						guiAnimItems[0].Image.transform.localScale.y);
					break;
				case GuiAnimationType.ScaleY:
					guiAnimItems[indexList[num]].ScaleY(num2, scalingYFrom, scalingYTo,
						guiAnimItems[0].Image.transform.localScale.x);
					break;
			}

			tmpTransitionTime -= 1f - animationData.TransitionTime / animationData.Time;
			if (tmpTransitionTime > 0f && num < guiAnimItems.Count - 1)
			{
				AnimateOtherItemRecursive(animationData, animType, num, tmpTransitionTime, ref indexList);
			}
		}

		public void CalculateDifrence()
		{
			if (guiAnimItems.Count <= 0 || guiAnimItems[0] == null || hasCalculated)
			{
				return;
			}

			if (AnimationEndTransfrom != null)
			{
				xMovement = AnimationEndTransfrom.localPosition.x - guiAnimItems[0].Image.transform.localPosition.x;
				yMovement = AnimationEndTransfrom.localPosition.y - guiAnimItems[0].Image.transform.localPosition.y;
				zRotFrom = guiAnimItems[0].Image.GetComponent<RectTransform>().localRotation.eulerAngles.z;
				zRotTo = AnimationEndTransfrom.GetComponent<RectTransform>().localRotation.eulerAngles.z;
				lastKnownYScale = guiAnimItems[0].Image.transform.localScale.y;
				lastKnownXScale = guiAnimItems[0].Image.transform.localScale.x;
				scalingXFrom = guiAnimItems[0].Image.transform.localScale.x;
				scalingXTo = AnimationEndTransfrom.transform.localScale.x;
				scalingYFrom = guiAnimItems[0].Image.transform.localScale.y;
				scalingYTo = AnimationEndTransfrom.transform.localScale.y;
			}

			for (int i = 0; i < guiAnimItems.Count; i++)
			{
				if (guiAnimItems[i].Image.transform.GetComponent<Button>() != null)
				{
					guiAnimItems[i].Material = UnityEngine.Object.Instantiate(guiAnimItems[i].Image.material);
					guiAnimItems[i].Image.material = guiAnimItems[i].Material;
				}
			}

			hasCalculated = true;
		}

		public float GetAnimationTime(GuiAnimationType type)
		{
			switch (type)
			{
				case GuiAnimationType.Transparency:
					return animationTransparency.Time;
				case GuiAnimationType.Translation:
					return animationTranslation.Time;
				case GuiAnimationType.Rotation:
					return animationRotation.Time;
				case GuiAnimationType.ScaleX:
					return animationScalingX.Time;
				default:
					return animationScalingY.Time;
			}
		}

		public void StartAnimation()
		{
			IsRunning = true;
			CalculateDifrence();
			DoTransparencyAnimation = DoTransparency;
			DoTranslationAnimation = DoTranslation;
			DoRotationAnimation = DoRotation;
			DoScalingXAnimation = DoScalingX;
			DoScalingYAnimation = DoScalingY;
		}

		public void EndAnimation()
		{
			IsRunning = false;
			DoTransparencyAnimation = false;
			DoTranslationAnimation = false;
			DoRotationAnimation = false;
			DoScalingXAnimation = false;
			DoScalingYAnimation = false;
		}

		public void TestDodajUListu()
		{
			if (guiAnimItems.Count >= 1)
			{
				return;
			}

			IEnumerator enumerator = base.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Transform transform = (Transform)enumerator.Current;
					if (transform.GetComponent<Image>() != null && transform.gameObject.activeInHierarchy)
					{
						GuiAnimationItem guiAnimationItem = new GuiAnimationItem();
						guiAnimationItem.Image = transform.GetComponent<Image>();
						guiAnimationItem.Text = ((!(transform.GetComponent<Button>() != null))
							? null
							: transform.transform.Find("Text").GetComponent<Text>());
						GuiAnimationItem item = guiAnimationItem;
						if (!guiAnimItems.Contains(item))
						{
							guiAnimItems.Add(item);
						}
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
		}

		public void AddOneItem(GameObject item)
		{
			if (item.GetComponent<Image>() != null && item.gameObject.activeInHierarchy)
			{
				GuiAnimationItem guiAnimationItem = new GuiAnimationItem();
				guiAnimationItem.Image = item.GetComponent<Image>();
				guiAnimationItem.Text = ((!(item.GetComponent<Button>() != null))
					? null
					: item.transform.Find("Text").GetComponent<Text>());
				guiAnimationItem.Text2 =
					((!(item.transform.Find("Text2") != null) ||
					  !(item.transform.Find("Text2").GetComponent<Text>() != null))
						? null
						: item.transform.Find("Text2").GetComponent<Text>());
				GuiAnimationItem item2 = guiAnimationItem;
				if (!guiAnimItems.Contains(item2))
				{
					guiAnimItems.Add(item2);
				}
			}
		}

		public void FillUpIndexList()
		{
			FillSwitchIndexOrder(ref currentOrderIndexListTransparency, animationTransparency.Order,
				animationTransparency.RandomOrderSeed);
			FillSwitchIndexOrder(ref currentOrderIndexListTranslation, animationTranslation.Order,
				animationTranslation.RandomOrderSeed);
			FillSwitchIndexOrder(ref currentOrderIndexListRotation, animationRotation.Order,
				animationRotation.RandomOrderSeed);
			FillSwitchIndexOrder(ref currentOrderIndexListScalingX, animationScalingX.Order,
				animationScalingX.RandomOrderSeed);
			FillSwitchIndexOrder(ref currentOrderIndexListScalingY, animationScalingY.Order,
				animationScalingY.RandomOrderSeed);
		}

		public void ClearLists()
		{
			guiAnimItems.Clear();
			currentOrderIndexListTransparency.Clear();
			currentOrderIndexListTranslation.Clear();
			currentOrderIndexListRotation.Clear();
			currentOrderIndexListScalingX.Clear();
			currentOrderIndexListScalingY.Clear();
			currentIndexTransparency = 0;
			currentIndexTranslation = 0;
			currentIndexRotation = 0;
			currentIndexScalingX = 0;
			currentIndexScalingY = 0;
		}

		public void SetDefaultMaterial()
		{
			foreach (GuiAnimationItem guiAnimItem in guiAnimItems)
			{
				if (guiAnimItem.Image.GetComponent<Button>() != null && guiAnimItem.Image.gameObject.activeInHierarchy)
				{
					Material material = (guiAnimItem.Material = UnityEngine.Object.Instantiate(DefaultMaterial));
					guiAnimItem.Image.material = material;
				}
			}
		}
	}
}
