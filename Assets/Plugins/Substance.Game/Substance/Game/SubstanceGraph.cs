using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Substance.Game
{
	[Serializable]
	public class SubstanceGraph : ScriptableObject
	{
		public struct InputProperties
		{
			public string name;

			public string label;

			public string group;

			public string[] componentLabels;

			public string[] enumOptions;

			public InputPropertiesType type;

			public Vector4 maximum;

			public Vector4 minimum;

			public float step;
		}

		public enum InputPropertiesType
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

		[Serializable]
		public struct InputTextureMapping
		{
			public string mInputName;

			public Texture2D mTexture;
		}

		[Serializable]
		public enum TargetSettingTextureFormat
		{
			Compressed = 0,
			CompressedNoAlpha = 1,
			Raw = 2,
			RawNoAlpha = 3
		}

		[Serializable]
		public class TargetSettingItem
		{
			public string label;

			public bool bOverride;

			public bool bLockRatio;

			public bool bHighQuality;

			public int textureWidth;

			public int textureHeight;

			public TargetSettingTextureFormat textureFormat;

			public int loadBehavior;
		}

		private struct MColorSpaceItem
		{
			[MarshalAs(UnmanagedType.LPStr)]
			public string outputName;

			public int bLinear;
		}

		[Serializable]
		public class ColorSpaceItem
		{
			public string outputName;

			public bool bLinear;
		}

		private struct MTexturePackingItem
		{
			[MarshalAs(UnmanagedType.LPStr)]
			public string outputName;

			public string alphaSource;
		}

		[Serializable]
		public class TexturePackingItem
		{
			public string outputName;

			public string alphaSource;
		}

		[HideInInspector]
		[SerializeField]
		private Substance mParent;

		[HideInInspector]
		[SerializeField]
		private string mPackageURL;

		[HideInInspector]
		[SerializeField]
		private string mPrototypeLabel;

		[HideInInspector]
		[SerializeField]
		private string mDescription;

		[HideInInspector]
		[SerializeField]
		private string mCategory;

		[HideInInspector]
		[SerializeField]
		private string mKeywords;

		[HideInInspector]
		[SerializeField]
		private string mAuthor;

		[HideInInspector]
		[SerializeField]
		private string mAuthorURL;

		[HideInInspector]
		[SerializeField]
		private string mUserTag;

		[SerializeField]
		[HideInInspector]
		internal string mGraphLabel;

		[SerializeField]
		[HideInInspector]
		internal uint mGraphDescIndex;

		[SerializeField]
		[HideInInspector]
		internal uint mGraphInstanceIndex;

		[SerializeField]
		[HideInInspector]
		internal Material mMaterial;

		[SerializeField]
		[HideInInspector]
		private bool mGenerateAllOutputs;

		[SerializeField]
		[HideInInspector]
		private string mShaderName;

		[SerializeField]
		[HideInInspector]
		private string mOutputNames;

		[SerializeField]
		[HideInInspector]
		private bool mGenerateMipMaps;

		[SerializeField]
		[HideInInspector]
		private string mInitialPreset;

		[SerializeField]
		[HideInInspector]
		internal List<TexturePackingItem> mInitialTexturePacking;

		[SerializeField]
		[HideInInspector]
		internal List<TexturePackingItem> mTexturePackingList = new List<TexturePackingItem>();

		[SerializeField]
		[HideInInspector]
		private InputTextureMapping[] mInputTextureMapping;

		[SerializeField]
		[HideInInspector]
		private List<TargetSettingItem> mInitialTargetSetting;

		[SerializeField]
		[HideInInspector]
		public List<TargetSettingItem> mTargetSettingList = new List<TargetSettingItem>();

		[SerializeField]
		[HideInInspector]
		private List<ColorSpaceItem> mInitialColorSpace;

		[SerializeField]
		[HideInInspector]
		private List<ColorSpaceItem> mColorSpaceList = new List<ColorSpaceItem>();

		internal IntPtr mNativeHandle;

		private static float[] mScratchFloatValue = new float[4];

		private static int[] mScratchIntValue = new int[4];

		public Substance substance
		{
			get
			{
				return mParent;
			}
		}

		public IntPtr nativeHandle
		{
			get
			{
				return mNativeHandle;
			}
		}

		public string packageURL
		{
			get
			{
				return mPackageURL;
			}
		}

		public string prototypeLabel
		{
			get
			{
				return mPrototypeLabel;
			}
		}

		public string description
		{
			get
			{
				return mDescription;
			}
		}

		public string category
		{
			get
			{
				return mCategory;
			}
		}

		public string keywords
		{
			get
			{
				return mKeywords;
			}
		}

		public string author
		{
			get
			{
				return mAuthor;
			}
		}

		public string authorURL
		{
			get
			{
				return mAuthorURL;
			}
		}

		public string userTag
		{
			get
			{
				return mUserTag;
			}
		}

		public Material material
		{
			get
			{
				return mMaterial;
			}
		}

		public uint graphInstanceIndex
		{
			get
			{
				return mGraphInstanceIndex;
			}
		}

		public uint graphDescIndex
		{
			get
			{
				return mGraphDescIndex;
			}
		}

		public bool shouldGenerateAllOutputs
		{
			get
			{
				return mGenerateAllOutputs;
			}
			set
			{
				UpdateGenerateAllOutputs(value, false);
			}
		}

		public bool shouldGenerateMipMaps
		{
			get
			{
				return mGenerateMipMaps;
			}
			set
			{
				UpdateGenerateMipMaps(value, false);
			}
		}

		public string preset
		{
			get
			{
				IntPtr intPtr = NativeFunctions.cppGetPreset(mNativeHandle);
				string result = "";
				if (intPtr != IntPtr.Zero)
				{
					result = Marshal.PtrToStringAnsi(intPtr);
					NativeFunctions.cppFreeMemory(intPtr);
				}
				return result;
			}
			set
			{
				NativeFunctions.cppApplyPreset(mNativeHandle, value);
			}
		}

		public string graphLabel
		{
			get
			{
				return mGraphLabel;
			}
			set
			{
				mGraphLabel = value;
			}
		}

		public string shaderName
		{
			get
			{
				return mShaderName;
			}
			set
			{
				mShaderName = value;
			}
		}

		public string outputNames
		{
			get
			{
				return mOutputNames;
			}
			set
			{
				mOutputNames = value;
			}
		}

		public List<TexturePackingItem> tpList
		{
			get
			{
				if (mTexturePackingList.Count == 0)
				{
					mTexturePackingList = GetTexturePackingList(mNativeHandle, shaderName);
					UpdateTexturePacking();
				}
				return mTexturePackingList;
			}
			set
			{
				mTexturePackingList = value;
				UpdateTexturePacking();
			}
		}

		public TargetSettingItem activeTargetSetting
		{
			get
			{
				return tsList[NativeCallbacks.GetTargetSettingIndex()];
			}
		}

		public List<TargetSettingItem> tsList
		{
			get
			{
				if (mTargetSettingList.Count == 0)
				{
					string[] array = new string[3] { "Default", "Standalone", "iPhone" };
					for (int i = 0; i < array.Length; i++)
					{
						TargetSettingItem targetSettingItem = new TargetSettingItem();
						targetSettingItem.label = array[i];
						targetSettingItem.bOverride = false;
						targetSettingItem.bLockRatio = true;
						targetSettingItem.bHighQuality = false;
						targetSettingItem.textureFormat = TargetSettingTextureFormat.Compressed;
						if (i == 0)
						{
							NativeFunctions.cppGetTextureDimensions(mNativeHandle, out targetSettingItem.textureWidth, out targetSettingItem.textureHeight);
						}
						else
						{
							targetSettingItem.textureWidth = mTargetSettingList[0].textureWidth;
							targetSettingItem.textureHeight = mTargetSettingList[0].textureHeight;
						}
						mTargetSettingList.Add(targetSettingItem);
					}
				}
				return mTargetSettingList;
			}
			set
			{
				mTargetSettingList = value;
			}
		}

		public List<ColorSpaceItem> colorSpaceList
		{
			get
			{
				if (mColorSpaceList.Count == 0)
				{
					mColorSpaceList = GetColorSpaceList(mNativeHandle);
				}
				return mColorSpaceList;
			}
			set
			{
				mColorSpaceList = value;
			}
		}

		public float GetInputFloat(string inputName)
		{
			NativeFunctions.cppGetInput_Float(mNativeHandle, inputName, mScratchFloatValue, 1u);
			return mScratchFloatValue[0];
		}

		public int SetInputFloat(string inputName, float value)
		{
			mScratchFloatValue[0] = value;
			return NativeFunctions.cppSetInput_Float(mNativeHandle, inputName, mScratchFloatValue, 1u);
		}

		public void SetInputVector2(string inputName, Vector2 value)
		{
			mScratchFloatValue[0] = value.x;
			mScratchFloatValue[1] = value.y;
			NativeFunctions.cppSetInput_Float(mNativeHandle, inputName, mScratchFloatValue, 2u);
		}

		public Vector2 GetInputVector2(string inputName)
		{
			NativeFunctions.cppGetInput_Float(mNativeHandle, inputName, mScratchFloatValue, 2u);
			return new Vector2(mScratchFloatValue[0], mScratchFloatValue[1]);
		}

		public void SetInputVector3(string inputName, Vector3 value)
		{
			mScratchFloatValue[0] = value.x;
			mScratchFloatValue[1] = value.y;
			mScratchFloatValue[2] = value.z;
			NativeFunctions.cppSetInput_Float(mNativeHandle, inputName, mScratchFloatValue, 3u);
		}

		public Vector3 GetInputVector3(string inputName)
		{
			NativeFunctions.cppGetInput_Float(mNativeHandle, inputName, mScratchFloatValue, 3u);
			return new Vector3(mScratchFloatValue[0], mScratchFloatValue[1], mScratchFloatValue[2]);
		}

		public void SetInputVector4(string inputName, Vector4 value)
		{
			mScratchFloatValue[0] = value.x;
			mScratchFloatValue[1] = value.y;
			mScratchFloatValue[2] = value.z;
			mScratchFloatValue[3] = value.w;
			NativeFunctions.cppSetInput_Float(mNativeHandle, inputName, mScratchFloatValue, 4u);
		}

		public Vector4 GetInputVector4(string inputName)
		{
			NativeFunctions.cppGetInput_Float(mNativeHandle, inputName, mScratchFloatValue, 4u);
			return new Vector4(mScratchFloatValue[0], mScratchFloatValue[1], mScratchFloatValue[2], mScratchFloatValue[3]);
		}

		public void SetInputColor(string inputName, Color value)
		{
			mScratchFloatValue[0] = value.r;
			mScratchFloatValue[1] = value.g;
			mScratchFloatValue[2] = value.b;
			mScratchFloatValue[3] = value.a;
			NativeFunctions.cppSetInput_Float(mNativeHandle, inputName, mScratchFloatValue, 4u);
		}

		public Color GetInputColor(string inputName, int dataType)
		{
			NativeFunctions.cppGetInput_Float(mNativeHandle, inputName, mScratchFloatValue, 4u);
			if (dataType == 2)
			{
				mScratchFloatValue[3] = 1f;
			}
			return new Color(mScratchFloatValue[0], mScratchFloatValue[1], mScratchFloatValue[2], mScratchFloatValue[3]);
		}

		public void SetInputBool(string inputName, bool value)
		{
			mScratchIntValue[0] = (value ? 1 : 0);
			NativeFunctions.cppSetInput_Int(mNativeHandle, inputName, mScratchIntValue, 1u);
		}

		public bool GetInputBool(string inputName)
		{
			NativeFunctions.cppGetInput_Int(mNativeHandle, inputName, mScratchIntValue, 1u);
			return Convert.ToBoolean(mScratchIntValue[0]);
		}

		public void SetInputInt(string inputName, int value)
		{
			mScratchIntValue[0] = value;
			NativeFunctions.cppSetInput_Int(mNativeHandle, inputName, mScratchIntValue, 1u);
		}

		public int GetInputInt(string inputName)
		{
			NativeFunctions.cppGetInput_Int(mNativeHandle, inputName, mScratchIntValue, 1u);
			return mScratchIntValue[0];
		}

		public void SetInputVector2Int(string inputName, int x, int y)
		{
			mScratchIntValue[0] = x;
			mScratchIntValue[1] = y;
			NativeFunctions.cppSetInput_Int(mNativeHandle, inputName, mScratchIntValue, 2u);
		}

		public int[] GetInputVector2Int(string inputName)
		{
			NativeFunctions.cppGetInput_Int(mNativeHandle, inputName, mScratchIntValue, 2u);
			return new int[2]
			{
				mScratchIntValue[0],
				mScratchIntValue[1]
			};
		}

		public void SetInputVector3Int(string inputName, int x, int y, int z)
		{
			mScratchIntValue[0] = x;
			mScratchIntValue[1] = y;
			mScratchIntValue[2] = z;
			NativeFunctions.cppSetInput_Int(mNativeHandle, inputName, mScratchIntValue, 3u);
		}

		public int[] GetInputVector3Int(string inputName)
		{
			NativeFunctions.cppGetInput_Int(mNativeHandle, inputName, mScratchIntValue, 3u);
			return new int[3]
			{
				mScratchIntValue[0],
				mScratchIntValue[1],
				mScratchIntValue[2]
			};
		}

		public void SetInputVector4Int(string inputName, int x, int y, int z, int w)
		{
			mScratchIntValue[0] = x;
			mScratchIntValue[1] = y;
			mScratchIntValue[2] = z;
			mScratchIntValue[3] = w;
			NativeFunctions.cppSetInput_Int(mNativeHandle, inputName, mScratchIntValue, 4u);
		}

		public int[] GetInputVector4Int(string inputName)
		{
			NativeFunctions.cppGetInput_Int(mNativeHandle, inputName, mScratchIntValue, 4u);
			return new int[4]
			{
				mScratchIntValue[0],
				mScratchIntValue[1],
				mScratchIntValue[2],
				mScratchIntValue[3]
			};
		}

		public void SetInputString(string inputName, string value)
		{
			NativeFunctions.cppSetInput_String(mNativeHandle, inputName, value);
		}

		public string GetInputString(string inputName)
		{
			return Marshal.PtrToStringAnsi(NativeFunctions.cppGetInput_String(mNativeHandle, inputName));
		}

		public void SetInputTexture(string inputName, Texture2D value)
		{
			List<InputTextureMapping> list = ((mInputTextureMapping != null) ? new List<InputTextureMapping>(mInputTextureMapping) : new List<InputTextureMapping>());
			foreach (InputTextureMapping item2 in list)
			{
				if (item2.mInputName == inputName)
				{
					list.Remove(item2);
					break;
				}
			}
			if (value != null)
			{
				if (!IsSupportedFormat(value.format))
				{
					Log.Error(string.Concat("Error: input texture format '", value.format, "' is not supported!"));
					return;
				}
				InputTextureMapping item = default(InputTextureMapping);
				item.mInputName = inputName;
				item.mTexture = value;
				list.Add(item);
				int substanceTextureFormat = PixelFormat.GetSubstanceTextureFormat(value.format);
				byte[] array = FlipY(value);
				if (array != null)
				{
					IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf((object)array[0]) * array.Length);
					Marshal.Copy(array, 0, intPtr, array.Length);
					NativeFunctions.cppSetInput_Texture(mNativeHandle, inputName, substanceTextureFormat, value.mipmapCount, value.width, value.height, intPtr);
					Marshal.FreeHGlobal(intPtr);
				}
			}
			else
			{
				NativeFunctions.cppSetInput_Texture(mNativeHandle, inputName, 0, 0, 0, 0, IntPtr.Zero);
			}
			mInputTextureMapping = list.ToArray();
		}

		public Vector2 GetTexturesResolution()
		{
			return GetInputVector2("$outputsize");
		}

		public int SetTexturesResolution(Vector2 size)
		{
			int result = -1;
			if (size.x < 32f || size.x > 4096f || size.y < 32f || size.y > 4096f)
			{
				Debug.LogError(string.Format("Texture dimension error: {0} <= valid value <= {1}", 32, 4096));
				return result;
			}
			if (!NativeTypes.IsPowerOfTwo(size.x) || !NativeTypes.IsPowerOfTwo(size.y))
			{
				Debug.LogError("Texture dimension error: must be a factor of 2");
				return result;
			}
			Debug.Log("preset = " + preset);
			Vector2Int vector2Int = default(Vector2Int);
			vector2Int.x = (int)NativeTypes.InvPow(2.0, size.x);
			vector2Int.y = (int)NativeTypes.InvPow(2.0, size.y);
			SetInputVector2("$outputsize", vector2Int);
			Debug.Log("preset = " + preset);
			NativeFunctions.cppApplyPreset(mNativeHandle, preset);
			return 0;
		}

		public int ListInputs()
		{
			return NativeFunctions.cppListInputs(mNativeHandle);
		}

		public InputProperties[] GetInputProperties()
		{
			int num = NativeFunctions.cppGetNumInputs(mNativeHandle);
			NativeTypes.Input[] nativeInputs = NativeTypes.GetNativeInputs(this, num);
			InputProperties[] array = new InputProperties[num];
			for (int i = 0; i < num; i++)
			{
				InputProperties inputProperties = default(InputProperties);
				inputProperties.name = nativeInputs[i].name;
				inputProperties.label = nativeInputs[i].label;
				inputProperties.group = nativeInputs[i].group;
				inputProperties.componentLabels = new string[nativeInputs[i].sliderLabels.Length];
				for (int j = 0; j < nativeInputs[i].sliderLabels.Length; j++)
				{
					inputProperties.componentLabels[j] = nativeInputs[i].sliderLabels[j];
				}
				if (nativeInputs[i].unityWidgetType == NativeTypes.UnityWidgetType.ComboBox)
				{
					int num2 = Marshal.SizeOf(typeof(NativeTypes.MComboBoxItem));
					int pNumValues;
					IntPtr pointer = NativeFunctions.cppGetMComboBoxItems(nativeHandle, nativeInputs[i].name, out pNumValues);
					inputProperties.enumOptions = new string[pNumValues];
					for (int k = 0; k < pNumValues; k++)
					{
						NativeTypes.MComboBoxItem mComboBoxItem = (NativeTypes.MComboBoxItem)Marshal.PtrToStructure(new IntPtr(pointer.ToInt64() + num2 * k), typeof(NativeTypes.MComboBoxItem));
						inputProperties.enumOptions[k] = mComboBoxItem.label;
					}
					if (pNumValues > 0)
					{
						NativeFunctions.cppFreeMemory(pointer);
					}
				}
				inputProperties.type = (InputPropertiesType)nativeInputs[i].unityInputType;
				inputProperties.maximum = new Vector4(nativeInputs[i].maximum.x, nativeInputs[i].maximum.y, nativeInputs[i].maximum.z, nativeInputs[i].maximum.w);
				inputProperties.minimum = new Vector4(nativeInputs[i].minimum.x, nativeInputs[i].minimum.y, nativeInputs[i].minimum.z, nativeInputs[i].minimum.w);
				inputProperties.step = nativeInputs[i].step;
				array[i] = inputProperties;
			}
			return array;
		}

		public byte[] FlipY(Texture2D pInput)
		{
			try
			{
				pInput.GetPixel(0, 0);
				Texture2D texture2D = new Texture2D(pInput.width, pInput.height, pInput.format, false);
				byte[] rawTextureData = pInput.GetRawTextureData();
				byte[] rawTextureData2 = texture2D.GetRawTextureData();
				int num = rawTextureData2.Length / pInput.height;
				int num2 = 0;
				int num3 = 0;
				int num4 = rawTextureData2.Length - num;
				while (num2 < pInput.height)
				{
					Array.Copy(rawTextureData, num3, rawTextureData2, num4, num);
					num2++;
					num3 += num;
					num4 -= num;
				}
				return rawTextureData2;
			}
			catch (UnityException ex)
			{
				if (ex.Message.StartsWith("Texture '" + pInput.name + "' is not readable"))
				{
					Debug.LogError("Please enable read/write on texture [" + pInput.name + "]");
				}
			}
			return null;
		}

		public bool IsSupportedFormat(TextureFormat pFormat)
		{
			bool result = false;
			switch (pFormat)
			{
			case TextureFormat.Alpha8:
			case TextureFormat.RGB24:
			case TextureFormat.RGBA32:
				result = true;
				break;
			}
			return result;
		}

		public Texture2D GetInputTexture(string inputName)
		{
			if (mInputTextureMapping != null)
			{
				InputTextureMapping[] array = mInputTextureMapping;
				for (int i = 0; i < array.Length; i++)
				{
					InputTextureMapping inputTextureMapping = array[i];
					if (inputTextureMapping.mInputName == inputName)
					{
						return inputTextureMapping.mTexture;
					}
				}
			}
			return null;
		}

		public void AddToOutputNames(string pOutputName)
		{
			if (outputNames != "")
			{
				outputNames += ",";
			}
			outputNames += pOutputName;
		}

		public void RemoveFromOutputNames(string pOutputName)
		{
			string[] array = outputNames.Split(',');
			outputNames = "";
			for (int i = 0; i < array.Length; i++)
			{
				if (!(array[i] == pOutputName))
				{
					AddToOutputNames(array[i]);
				}
			}
		}

		public TexturePackingItem GetTexturePackingItem(string pOutputName)
		{
			foreach (TexturePackingItem tp in tpList)
			{
				if (tp.outputName == pOutputName)
				{
					return tp;
				}
			}
			return null;
		}

		public void UpdateTexturePacking(TexturePackingItem pItem)
		{
			for (int i = 0; i < tpList.Count; i++)
			{
				if (tpList[i].outputName == pItem.outputName)
				{
					tpList[i].alphaSource = pItem.alphaSource;
				}
			}
			NativeFunctions.cppOnAlphaSourceChanged(mNativeHandle, pItem.outputName, pItem.alphaSource);
		}

		public void UpdateTexturePacking()
		{
			foreach (TexturePackingItem tp in tpList)
			{
				if (tsList[0].textureFormat == TargetSettingTextureFormat.Compressed || tsList[0].textureFormat == TargetSettingTextureFormat.Raw)
				{
					NativeFunctions.cppOnAlphaSourceChanged(mNativeHandle, tp.outputName, tp.alphaSource);
				}
				else
				{
					NativeFunctions.cppOnAlphaSourceChanged(mNativeHandle, tp.outputName, tp.outputName);
				}
			}
		}

		public List<TexturePackingItem> GetTexturePackingList(IntPtr pNativeHandle, string pShaderName)
		{
			List<TexturePackingItem> list = new List<TexturePackingItem>();
			int num = NativeFunctions.cppGetNumOutputs(pNativeHandle);
			int num2 = Marshal.SizeOf(typeof(MTexturePackingItem));
			IntPtr pointer = NativeFunctions.cppGetTexturePackingList(pNativeHandle, pShaderName, num);
			for (int i = 0; i < num; i++)
			{
				MTexturePackingItem mTexturePackingItem = (MTexturePackingItem)Marshal.PtrToStructure(new IntPtr(pointer.ToInt64() + num2 * i), typeof(MTexturePackingItem));
				TexturePackingItem texturePackingItem = new TexturePackingItem();
				texturePackingItem.outputName = mTexturePackingItem.outputName;
				texturePackingItem.alphaSource = mTexturePackingItem.alphaSource;
				list.Add(texturePackingItem);
			}
			NativeFunctions.cppFreeMemory(pointer);
			return list;
		}

		public void RefreshCurrentTarget()
		{
			NativeFunctions.cppGetTextureDimensions(mNativeHandle, out tsList[NativeCallbacks.GetTargetSettingIndex()].textureWidth, out tsList[NativeCallbacks.GetTargetSettingIndex()].textureHeight);
		}

		public List<ColorSpaceItem> GetColorSpaceList(IntPtr pNativeHandle)
		{
			List<ColorSpaceItem> list = new List<ColorSpaceItem>();
			int num = NativeFunctions.cppGetNumOutputs(pNativeHandle);
			int num2 = Marshal.SizeOf(typeof(MTexturePackingItem));
			IntPtr pointer = NativeFunctions.cppGetColorSpaceList(pNativeHandle, num);
			for (int i = 0; i < num; i++)
			{
				MColorSpaceItem mColorSpaceItem = (MColorSpaceItem)Marshal.PtrToStructure(new IntPtr(pointer.ToInt64() + num2 * i), typeof(MColorSpaceItem));
				ColorSpaceItem colorSpaceItem = new ColorSpaceItem();
				colorSpaceItem.outputName = mColorSpaceItem.outputName;
				colorSpaceItem.bLinear = mColorSpaceItem.bLinear == 1;
				list.Add(colorSpaceItem);
			}
			NativeFunctions.cppFreeMemory(pointer);
			return list;
		}

		public bool IsOutputLinear(string pOutputLabel)
		{
			bool result = false;
			foreach (ColorSpaceItem colorSpace in colorSpaceList)
			{
				if (pOutputLabel == colorSpace.outputName)
				{
					return colorSpace.bLinear;
				}
			}
			return result;
		}

		public void SetOutputLinear(string pOutputLabel, bool pLinear)
		{
			foreach (ColorSpaceItem colorSpace in colorSpaceList)
			{
				if (pOutputLabel == colorSpace.outputName)
				{
					colorSpace.bLinear = pLinear;
					break;
				}
			}
		}

		public static SubstanceGraph CreateGraphInstance(Substance parent, uint descIndex, uint instanceIndex, string packageURL, string graphLabel, string prototypeLabel, string description, string category, string keywords, string author, string authorURL, string userTag)
		{
			SubstanceGraph substanceGraph = ScriptableObject.CreateInstance<SubstanceGraph>();
			substanceGraph.name = "graph." + graphLabel;
			substanceGraph.mGraphDescIndex = descIndex;
			substanceGraph.mGraphInstanceIndex = instanceIndex;
			substanceGraph.mParent = parent;
			substanceGraph.mPackageURL = packageURL;
			substanceGraph.mGraphLabel = graphLabel;
			substanceGraph.mPrototypeLabel = prototypeLabel;
			substanceGraph.mDescription = description;
			substanceGraph.mCategory = category;
			substanceGraph.mKeywords = keywords;
			substanceGraph.mAuthor = author;
			substanceGraph.mAuthorURL = authorURL;
			substanceGraph.mUserTag = userTag;
			substanceGraph.mGenerateAllOutputs = false;
			substanceGraph.mGenerateMipMaps = true;
			substanceGraph.mShaderName = "Standard";
			substanceGraph.mOutputNames = "";
			parent.mGraphs.Add(substanceGraph);
			return substanceGraph;
		}

		internal static SubstanceGraph DuplicateInstance(SubstanceGraph graph)
		{
			IntPtr intPtr = NativeFunctions.cppDuplicateGraphInstance(graph.mNativeHandle);
			Substance substance = graph.substance;
			string uniqueGraphName = GetUniqueGraphName(substance, graph.prototypeLabel);
			SubstanceGraph substanceGraph = CreateGraphInstance(substance, graph.graphDescIndex, (uint)substance.graphs.Count, graph.packageURL, uniqueGraphName, graph.prototypeLabel, graph.description, graph.category, graph.keywords, graph.author, graph.authorURL, graph.userTag);
			substanceGraph.mNativeHandle = intPtr;
			substanceGraph.preset = graph.preset;
			substanceGraph.mInitialTexturePacking = new List<TexturePackingItem>(graph.mInitialTexturePacking.ToArray());
			substanceGraph.mTexturePackingList = new List<TexturePackingItem>(graph.mTexturePackingList.ToArray());
			if (graph.mInputTextureMapping != null)
			{
				for (int i = 0; i < graph.mInputTextureMapping.Length; i++)
				{
					substanceGraph.SetInputTexture(graph.mInputTextureMapping[i].mInputName, graph.mInputTextureMapping[i].mTexture);
				}
			}
			substanceGraph.mInitialColorSpace = new List<ColorSpaceItem>(graph.mInitialColorSpace.ToArray());
			substanceGraph.mInitialTargetSetting = new List<TargetSettingItem>(graph.mInitialTargetSetting.ToArray());
			substanceGraph.mTargetSettingList = new List<TargetSettingItem>(graph.mTargetSettingList.ToArray());
			substanceGraph.shouldGenerateAllOutputs = graph.shouldGenerateAllOutputs;
			substanceGraph.shouldGenerateMipMaps = graph.shouldGenerateMipMaps;
			substanceGraph.mShaderName = graph.mShaderName;
			substanceGraph.mOutputNames = "";
			return substanceGraph;
		}

		public static string GetUniqueGraphName(Substance substance, string prototypeLabel)
		{
			string text = prototypeLabel;
			bool flag = false;
			int num = 1;
			do
			{
				if (flag)
				{
					text = prototypeLabel + "_" + num++;
					flag = false;
				}
				foreach (SubstanceGraph graph in substance.graphs)
				{
					if (text == graph.graphLabel)
					{
						flag = true;
						break;
					}
				}
			}
			while (flag);
			return text;
		}

		public static SubstanceGraph Find(Material material)
		{
			foreach (KeyValuePair<string, WeakReference> mSubstance in Substance.mSubstances)
			{
				Substance substance = mSubstance.Value.Target as Substance;
				if (substance == null)
				{
					continue;
				}
				foreach (SubstanceGraph graph in substance.graphs)
				{
					if (graph.material == material)
					{
						return graph;
					}
				}
			}
			return null;
		}

		public static SubstanceGraph Find(string materialName)
		{
			foreach (KeyValuePair<string, WeakReference> mSubstance in Substance.mSubstances)
			{
				Substance substance = mSubstance.Value.Target as Substance;
				if (substance == null)
				{
					continue;
				}
				foreach (SubstanceGraph graph in substance.graphs)
				{
					if (graph.material.name == materialName)
					{
						return graph;
					}
				}
			}
			return null;
		}

		public void QueueForRender()
		{
			NativeFunctions.cppQueueSubstance(mNativeHandle);
		}

		private void UpdateGenerateAllOutputs(bool value, bool force)
		{
			if (force || value != mGenerateAllOutputs)
			{
				mGenerateAllOutputs = value;
			}
		}

		private void UpdateGenerateMipMaps(bool value, bool force)
		{
			if (force || value != mGenerateMipMaps)
			{
				mGenerateMipMaps = value;
				NativeFunctions.cppOnGenerateMipMapsChanged(mNativeHandle, value);
			}
		}

		internal void OnSubstanceLoaded()
		{
			shouldGenerateAllOutputs = mGenerateAllOutputs;
			if (!mGenerateMipMaps)
			{
				UpdateGenerateMipMaps(mGenerateMipMaps, true);
			}
			preset = mInitialPreset;
			if (mInitialTexturePacking != null)
			{
				tpList = mInitialTexturePacking;
			}
			if (mInitialTargetSetting != null)
			{
				tsList = mInitialTargetSetting;
			}
			if (mInitialColorSpace != null)
			{
				colorSpaceList = mInitialColorSpace;
			}
			if (mInputTextureMapping != null)
			{
				InputTextureMapping[] array = mInputTextureMapping;
				for (int i = 0; i < array.Length; i++)
				{
					InputTextureMapping inputTextureMapping = array[i];
					SetInputTexture(inputTextureMapping.mInputName, inputTextureMapping.mTexture);
				}
			}
			graphLabel = mGraphLabel;
			shaderName = mShaderName;
			outputNames = mOutputNames;
		}

		public void SetInitialPreset(string presetString)
		{
			mInitialPreset = presetString;
		}

		public void SetInitialTexturePacking(List<TexturePackingItem> pList)
		{
			mInitialTexturePacking = pList;
		}

		public void SetInitialColorSpace(List<ColorSpaceItem> pList)
		{
			mInitialColorSpace = pList;
		}

		public void SetInitialTargetSetting(List<TargetSettingItem> pList)
		{
			mInitialTargetSetting = pList;
		}

		public InputTextureMapping[] GetInputTextureMapping()
		{
			if (mInputTextureMapping != null)
			{
				return mInputTextureMapping.Clone() as InputTextureMapping[];
			}
			return new InputTextureMapping[0];
		}

		public string GetOutputNameFromTexture(string pTextureName)
		{
			string text = mPrototypeLabel + "_";
			return pTextureName.Remove(0, text.Length);
		}
	}
}
