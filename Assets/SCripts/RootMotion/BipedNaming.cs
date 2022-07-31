using System;
using UnityEngine;

namespace RootMotion
{
	public static class BipedNaming
	{
		[Serializable]
		public enum BoneType
		{
			Unassigned = 0,
			Spine = 1,
			Head = 2,
			Arm = 3,
			Leg = 4,
			Tail = 5,
			Eye = 6
		}

		[Serializable]
		public enum BoneSide
		{
			Center = 0,
			Left = 1,
			Right = 2
		}

		public static string[] typeLeft = new string[9] { " L ", "_L_", "-L-", " l ", "_l_", "-l-", "Left", "left", "CATRigL" };

		public static string[] typeRight = new string[9] { " R ", "_R_", "-R-", " r ", "_r_", "-r-", "Right", "right", "CATRigR" };

		public static string[] typeSpine = new string[16]
		{
			"Spine", "spine", "Pelvis", "pelvis", "Root", "root", "Torso", "torso", "Body", "body",
			"Hips", "hips", "Neck", "neck", "Chest", "chest"
		};

		public static string[] typeHead = new string[2] { "Head", "head" };

		public static string[] typeArm = new string[10] { "Arm", "arm", "Hand", "hand", "Wrist", "Wrist", "Elbow", "elbow", "Palm", "palm" };

		public static string[] typeLeg = new string[16]
		{
			"Leg", "leg", "Thigh", "thigh", "Calf", "calf", "Femur", "femur", "Knee", "knee",
			"Foot", "foot", "Ankle", "ankle", "Hip", "hip"
		};

		public static string[] typeTail = new string[2] { "Tail", "tail" };

		public static string[] typeEye = new string[2] { "Eye", "eye" };

		public static string[] typeExclude = new string[6] { "Nub", "Dummy", "dummy", "Tip", "IK", "Mesh" };

		public static string[] typeExcludeSpine = new string[2] { "Head", "head" };

		public static string[] typeExcludeHead = new string[2] { "Top", "End" };

		public static string[] typeExcludeArm = new string[19]
		{
			"Collar", "collar", "Clavicle", "clavicle", "Finger", "finger", "Index", "index", "Mid", "mid",
			"Pinky", "pinky", "Ring", "Thumb", "thumb", "Adjust", "adjust", "Twist", "twist"
		};

		public static string[] typeExcludeLeg = new string[7] { "Toe", "toe", "Platform", "Adjust", "adjust", "Twist", "twist" };

		public static string[] typeExcludeTail = Array.Empty<string>();

		public static string[] typeExcludeEye = new string[6] { "Lid", "lid", "Brow", "brow", "Lash", "lash" };

		public static string[] pelvis = new string[4] { "Pelvis", "pelvis", "Hip", "hip" };

		public static string[] hand = new string[6] { "Hand", "hand", "Wrist", "wrist", "Palm", "palm" };

		public static string[] foot = new string[4] { "Foot", "foot", "Ankle", "ankle" };

		public static Transform[] GetBonesOfType(BoneType boneType, Transform[] bones)
		{
			Transform[] array = new Transform[0];
			foreach (Transform transform in bones)
			{
				if (transform != null && GetBoneType(transform.name) == boneType)
				{
					Array.Resize(ref array, array.Length + 1);
					array[array.Length - 1] = transform;
				}
			}
			return array;
		}

		public static Transform[] GetBonesOfSide(BoneSide boneSide, Transform[] bones)
		{
			Transform[] array = new Transform[0];
			foreach (Transform transform in bones)
			{
				if (transform != null && GetBoneSide(transform.name) == boneSide)
				{
					Array.Resize(ref array, array.Length + 1);
					array[array.Length - 1] = transform;
				}
			}
			return array;
		}

		public static Transform[] GetBonesOfTypeAndSide(BoneType boneType, BoneSide boneSide, Transform[] bones)
		{
			Transform[] bonesOfType = GetBonesOfType(boneType, bones);
			return GetBonesOfSide(boneSide, bonesOfType);
		}

		public static Transform GetFirstBoneOfTypeAndSide(BoneType boneType, BoneSide boneSide, Transform[] bones)
		{
			Transform[] bonesOfTypeAndSide = GetBonesOfTypeAndSide(boneType, boneSide, bones);
			if (bonesOfTypeAndSide.Length == 0)
			{
				return null;
			}
			return bonesOfTypeAndSide[0];
		}

		public static Transform GetNamingMatch(Transform[] transforms, params string[][] namings)
		{
			foreach (Transform transform in transforms)
			{
				bool flag = true;
				foreach (string[] namingConvention in namings)
				{
					if (!matchesNaming(transform.name, namingConvention))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return transform;
				}
			}
			return null;
		}

		public static BoneType GetBoneType(string boneName)
		{
			if (isSpine(boneName))
			{
				return BoneType.Spine;
			}
			if (isHead(boneName))
			{
				return BoneType.Head;
			}
			if (isArm(boneName))
			{
				return BoneType.Arm;
			}
			if (isLeg(boneName))
			{
				return BoneType.Leg;
			}
			if (isTail(boneName))
			{
				return BoneType.Tail;
			}
			if (isEye(boneName))
			{
				return BoneType.Eye;
			}
			return BoneType.Unassigned;
		}

		public static BoneSide GetBoneSide(string boneName)
		{
			if (isLeft(boneName))
			{
				return BoneSide.Left;
			}
			if (isRight(boneName))
			{
				return BoneSide.Right;
			}
			return BoneSide.Center;
		}

		public static Transform GetBone(Transform[] transforms, BoneType boneType, BoneSide boneSide = BoneSide.Center, params string[][] namings)
		{
			Transform[] bonesOfTypeAndSide = GetBonesOfTypeAndSide(boneType, boneSide, transforms);
			return GetNamingMatch(bonesOfTypeAndSide, namings);
		}

		private static bool isLeft(string boneName)
		{
			return matchesNaming(boneName, typeLeft) || lastLetter(boneName) == "L" || firstLetter(boneName) == "L";
		}

		private static bool isRight(string boneName)
		{
			return matchesNaming(boneName, typeRight) || lastLetter(boneName) == "R" || firstLetter(boneName) == "R";
		}

		private static bool isSpine(string boneName)
		{
			return matchesNaming(boneName, typeSpine) && !excludesNaming(boneName, typeExcludeSpine);
		}

		private static bool isHead(string boneName)
		{
			return matchesNaming(boneName, typeHead) && !excludesNaming(boneName, typeExcludeHead);
		}

		private static bool isArm(string boneName)
		{
			return matchesNaming(boneName, typeArm) && !excludesNaming(boneName, typeExcludeArm);
		}

		private static bool isLeg(string boneName)
		{
			return matchesNaming(boneName, typeLeg) && !excludesNaming(boneName, typeExcludeLeg);
		}

		private static bool isTail(string boneName)
		{
			return matchesNaming(boneName, typeTail) && !excludesNaming(boneName, typeExcludeTail);
		}

		private static bool isEye(string boneName)
		{
			return matchesNaming(boneName, typeEye) && !excludesNaming(boneName, typeExcludeEye);
		}

		private static bool isTypeExclude(string boneName)
		{
			return matchesNaming(boneName, typeExclude);
		}

		private static bool matchesNaming(string boneName, string[] namingConvention)
		{
			if (excludesNaming(boneName, typeExclude))
			{
				return false;
			}
			foreach (string value in namingConvention)
			{
				if (boneName.Contains(value))
				{
					return true;
				}
			}
			return false;
		}

		private static bool excludesNaming(string boneName, string[] namingConvention)
		{
			foreach (string value in namingConvention)
			{
				if (boneName.Contains(value))
				{
					return true;
				}
			}
			return false;
		}

		private static bool matchesLastLetter(string boneName, string[] namingConvention)
		{
			foreach (string letter in namingConvention)
			{
				if (LastLetterIs(boneName, letter))
				{
					return true;
				}
			}
			return false;
		}

		private static bool LastLetterIs(string boneName, string letter)
		{
			string text = boneName.Substring(boneName.Length - 1, 1);
			return text == letter;
		}

		private static string firstLetter(string boneName)
		{
			if (boneName.Length > 0)
			{
				return boneName.Substring(0, 1);
			}
			return string.Empty;
		}

		private static string lastLetter(string boneName)
		{
			if (boneName.Length > 0)
			{
				return boneName.Substring(boneName.Length - 1, 1);
			}
			return string.Empty;
		}
	}
}
