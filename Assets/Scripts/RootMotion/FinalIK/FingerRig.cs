using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public class FingerRig : SolverManager
	{
		[Tooltip("The master weight for all fingers.")]
		[Range(0f, 1f)]
		public float weight = 1f;

		public Finger[] fingers = new Finger[0];

		public bool initiated { get; private set; }

		public bool IsValid(ref string errorMessage)
		{
			Finger[] array = fingers;
			foreach (Finger finger in array)
			{
				if (!finger.IsValid(ref errorMessage))
				{
					return false;
				}
			}
			return true;
		}

		[ContextMenu("Auto-detect")]
		public void AutoDetect()
		{
			fingers = new Finger[0];
			for (int i = 0; i < base.transform.childCount; i++)
			{
				Transform[] array = new Transform[0];
				AddChildrenRecursive(base.transform.GetChild(i), ref array);
				if (array.Length == 3 || array.Length == 4)
				{
					Finger finger = new Finger();
					finger.bone1 = array[0];
					finger.bone2 = array[1];
					if (array.Length == 3)
					{
						finger.tip = array[2];
					}
					else
					{
						finger.bone3 = array[2];
						finger.tip = array[3];
					}
					finger.weight = 1f;
					Array.Resize(ref fingers, fingers.Length + 1);
					fingers[fingers.Length - 1] = finger;
				}
			}
		}

		public void AddFinger(Transform bone1, Transform bone2, Transform bone3, Transform tip, Transform target = null)
		{
			Finger finger = new Finger();
			finger.bone1 = bone1;
			finger.bone2 = bone2;
			finger.bone3 = bone3;
			finger.tip = tip;
			finger.target = target;
			Array.Resize(ref fingers, fingers.Length + 1);
			fingers[fingers.Length - 1] = finger;
			initiated = false;
			finger.Initiate(base.transform, fingers.Length - 1);
			if (fingers[fingers.Length - 1].initiated)
			{
				initiated = true;
			}
		}

		public void RemoveFinger(int index)
		{
			if ((float)index < 0f || index >= fingers.Length)
			{
				Warning.Log("RemoveFinger index out of bounds.", base.transform);
				return;
			}
			if (fingers.Length == 1)
			{
				fingers = new Finger[0];
				return;
			}
			Finger[] array = new Finger[fingers.Length - 1];
			int num = 0;
			for (int i = 0; i < fingers.Length; i++)
			{
				if (i != index)
				{
					array[num] = fingers[i];
					num++;
				}
			}
			fingers = array;
		}

		private void AddChildrenRecursive(Transform parent, ref Transform[] array)
		{
			Array.Resize(ref array, array.Length + 1);
			array[array.Length - 1] = parent;
			if (parent.childCount == 1)
			{
				AddChildrenRecursive(parent.GetChild(0), ref array);
			}
		}

		protected override void InitiateSolver()
		{
			initiated = true;
			for (int i = 0; i < fingers.Length; i++)
			{
				fingers[i].Initiate(base.transform, i);
				if (!fingers[i].initiated)
				{
					initiated = false;
				}
			}
		}

		public void UpdateFingerSolvers()
		{
			if (!(weight <= 0f))
			{
				Finger[] array = fingers;
				foreach (Finger finger in array)
				{
					finger.Update(weight);
				}
			}
		}

		public void FixFingerTransforms()
		{
			Finger[] array = fingers;
			foreach (Finger finger in array)
			{
				finger.FixTransforms();
			}
		}

		protected override void UpdateSolver()
		{
			UpdateFingerSolvers();
		}

		protected override void FixTransforms()
		{
			FixFingerTransforms();
		}
	}
}
