using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Substance.Game
{
	public static class NativeTypes
	{
		public enum UnityInputType
		{
			Invalid = -1,
			Boolean = 0,
			Float = 1,
			Vector2 = 2,
			Vector3 = 3,
			Vector4 = 4,
			Color = 5,
			Enum = 6,
			Texture = 7,
			String = 8
		}

		public enum UnityWidgetType
		{
			Invalid = -1,
			Label = 0,
			Button = 1,
			Toggle = 2,
			Slider = 3,
			Vector2Field = 4,
			Vector3Field = 5,
			Vector4Field = 6,
			ColorField = 7,
			TextField = 8,
			ComboBox = 9,
			Image = 10
		}

		public struct SubstanceTexture
		{
			public uint level0Width;

			public uint level0Height;

			public byte pixelFormat;

			public byte mipmapCount;

			public byte[] buffer;
		}

		public struct MInput
		{
			[MarshalAs(UnmanagedType.LPStr)]
			public string name;

			[MarshalAs(UnmanagedType.LPStr)]
			public string label;

			[MarshalAs(UnmanagedType.LPStr)]
			public string sliderLabelsArray;

			[MarshalAs(UnmanagedType.LPStr)]
			public string group;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
			public float[] minimum;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
			public float[] maximum;

			public float step;

			public int substanceInputType;

			public int substanceWidgetType;

			public int stringArraySeparator;

			public int isNumerical;
		}

		public struct MComboBoxItem
		{
			[MarshalAs(UnmanagedType.LPStr)]
			public string label;

			public int value;
		}

		public class SerializeMe : ScriptableObject
		{
			public Texture2D texture2D;

			public void OnEnable()
			{
			}
		}

		public class Value
		{
			[Serializable]
			[CompilerGenerated]
			private sealed class _003C_003Ec
			{
				public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

				public static Converter<float, float> _003C_003E9__3_0;

				internal float _003C_002Ector_003Eb__3_0(float v)
				{
					return 0f;
				}
			}

			public float[] scalar = Array.ConvertAll(new float[4], _003C_003Ec._003C_003E9__3_0 ?? (_003C_003Ec._003C_003E9__3_0 = _003C_003Ec._003C_003E9._003C_002Ector_003Eb__3_0));

			public SerializeMe texture;

			public string stringvalue;
		}

		public class Input
		{
			public string name;

			public string label;

			public string[] sliderLabels;

			public string group;

			public Vector4 minimum;

			public Vector4 maximum;

			public float step;

			public int substanceInputType;

			public int substanceWidgetType;

			public UnityInputType unityInputType;

			public UnityWidgetType unityWidgetType;

			public string[] comboBoxLabels;

			public int[] comboBoxValues;

			public bool bWidgetVisible;
		}

		public const int TEXTURE_SIZE_MIN = 32;

		public const int TEXTURE_SIZE_MAX = 4096;

		public const int Substance_IType_Float = 0;

		public const int Substance_IType_Float2 = 1;

		public const int Substance_IType_Float3 = 2;

		public const int Substance_IType_Float4 = 3;

		public const int Substance_IType_Integer = 4;

		public const int Substance_IType_Integer2 = 8;

		public const int Substance_IType_Integer3 = 9;

		public const int Substance_IType_Integer4 = 10;

		public const int Substance_IType_Image = 5;

		public const int Substance_IType_String = 6;

		public const int Substance_IType_Font = 7;

		public const int Substance_WType_Unknown = 0;

		public const int Substance_WType_Slider = 1;

		public const int Substance_WType_Angle = 2;

		public const int Substance_WType_Color = 3;

		public const int Substance_WType_ToggleButton = 4;

		public const int Substance_WType_ComboBox = 5;

		public const int Substance_WType_Image = 6;

		public const string defaultShaderName = "Standard";

		public static double Pow(double pBase, double pExposent)
		{
			return Math.Pow(pBase, pExposent);
		}

		public static double InvPow(double pBase, double pResult)
		{
			return Math.Log(pResult) / Math.Log(pBase);
		}

		public static bool IsPowerOfTwo(double number)
		{
			double a = Math.Log(number, 2.0);
			return Math.Pow(2.0, Math.Round(a)) == number;
		}

		public static UnityInputType GetUnityDataType(int pSubstanceDataType, int pSubstanceWidgetType)
		{
			UnityInputType unityInputType = UnityInputType.Invalid;
			switch (pSubstanceDataType)
			{
			case 0:
				if (pSubstanceWidgetType == 4)
				{
					return UnityInputType.Boolean;
				}
				return UnityInputType.Float;
			case 1:
				return UnityInputType.Vector2;
			case 2:
				if (pSubstanceWidgetType == 3)
				{
					return UnityInputType.Color;
				}
				return UnityInputType.Vector3;
			case 3:
				if (pSubstanceWidgetType == 3)
				{
					return UnityInputType.Color;
				}
				return UnityInputType.Vector4;
			case 4:
				switch (pSubstanceWidgetType)
				{
				case 4:
					return UnityInputType.Boolean;
				case 5:
					return UnityInputType.Enum;
				default:
					return UnityInputType.Float;
				}
			case 8:
				return UnityInputType.Vector2;
			case 9:
				if (pSubstanceWidgetType == 3)
				{
					return UnityInputType.Color;
				}
				return UnityInputType.Vector3;
			case 10:
				if (pSubstanceWidgetType == 3)
				{
					return UnityInputType.Color;
				}
				return UnityInputType.Vector4;
			case 5:
				return UnityInputType.Texture;
			case 6:
				return UnityInputType.String;
			default:
				return UnityInputType.Invalid;
			}
		}

		public static UnityWidgetType GetUnityWidgetType(UnityInputType pUnityInputType)
		{
			UnityWidgetType unityWidgetType = UnityWidgetType.Invalid;
			switch (pUnityInputType)
			{
			case UnityInputType.Boolean:
				return UnityWidgetType.Toggle;
			case UnityInputType.Float:
				return UnityWidgetType.Slider;
			case UnityInputType.Vector2:
				return UnityWidgetType.Vector2Field;
			case UnityInputType.Vector3:
				return UnityWidgetType.Vector3Field;
			case UnityInputType.Vector4:
				return UnityWidgetType.Vector4Field;
			case UnityInputType.Color:
				return UnityWidgetType.ColorField;
			case UnityInputType.String:
				return UnityWidgetType.TextField;
			case UnityInputType.Enum:
				return UnityWidgetType.ComboBox;
			case UnityInputType.Texture:
				return UnityWidgetType.Image;
			default:
				return UnityWidgetType.Invalid;
			}
		}

		public static string GetShaderTextureName(string pShaderName, string pOutputName)
		{
			switch (pOutputName.ToLower())
			{
			case "diffuse":
				return "_MainTex";
			case "basecolor":
			case "base color":
				return "_MainTex";
			case "normal":
				return "_BumpMap";
			case "height":
				return "_ParallaxMap";
			case "emissive":
				return "_EmissionMap";
			case "specular":
				return "_SpecGlossMap";
			case "opacity":
				return "_OpacityMap";
			case "glossiness":
				return "_GlossinessMap";
			case "ambientocclusion":
			case "ambient occlusion":
				return "_OcclusionMap";
			case "detailMask":
				return "_DetailMask";
			case "metallic":
				return "_MetallicGlossMap";
			case "roughness":
				return "_RoughnessMap";
			default:
				return "_Unknown_" + pOutputName;
			}
		}

		public static string GetString(IntPtr pPtr)
		{
			return Marshal.PtrToStringAnsi(pPtr);
		}

		public static int[] GetIntArray(IntPtr pPtr, int pNumInputs)
		{
			int[] array = new int[pNumInputs];
			Marshal.Copy(pPtr, array, 0, pNumInputs);
			return array;
		}

		public static Input[] GetNativeInputs(SubstanceGraph graph, int pNumInputs)
		{
			Input[] array = new Input[pNumInputs];
			for (int i = 0; i < pNumInputs; i++)
			{
				array[i] = new Input();
			}
			int num = Marshal.SizeOf(typeof(MInput));
			IntPtr pointer = NativeFunctions.cppGetMInputs(graph.nativeHandle, pNumInputs);
			for (int j = 0; j < pNumInputs; j++)
			{
				MInput mInput = (MInput)Marshal.PtrToStructure(new IntPtr(pointer.ToInt64() + num * j), typeof(MInput));
				array[j].name = mInput.name;
				array[j].label = mInput.label;
				array[j].group = mInput.group;
				array[j].substanceInputType = mInput.substanceInputType;
				array[j].substanceWidgetType = mInput.substanceWidgetType;
				char c = (char)mInput.stringArraySeparator;
				array[j].sliderLabels = mInput.sliderLabelsArray.Split(c);
				for (int k = 0; k < 4; k++)
				{
					array[j].minimum[k] = mInput.minimum[k];
					array[j].maximum[k] = mInput.maximum[k];
				}
				array[j].step = mInput.step;
				array[j].unityInputType = GetUnityDataType(array[j].substanceInputType, array[j].substanceWidgetType);
				array[j].unityWidgetType = GetUnityWidgetType(array[j].unityInputType);
			}
			NativeFunctions.cppFreeMemory(pointer);
			return array;
		}
	}
}
