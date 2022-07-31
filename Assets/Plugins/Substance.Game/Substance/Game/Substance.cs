using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Substance.Game
{
	[Serializable]
	public class Substance : ScriptableObject
	{
		[Serializable]
		private struct TextureOutputHash
		{
			public uint OutputHash;

			public Texture2D Texture;
		}

		[SerializeField]
		[HideInInspector]
		private byte[] mArchiveData;

		[SerializeField]
		[HideInInspector]
		private string mAssetPath;

		[SerializeField]
		[HideInInspector]
		private List<TextureOutputHash> mTextureOutputHash = new List<TextureOutputHash>();

		[SerializeField]
		[HideInInspector]
		internal List<SubstanceGraph> mGraphs = new List<SubstanceGraph>();

		internal static Dictionary<string, WeakReference> mSubstances = new Dictionary<string, WeakReference>();

		[HideInInspector]
		public byte[] archiveData
		{
			get
			{
				return mArchiveData;
			}
		}

		[HideInInspector]
		public string assetPath
		{
			get
			{
				return mAssetPath;
			}
		}

		[HideInInspector]
		public List<SubstanceGraph> graphs
		{
			get
			{
				return mGraphs;
			}
		}

		private void OnEnable()
		{
			if (mSubstances.Count == 0)
			{
				NativeCallbacks.InitSubstance();
			}
			if (mArchiveData == null || !(Find(assetPath) == null))
			{
				return;
			}
			uint[] array = new uint[mGraphs.Count];
			string[] array2 = new string[mGraphs.Count];
			int[] array3 = new int[mGraphs.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = mGraphs[i].graphDescIndex;
				array2[i] = mGraphs[i].graphLabel;
				if (mGraphs[i].mTargetSettingList.Count == 0)
				{
					array3[i] = 0;
				}
				else
				{
					array3[i] = (int)mGraphs[i].mTargetSettingList[0].textureFormat;
				}
			}
			LoadSubstanceInternal(assetPath, archiveData, null, array, array2, array3);
			if (!Application.isEditor)
			{
				mArchiveData = null;
			}
		}

		private void OnDisable()
		{
			if (Find(assetPath) == this)
			{
				mSubstances.Remove(assetPath);
				NativeFunctions.cppRemoveAsset(assetPath);
			}
			if (mSubstances.Count == 0 && !Application.isEditor)
			{
				NativeCallbacks.ShutdownSubstance();
			}
		}

		public void Awake()
		{
			Runtime.TryLoadRuntime();
		}

		public void SetAssetPath(string pAssetPath)
		{
			mAssetPath = pAssetPath;
		}

		private bool LoadSubstanceInternal(string assetPath, byte[] sbsar, object assetCtx, uint[] graphIndices, string[] graphLabels, int[] graphFormats)
		{
			int num = Marshal.SizeOf((object)sbsar[0]) * sbsar.Length;
			IntPtr intPtr = Marshal.AllocHGlobal(num);
			Marshal.Copy(sbsar, 0, intPtr, sbsar.Length);
			GCHandle value = GCHandle.Alloc(this);
			GCHandle value2 = GCHandle.Alloc(assetCtx);
			NativeFunctions.cppLoadSubstance(assetPath, intPtr, num, GCHandle.ToIntPtr(value2), GCHandle.ToIntPtr(value), graphIndices, graphLabels, graphFormats, (uint)graphIndices.Length);
			value.Free();
			value2.Free();
			Marshal.FreeHGlobal(intPtr);
			foreach (SubstanceGraph mGraph in mGraphs)
			{
				mGraph.OnSubstanceLoaded();
			}
			mSubstances.Remove(assetPath);
			mSubstances.Add(assetPath, new WeakReference(this));
			return true;
		}

		public static Substance CreateInstance(string assetPath, byte[] sbsar, object assetCtx, uint[] graphIndices, string[] graphLabels, int[] graphFormats)
		{
			Substance substance = ScriptableObject.CreateInstance<Substance>();
			substance.mArchiveData = sbsar;
			substance.mAssetPath = assetPath;
			substance.LoadSubstanceInternal(assetPath, sbsar, assetCtx, graphIndices, graphLabels, graphFormats);
			return substance;
		}

		public static Substance Find(string assetPath)
		{
			WeakReference value;
			if (mSubstances.TryGetValue(assetPath, out value))
			{
				return value.Target as Substance;
			}
			return null;
		}

		public void AssignTextureToOutputHash(Texture2D texture, uint outputHash)
		{
			foreach (TextureOutputHash item2 in mTextureOutputHash)
			{
				if (item2.OutputHash == outputHash)
				{
					mTextureOutputHash.Remove(item2);
					break;
				}
			}
			TextureOutputHash item = default(TextureOutputHash);
			item.Texture = texture;
			item.OutputHash = outputHash;
			mTextureOutputHash.Add(item);
		}

		public void AssignMaterialToGraph(int graphIndex, Material material)
		{
			graphs[graphIndex].mMaterial = material;
		}

		public void DuplicateGraph(SubstanceGraph graph)
		{
			if (!(graph == null))
			{
				SubstanceGraph.DuplicateInstance(graph);
			}
		}

		public bool RemoveGraph(SubstanceGraph graph)
		{
			if (graph == null)
			{
				return false;
			}
			if (mGraphs.Count == 1)
			{
				return false;
			}
			NativeFunctions.cppRemoveGraphInstance(graph.mNativeHandle);
			int num = mGraphs.IndexOf(graph);
			mGraphs.Remove(graph);
			for (int i = num; i < mGraphs.Count; i++)
			{
				mGraphs[i].mGraphInstanceIndex--;
			}
			return true;
		}

		public static int GetGraphIndexFromOutputHash(uint outputHash)
		{
			return (int)(outputHash >> 16);
		}

		public string GetOutputLabelFromOutputHash(uint outputHash)
		{
			return Marshal.PtrToStringAnsi(NativeFunctions.cppGetOutputLabelFromHash(assetPath, outputHash));
		}

		internal Texture2D GetOutputTexture(uint outputHash)
		{
			foreach (TextureOutputHash item in mTextureOutputHash)
			{
				if (item.OutputHash == outputHash)
				{
					return item.Texture;
				}
			}
			return null;
		}

		public static void RenderSubstancesSync()
		{
			NativeFunctions.cppRenderSubstances(false, IntPtr.Zero);
		}

		public static void RenderSubstancesAsync()
		{
			NativeFunctions.cppRenderSubstances(true, IntPtr.Zero);
		}

		public static void ClearSubstanceList()
		{
			if (mSubstances != null)
			{
				mSubstances.Clear();
			}
		}

		public static List<string> GetChangedSubstancePaths()
		{
			List<string> list = new List<string>();
			foreach (WeakReference item in new List<WeakReference>(mSubstances.Values))
			{
				Substance substance = item.Target as Substance;
				list.Add(substance.assetPath);
			}
			return list;
		}
	}
}
