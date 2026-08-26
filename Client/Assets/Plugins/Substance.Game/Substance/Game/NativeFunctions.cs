using System;

namespace Substance.Game
{
	public class NativeFunctions
	{
		public delegate void cppInitSubstanceDelegate(string applicationDataPath);

		public delegate void cppSetCallbacksDelegate(IntPtr log, IntPtr texture, IntPtr graphInitialized);

		public delegate void cppIsValidGraphHandleDelegate(IntPtr pGraphHandle);

		public delegate void cppApplyPresetDelegate(IntPtr graphHandle, string presetStr);

		public delegate IntPtr cppGetPresetDelegate(IntPtr graphHandle);

		public delegate int cppGetNumOutputsDelegate(IntPtr graphHandle);

		public delegate int cppGetNumMainTexturesDelegate(IntPtr graphHandle);

		public delegate IntPtr cppGetTexturePackingListDelegate(IntPtr graphHandle, string pShaderName, int numOutputs);

		public delegate IntPtr cppGetColorSpaceListDelegate(IntPtr graphHandle, int numOutputs);

		public delegate void cppFreeMemoryDelegate(IntPtr pointer);

		public delegate void cppSetCreateSubstanceGraphCallbackEditorPtrDelegate(IntPtr fp);

		public delegate int cppSetLogPathDelegate(string pLogPath);

		public delegate int cppLogDelegate(string pString);

		public delegate int cppLogErrorDelegate(string pString);

		public delegate IntPtr cppGetMInputsDelegate(IntPtr graphHandle, int pNumInputs);

		public delegate int cppShutdownSubstanceDelegate();

		public delegate int cppListAssetsDelegate();

		public delegate int cppRemoveAssetDelegate(string pAssetPath);

		public delegate int cppMoveAssetDelegate(string pFromAssetPath, string pToAssetPath);

		public delegate int cppLoadSubstanceDelegate(string pAssetPath, IntPtr array, int size, IntPtr assetCtx, IntPtr substanceObject, uint[] graphIndices, string[] graphLabels, int[] graphFormats, uint numGraphIndices);

		public delegate void cppQueueSubstanceDelegate(IntPtr graphHandle);

		public delegate void cppRenderSubstancesDelegate(bool bAsync, IntPtr preComputeCallback);

		public delegate void cppSetDirtyOutputsDelegate(IntPtr graphHandle);

		public delegate int cppGetNumInputsDelegate(IntPtr graphHandle);

		public delegate IntPtr cppGetInput_IntsDelegate(IntPtr graphHandle, string pFieldName, int pNumInputs);

		public delegate IntPtr cppGetMComboBoxItemsDelegate(IntPtr graphHandle, string pIdentifier, out int pNumValues);

		public delegate void cppGetInput_FloatDelegate(IntPtr graphHandle, string pInputName, float[] values, uint numValues);

		public delegate int cppSetInput_FloatDelegate(IntPtr graphHandle, string pInputName, float[] values, uint numValues);

		public delegate void cppGetInput_IntDelegate(IntPtr graphHandle, string pInputName, int[] values, uint numValues);

		public delegate void cppSetInput_IntDelegate(IntPtr graphHandle, string pInputName, int[] values, uint numValues);

		public delegate int cppSetInput_StringDelegate(IntPtr graphHandle, string pInputName, string value);

		public delegate IntPtr cppGetInput_StringDelegate(IntPtr graphHandle, string pInputName);

		public delegate int cppSetInput_TextureDelegate(IntPtr graphHandle, string pInputName, int format, int mipCount, int width, int height, IntPtr pixels);

		public delegate void cppProcessOutputQueueDelegate();

		public delegate IntPtr cppGetOutputLabelFromHashDelegate(string pAssetPath, uint outputHash);

		public delegate IntPtr cppGetOutputChannelStrFromHashDelegate(string pAssetPath, uint outputHash);

		public delegate void cppOnGenerateMipMapsChangedDelegate(IntPtr graphHandle, bool bGenerateMipMaps);

		public delegate void cppOnAlphaSourceChangedDelegate(IntPtr graphHandle, string pOutputUsage, string pAlphaSource);

		public delegate void cppGetTextureDimensionsDelegate(IntPtr graphHandle, out int pWidth, out int pHeight);

		public delegate IntPtr cppDuplicateGraphInstanceDelegate(IntPtr graphHandle);

		public delegate void cppRemoveGraphInstanceDelegate(IntPtr graphHandle);

		public delegate int cppListInputsDelegate(IntPtr graphHandle);

		public static void cppInitSubstance(string applicationDataPath)
		{
			DLLHelpers.LoadDLL(applicationDataPath);
			if (!(DLLHelpers.DllHandle == IntPtr.Zero))
			{
				object[] @params = DLLHelpers.GetParams(1);
				@params[0] = applicationDataPath;
				DLLHelpers.GetFunction("cppInitSubstance", typeof(cppInitSubstanceDelegate)).DynamicInvoke(@params);
			}
		}

		public static void cppSetCallbacks(IntPtr log, IntPtr texture, IntPtr graphInitialized)
		{
			if (!(DLLHelpers.DllHandle == IntPtr.Zero))
			{
				object[] @params = DLLHelpers.GetParams(3);
				@params[0] = log;
				@params[1] = texture;
				@params[2] = graphInitialized;
				DLLHelpers.GetFunction("cppSetCallbacks", typeof(cppSetCallbacksDelegate)).DynamicInvoke(@params);
			}
		}

		public static void cppIsValidGraphHandle(IntPtr pGraphHandle)
		{
			if (!(DLLHelpers.DllHandle == IntPtr.Zero))
			{
				object[] @params = DLLHelpers.GetParams(1);
				@params[0] = pGraphHandle;
				DLLHelpers.GetFunction("cppIsValidGraphHandle", typeof(cppIsValidGraphHandleDelegate)).DynamicInvoke(@params);
			}
		}

		public static void cppApplyPreset(IntPtr graphHandle, string presetStr)
		{
			if (!(DLLHelpers.DllHandle == IntPtr.Zero))
			{
				object[] @params = DLLHelpers.GetParams(2);
				@params[0] = graphHandle;
				@params[1] = presetStr;
				DLLHelpers.GetFunction("cppApplyPreset", typeof(cppApplyPresetDelegate)).DynamicInvoke(@params);
			}
		}

		public static IntPtr cppGetPreset(IntPtr graphHandle)
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				return IntPtr.Zero;
			}
			object[] @params = DLLHelpers.GetParams(1);
			@params[0] = graphHandle;
			return (IntPtr)DLLHelpers.GetFunction("cppGetPreset", typeof(cppGetPresetDelegate)).DynamicInvoke(@params);
		}

		public static int cppGetNumOutputs(IntPtr graphHandle)
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				return 0;
			}
			object[] @params = DLLHelpers.GetParams(1);
			@params[0] = graphHandle;
			return (int)DLLHelpers.GetFunction("cppGetNumOutputs", typeof(cppGetNumOutputsDelegate)).DynamicInvoke(@params);
		}

		public static int cppGetNumMainTextures(IntPtr graphHandle)
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				return 0;
			}
			object[] @params = DLLHelpers.GetParams(1);
			@params[0] = graphHandle;
			return (int)DLLHelpers.GetFunction("cppGetNumMainTextures", typeof(cppGetNumMainTexturesDelegate)).DynamicInvoke(@params);
		}

		public static IntPtr cppGetTexturePackingList(IntPtr graphHandle, string pShaderName, int numOutputs)
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				return IntPtr.Zero;
			}
			object[] @params = DLLHelpers.GetParams(3);
			@params[0] = graphHandle;
			@params[1] = pShaderName;
			@params[2] = numOutputs;
			return (IntPtr)DLLHelpers.GetFunction("cppGetTexturePackingList", typeof(cppGetTexturePackingListDelegate)).DynamicInvoke(@params);
		}

		public static IntPtr cppGetColorSpaceList(IntPtr graphHandle, int numOutputs)
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				return IntPtr.Zero;
			}
			object[] @params = DLLHelpers.GetParams(2);
			@params[0] = graphHandle;
			@params[1] = numOutputs;
			return (IntPtr)DLLHelpers.GetFunction("cppGetColorSpaceList", typeof(cppGetColorSpaceListDelegate)).DynamicInvoke(@params);
		}

		public static void cppFreeMemory(IntPtr pointer)
		{
			if (!(DLLHelpers.DllHandle == IntPtr.Zero))
			{
				object[] @params = DLLHelpers.GetParams(1);
				@params[0] = pointer;
				DLLHelpers.GetFunction("cppFreeMemory", typeof(cppFreeMemoryDelegate)).DynamicInvoke(@params);
			}
		}

		public static void cppSetCreateSubstanceGraphCallbackEditorPtr(IntPtr fp)
		{
			if (!(DLLHelpers.DllHandle == IntPtr.Zero))
			{
				object[] @params = DLLHelpers.GetParams(1);
				@params[0] = fp;
				DLLHelpers.GetFunction("cppSetCreateSubstanceGraphCallbackEditorPtr", typeof(cppSetCreateSubstanceGraphCallbackEditorPtrDelegate)).DynamicInvoke(@params);
			}
		}

		public static int cppSetLogPath(string pLogPath)
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				return 0;
			}
			object[] @params = DLLHelpers.GetParams(1);
			@params[0] = pLogPath;
			return (int)DLLHelpers.GetFunction("cppSetLogPath", typeof(cppSetLogPathDelegate)).DynamicInvoke(@params);
		}

		public static int cppLog(string pString)
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				return 0;
			}
			object[] @params = DLLHelpers.GetParams(1);
			@params[0] = pString;
			return (int)DLLHelpers.GetFunction("cppLog", typeof(cppLogDelegate)).DynamicInvoke(@params);
		}

		public static int cppLogError(string pString)
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				return 0;
			}
			object[] @params = DLLHelpers.GetParams(1);
			@params[0] = pString;
			return (int)DLLHelpers.GetFunction("cppLogError", typeof(cppLogErrorDelegate)).DynamicInvoke(@params);
		}

		public static IntPtr cppGetMInputs(IntPtr graphHandle, int pNumInputs)
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				return IntPtr.Zero;
			}
			object[] @params = DLLHelpers.GetParams(2);
			@params[0] = graphHandle;
			@params[1] = pNumInputs;
			return (IntPtr)DLLHelpers.GetFunction("cppGetMInputs", typeof(cppGetMInputsDelegate)).DynamicInvoke(@params);
		}

		public static int cppShutdownSubstance()
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				return 0;
			}
			int result = (int)DLLHelpers.GetFunction("cppShutdownSubstance", typeof(cppShutdownSubstanceDelegate)).DynamicInvoke(null);
			DLLHelpers.UnloadDLL();
			return result;
		}

		public static int cppListAssets()
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				return 0;
			}
			return (int)DLLHelpers.GetFunction("cppListAssets", typeof(cppListAssetsDelegate)).DynamicInvoke(null);
		}

		public static int cppRemoveAsset(string pAssetPath)
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				return 0;
			}
			object[] @params = DLLHelpers.GetParams(1);
			@params[0] = pAssetPath;
			return (int)DLLHelpers.GetFunction("cppRemoveAsset", typeof(cppRemoveAssetDelegate)).DynamicInvoke(@params);
		}

		public static int cppMoveAsset(string pFromAssetPath, string pToAssetPath)
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				return 0;
			}
			object[] @params = DLLHelpers.GetParams(2);
			@params[0] = pFromAssetPath;
			@params[1] = pToAssetPath;
			return (int)DLLHelpers.GetFunction("cppMoveAsset", typeof(cppMoveAssetDelegate)).DynamicInvoke(@params);
		}

		public static int cppLoadSubstance(string pAssetPath, IntPtr array, int size, IntPtr assetCtx, IntPtr substanceObject, uint[] graphIndices, string[] graphLabels, int[] graphFormats, uint numGraphIndices)
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				return 0;
			}
			object[] @params = DLLHelpers.GetParams(9);
			@params[0] = pAssetPath;
			@params[1] = array;
			@params[2] = size;
			@params[3] = assetCtx;
			@params[4] = substanceObject;
			@params[5] = graphIndices;
			@params[6] = graphLabels;
			@params[7] = graphFormats;
			@params[8] = numGraphIndices;
			return (int)DLLHelpers.GetFunction("cppLoadSubstance", typeof(cppLoadSubstanceDelegate)).DynamicInvoke(@params);
		}

		public static void cppQueueSubstance(IntPtr graphHandle)
		{
			if (!(DLLHelpers.DllHandle == IntPtr.Zero))
			{
				object[] @params = DLLHelpers.GetParams(1);
				@params[0] = graphHandle;
				DLLHelpers.GetFunction("cppQueueSubstance", typeof(cppQueueSubstanceDelegate)).DynamicInvoke(@params);
			}
		}

		public static void cppRenderSubstances(bool bAsync, IntPtr preComputeCallback)
		{
			if (!(DLLHelpers.DllHandle == IntPtr.Zero))
			{
				object[] @params = DLLHelpers.GetParams(2);
				@params[0] = bAsync;
				@params[1] = preComputeCallback;
				DLLHelpers.GetFunction("cppRenderSubstances", typeof(cppRenderSubstancesDelegate)).DynamicInvoke(@params);
			}
		}

		public static void cppSetDirtyOutputs(IntPtr graphHandle)
		{
			if (!(DLLHelpers.DllHandle == IntPtr.Zero))
			{
				object[] @params = DLLHelpers.GetParams(1);
				@params[0] = graphHandle;
				DLLHelpers.GetFunction("cppSetDirtyOutputs", typeof(cppSetDirtyOutputsDelegate)).DynamicInvoke(@params);
			}
		}

		public static int cppGetNumInputs(IntPtr graphHandle)
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				return 0;
			}
			object[] @params = DLLHelpers.GetParams(1);
			@params[0] = graphHandle;
			return (int)DLLHelpers.GetFunction("cppGetNumInputs", typeof(cppGetNumInputsDelegate)).DynamicInvoke(@params);
		}

		public static IntPtr cppGetInput_Ints(IntPtr graphHandle, string pFieldName, int pNumInputs)
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				return IntPtr.Zero;
			}
			object[] @params = DLLHelpers.GetParams(3);
			@params[0] = graphHandle;
			@params[1] = pFieldName;
			@params[2] = pNumInputs;
			return (IntPtr)DLLHelpers.GetFunction("cppGetInput_Ints", typeof(cppGetInput_IntsDelegate)).DynamicInvoke(@params);
		}

		public static IntPtr cppGetMComboBoxItems(IntPtr graphHandle, string pIdentifier, out int pNumValues)
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				pNumValues = 0;
				return IntPtr.Zero;
			}
			object[] @params = DLLHelpers.GetParams(3);
			@params[0] = graphHandle;
			@params[1] = pIdentifier;
			@params[2] = null;
			IntPtr result = (IntPtr)DLLHelpers.GetFunction("cppGetMComboBoxItems", typeof(cppGetMComboBoxItemsDelegate)).DynamicInvoke(@params);
			pNumValues = (int)@params[2];
			return result;
		}

		public static void cppGetInput_Float(IntPtr graphHandle, string pInputName, float[] values, uint numValues)
		{
			if (!(DLLHelpers.DllHandle == IntPtr.Zero))
			{
				object[] @params = DLLHelpers.GetParams(4);
				@params[0] = graphHandle;
				@params[1] = pInputName;
				@params[2] = values;
				@params[3] = numValues;
				DLLHelpers.GetFunction("cppGetInput_Float", typeof(cppGetInput_FloatDelegate)).DynamicInvoke(@params);
			}
		}

		public static int cppSetInput_Float(IntPtr graphHandle, string pInputName, float[] values, uint numValues)
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				return -2;
			}
			object[] @params = DLLHelpers.GetParams(4);
			@params[0] = graphHandle;
			@params[1] = pInputName;
			@params[2] = values;
			@params[3] = numValues;
			return (int)DLLHelpers.GetFunction("cppSetInput_Float", typeof(cppSetInput_FloatDelegate)).DynamicInvoke(@params);
		}

		public static void cppGetInput_Int(IntPtr graphHandle, string pInputName, int[] values, uint numValues)
		{
			if (!(DLLHelpers.DllHandle == IntPtr.Zero))
			{
				object[] @params = DLLHelpers.GetParams(4);
				@params[0] = graphHandle;
				@params[1] = pInputName;
				@params[2] = values;
				@params[3] = numValues;
				DLLHelpers.GetFunction("cppGetInput_Int", typeof(cppGetInput_IntDelegate)).DynamicInvoke(@params);
			}
		}

		public static void cppSetInput_Int(IntPtr graphHandle, string pInputName, int[] values, uint numValues)
		{
			if (!(DLLHelpers.DllHandle == IntPtr.Zero))
			{
				object[] @params = DLLHelpers.GetParams(4);
				@params[0] = graphHandle;
				@params[1] = pInputName;
				@params[2] = values;
				@params[3] = numValues;
				DLLHelpers.GetFunction("cppSetInput_Int", typeof(cppSetInput_IntDelegate)).DynamicInvoke(@params);
			}
		}

		public static int cppSetInput_String(IntPtr graphHandle, string pInputName, string value)
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				return -2;
			}
			object[] @params = DLLHelpers.GetParams(3);
			@params[0] = graphHandle;
			@params[1] = pInputName;
			@params[2] = value;
			return (int)DLLHelpers.GetFunction("cppSetInput_String", typeof(cppSetInput_StringDelegate)).DynamicInvoke(@params);
		}

		public static IntPtr cppGetInput_String(IntPtr graphHandle, string pInputName)
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				return IntPtr.Zero;
			}
			object[] @params = DLLHelpers.GetParams(2);
			@params[0] = graphHandle;
			@params[1] = pInputName;
			return (IntPtr)DLLHelpers.GetFunction("cppGetInput_String", typeof(cppGetInput_StringDelegate)).DynamicInvoke(@params);
		}

		public static int cppSetInput_Texture(IntPtr graphHandle, string pInputName, int format, int mipCount, int width, int height, IntPtr pixels)
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				return -2;
			}
			object[] @params = DLLHelpers.GetParams(7);
			@params[0] = graphHandle;
			@params[1] = pInputName;
			@params[2] = format;
			@params[3] = mipCount;
			@params[4] = width;
			@params[5] = height;
			@params[6] = pixels;
			return (int)DLLHelpers.GetFunction("cppSetInput_Texture", typeof(cppSetInput_TextureDelegate)).DynamicInvoke(@params);
		}

		public static void cppProcessOutputQueue()
		{
			if (!(DLLHelpers.DllHandle == IntPtr.Zero))
			{
				DLLHelpers.GetFunction("cppProcessOutputQueue", typeof(cppProcessOutputQueueDelegate)).DynamicInvoke(null);
			}
		}

		public static IntPtr cppGetOutputLabelFromHash(string pAssetPath, uint outputHash)
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				return IntPtr.Zero;
			}
			object[] @params = DLLHelpers.GetParams(2);
			@params[0] = pAssetPath;
			@params[1] = outputHash;
			return (IntPtr)DLLHelpers.GetFunction("cppGetOutputLabelFromHash", typeof(cppGetOutputLabelFromHashDelegate)).DynamicInvoke(@params);
		}

		public static IntPtr cppGetOutputChannelStrFromHash(string pAssetPath, uint outputHash)
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				return IntPtr.Zero;
			}
			object[] @params = DLLHelpers.GetParams(2);
			@params[0] = pAssetPath;
			@params[1] = outputHash;
			return (IntPtr)DLLHelpers.GetFunction("cppGetOutputChannelStrFromHash", typeof(cppGetOutputChannelStrFromHashDelegate)).DynamicInvoke(@params);
		}

		public static void cppOnGenerateMipMapsChanged(IntPtr graphHandle, bool bGenerateMipMaps)
		{
			if (!(DLLHelpers.DllHandle == IntPtr.Zero))
			{
				object[] @params = DLLHelpers.GetParams(2);
				@params[0] = graphHandle;
				@params[1] = bGenerateMipMaps;
				DLLHelpers.GetFunction("cppOnGenerateMipMapsChanged", typeof(cppOnGenerateMipMapsChangedDelegate)).DynamicInvoke(@params);
			}
		}

		public static void cppOnAlphaSourceChanged(IntPtr graphHandle, string pOutputUsage, string pAlphaSource)
		{
			if (!(DLLHelpers.DllHandle == IntPtr.Zero))
			{
				object[] @params = DLLHelpers.GetParams(3);
				@params[0] = graphHandle;
				@params[1] = pOutputUsage;
				@params[2] = pAlphaSource;
				DLLHelpers.GetFunction("cppOnAlphaSourceChanged", typeof(cppOnAlphaSourceChangedDelegate)).DynamicInvoke(@params);
			}
		}

		public static void cppGetTextureDimensions(IntPtr graphHandle, out int pWidth, out int pHeight)
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				pWidth = 0;
				pHeight = 0;
				return;
			}
			object[] @params = DLLHelpers.GetParams(3);
			@params[0] = graphHandle;
			@params[1] = null;
			@params[2] = null;
			DLLHelpers.GetFunction("cppGetTextureDimensions", typeof(cppGetTextureDimensionsDelegate)).DynamicInvoke(@params);
			pWidth = (int)@params[1];
			pHeight = (int)@params[2];
		}

		public static IntPtr cppDuplicateGraphInstance(IntPtr graphHandle)
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				return IntPtr.Zero;
			}
			object[] @params = DLLHelpers.GetParams(1);
			@params[0] = graphHandle;
			return (IntPtr)DLLHelpers.GetFunction("cppDuplicateGraphInstance", typeof(cppDuplicateGraphInstanceDelegate)).DynamicInvoke(@params);
		}

		public static void cppRemoveGraphInstance(IntPtr graphHandle)
		{
			if (!(DLLHelpers.DllHandle == IntPtr.Zero))
			{
				object[] @params = DLLHelpers.GetParams(1);
				@params[0] = graphHandle;
				DLLHelpers.GetFunction("cppRemoveGraphInstance", typeof(cppRemoveGraphInstanceDelegate)).DynamicInvoke(@params);
			}
		}

		public static int cppListInputs(IntPtr graphHandle)
		{
			if (DLLHelpers.DllHandle == IntPtr.Zero)
			{
				return -2;
			}
			object[] @params = DLLHelpers.GetParams(1);
			@params[0] = graphHandle;
			return (int)DLLHelpers.GetFunction("cppListInputs", typeof(cppListInputsDelegate)).DynamicInvoke(@params);
		}
	}
}
