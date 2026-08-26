using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace DigitalOpus.MB.Core
{
	[Serializable]
	public class MB3_TextureCombiner
	{
		public class MeshBakerMaterialTexture
		{
			public Texture2D t;

			public float texelDensity;

			public DRect encapsulatingSamplingRect;

			public DRect matTilingRect;

			public MeshBakerMaterialTexture()
			{
			}

			public MeshBakerMaterialTexture(Texture2D tx)
			{
				t = tx;
			}

			public MeshBakerMaterialTexture(Texture2D tx, Vector2 o, Vector2 s, float texelDens)
			{
				t = tx;
				matTilingRect = new DRect(o, s);
				texelDensity = texelDens;
			}
		}

		public class MatAndTransformToMerged
		{
			public Material mat;

			public DRect obUVRectIfTilingSame = new DRect(0f, 0f, 1f, 1f);

			public DRect samplingRectMatAndUVTiling = default(DRect);

			public DRect materialTiling = default(DRect);

			public string objName;

			public MatAndTransformToMerged(Material m)
			{
				mat = m;
			}

			public override bool Equals(object obj)
			{
				if (obj is MatAndTransformToMerged)
				{
					MatAndTransformToMerged matAndTransformToMerged = (MatAndTransformToMerged)obj;
					if (matAndTransformToMerged.mat == mat &&
					    matAndTransformToMerged.obUVRectIfTilingSame == obUVRectIfTilingSame)
					{
						return true;
					}
				}

				return false;
			}

			public override int GetHashCode()
			{
				return mat.GetHashCode() ^ obUVRectIfTilingSame.GetHashCode() ^
				       samplingRectMatAndUVTiling.GetHashCode();
			}
		}

		public class SamplingRectEnclosesComparer : IComparer<MatAndTransformToMerged>
		{
			public int Compare(MatAndTransformToMerged x, MatAndTransformToMerged y)
			{
				if (x.samplingRectMatAndUVTiling.Equals(y.samplingRectMatAndUVTiling))
				{
					return 0;
				}

				if (x.samplingRectMatAndUVTiling.Encloses(y.samplingRectMatAndUVTiling))
				{
					return -1;
				}

				return 1;
			}
		}

		public class MB_TexSet
		{
			public MeshBakerMaterialTexture[] ts;

			public List<MatAndTransformToMerged> mats;

			public List<GameObject> gos;

			public bool allTexturesUseSameMatTiling;

			public Vector2 obUVoffset = new Vector2(0f, 0f);

			public Vector2 obUVscale = new Vector2(1f, 1f);

			public int idealWidth;

			public int idealHeight;

			public DRect obUVrect => new DRect(obUVoffset, obUVscale);

			public MB_TexSet(MeshBakerMaterialTexture[] tss, Vector2 uvOffset, Vector2 uvScale)
			{
				ts = tss;
				obUVoffset = uvOffset;
				obUVscale = uvScale;
				allTexturesUseSameMatTiling = false;
				mats = new List<MatAndTransformToMerged>();
				gos = new List<GameObject>();
			}

			public bool IsEqual(object obj, bool fixOutOfBoundsUVs, bool considerNonTextureProperties,
				TextureBlender resultMaterialTextureBlender)
			{
				if (!(obj is MB_TexSet))
				{
					return false;
				}

				MB_TexSet mB_TexSet = (MB_TexSet)obj;
				if (mB_TexSet.ts.Length != ts.Length)
				{
					return false;
				}

				for (int i = 0; i < ts.Length; i++)
				{
					if (ts[i].matTilingRect != mB_TexSet.ts[i].matTilingRect)
					{
						return false;
					}

					if (ts[i].t != mB_TexSet.ts[i].t)
					{
						return false;
					}

					if (considerNonTextureProperties && resultMaterialTextureBlender != null &&
					    !resultMaterialTextureBlender.NonTexturePropertiesAreEqual(mats[0].mat, mB_TexSet.mats[0].mat))
					{
						return false;
					}
				}

				if (fixOutOfBoundsUVs && obUVoffset != mB_TexSet.obUVoffset)
				{
					return false;
				}

				if (fixOutOfBoundsUVs && obUVscale != mB_TexSet.obUVscale)
				{
					return false;
				}

				return true;
			}

			public void CalcInitialFullSamplingRects(bool fixOutOfBoundsUVs)
			{
				DRect encapsulatingSamplingRect = new DRect(0f, 0f, 1f, 1f);
				for (int i = 0; i < ts.Length; i++)
				{
					if (ts[i].t != null)
					{
						DRect r = ts[i].matTilingRect;
						DRect r2 = ((!fixOutOfBoundsUVs) ? new DRect(0.0, 0.0, 1.0, 1.0) : obUVrect);
						ts[i].encapsulatingSamplingRect = MB3_UVTransformUtility.CombineTransforms(ref r2, ref r);
						encapsulatingSamplingRect = ts[i].encapsulatingSamplingRect;
					}
				}

				for (int j = 0; j < ts.Length; j++)
				{
					if (ts[j].t == null)
					{
						ts[j].encapsulatingSamplingRect = encapsulatingSamplingRect;
					}
				}
			}

			public void CalcMatAndUVSamplingRectsIfAllMatTilingSame()
			{
				if (!allTexturesUseSameMatTiling)
				{
					UnityEngine.Debug.LogError(
						"All textures must use same material tiling to calc full sampling rects");
				}

				DRect r = new DRect(0f, 0f, 1f, 1f);
				for (int i = 0; i < ts.Length; i++)
				{
					if (ts[i].t != null)
					{
						r = ts[i].matTilingRect;
					}
				}

				for (int j = 0; j < mats.Count; j++)
				{
					mats[j].materialTiling = r;
					mats[j].samplingRectMatAndUVTiling =
						MB3_UVTransformUtility.CombineTransforms(ref mats[j].obUVRectIfTilingSame, ref r);
				}
			}

			public bool AllTexturesAreSameForMerge(MB_TexSet other, bool considerNonTextureProperties,
				TextureBlender resultMaterialTextureBlender)
			{
				if (other.ts.Length != ts.Length)
				{
					return false;
				}

				if (!other.allTexturesUseSameMatTiling || !allTexturesUseSameMatTiling)
				{
					return false;
				}

				int num = -1;
				for (int i = 0; i < ts.Length; i++)
				{
					if (ts[i].t != other.ts[i].t)
					{
						return false;
					}

					if (num == -1 && ts[i].t != null)
					{
						num = i;
					}

					if (considerNonTextureProperties && resultMaterialTextureBlender != null &&
					    !resultMaterialTextureBlender.NonTexturePropertiesAreEqual(mats[0].mat, other.mats[0].mat))
					{
						return false;
					}
				}

				if (num != -1)
				{
					for (int j = 0; j < ts.Length; j++)
					{
						if (ts[j].t != other.ts[j].t)
						{
							return false;
						}
					}
				}

				return true;
			}

			internal string GetDescription()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendFormat("[GAME_OBJS=");
				for (int i = 0; i < gos.Count; i++)
				{
					stringBuilder.AppendFormat("{0},", gos[i].name);
				}

				stringBuilder.AppendFormat("MATS=");
				for (int j = 0; j < mats.Count; j++)
				{
					stringBuilder.AppendFormat("{0},", mats[j].mat.name);
				}

				stringBuilder.Append("]");
				return stringBuilder.ToString();
			}

			internal string GetMatSubrectDescriptions()
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < mats.Count; i++)
				{
					stringBuilder.AppendFormat("\n    {0}={1},", mats[i].mat.name, mats[i].samplingRectMatAndUVTiling);
				}

				return stringBuilder.ToString();
			}
		}

		public class CombineTexturesIntoAtlasesCoroutineResult
		{
			public bool success = true;

			public bool isFinished;
		}

		public static bool DO_INTEGRITY_CHECKS = false;

		public MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;

		public static ShaderTextureProperty[] shaderTexPropertyNames = new ShaderTextureProperty[19]
		{
			new ShaderTextureProperty("_MainTex", norm: false),
			new ShaderTextureProperty("_BumpMap", norm: true),
			new ShaderTextureProperty("_Normal", norm: true),
			new ShaderTextureProperty("_BumpSpecMap", norm: false),
			new ShaderTextureProperty("_DecalTex", norm: false),
			new ShaderTextureProperty("_Detail", norm: false),
			new ShaderTextureProperty("_GlossMap", norm: false),
			new ShaderTextureProperty("_Illum", norm: false),
			new ShaderTextureProperty("_LightTextureB0", norm: false),
			new ShaderTextureProperty("_ParallaxMap", norm: false),
			new ShaderTextureProperty("_ShadowOffset", norm: false),
			new ShaderTextureProperty("_TranslucencyMap", norm: false),
			new ShaderTextureProperty("_SpecMap", norm: false),
			new ShaderTextureProperty("_SpecGlossMap", norm: false),
			new ShaderTextureProperty("_TranspMap", norm: false),
			new ShaderTextureProperty("_MetallicGlossMap", norm: false),
			new ShaderTextureProperty("_OcclusionMap", norm: false),
			new ShaderTextureProperty("_EmissionMap", norm: false),
			new ShaderTextureProperty("_DetailMask", norm: false)
		};

		[SerializeField] protected MB2_TextureBakeResults _textureBakeResults;

		[SerializeField] protected int _atlasPadding = 1;

		[SerializeField] protected int _maxAtlasSize = 1;

		[SerializeField] protected bool _resizePowerOfTwoTextures;

		[SerializeField] protected bool _fixOutOfBoundsUVs;

		[SerializeField] protected int _maxTilingBakeSize = 1024;

		[SerializeField] protected bool _saveAtlasesAsAssets;

		[SerializeField] protected MB2_PackingAlgorithmEnum _packingAlgorithm;

		[SerializeField] protected bool _meshBakerTexturePackerForcePowerOfTwo = true;

		[SerializeField]
		protected List<ShaderTextureProperty> _customShaderPropNames = new List<ShaderTextureProperty>();

		[SerializeField] protected bool _normalizeTexelDensity;

		[SerializeField] protected bool _considerNonTextureProperties;

		protected TextureBlender resultMaterialTextureBlender;

		protected TextureBlender[] textureBlenders = new TextureBlender[0];

		protected List<Texture2D> _temporaryTextures = new List<Texture2D>();

		internal static bool _RunCorutineWithoutPauseIsRunning = false;

		private int __step2_CalculateIdealSizesForTexturesInAtlasAndPadding;

		private Rect[] __createAtlasesMBTexturePacker;

		public MB2_TextureBakeResults textureBakeResults
		{
			get { return _textureBakeResults; }
			set { _textureBakeResults = value; }
		}

		public int atlasPadding
		{
			get { return _atlasPadding; }
			set { _atlasPadding = value; }
		}

		public int maxAtlasSize
		{
			get { return _maxAtlasSize; }
			set { _maxAtlasSize = value; }
		}

		public bool resizePowerOfTwoTextures
		{
			get { return _resizePowerOfTwoTextures; }
			set { _resizePowerOfTwoTextures = value; }
		}

		public bool fixOutOfBoundsUVs
		{
			get { return _fixOutOfBoundsUVs; }
			set { _fixOutOfBoundsUVs = value; }
		}

		public int maxTilingBakeSize
		{
			get { return _maxTilingBakeSize; }
			set { _maxTilingBakeSize = value; }
		}

		public bool saveAtlasesAsAssets
		{
			get { return _saveAtlasesAsAssets; }
			set { _saveAtlasesAsAssets = value; }
		}

		public MB2_PackingAlgorithmEnum packingAlgorithm
		{
			get { return _packingAlgorithm; }
			set { _packingAlgorithm = value; }
		}

		public bool meshBakerTexturePackerForcePowerOfTwo
		{
			get { return _meshBakerTexturePackerForcePowerOfTwo; }
			set { _meshBakerTexturePackerForcePowerOfTwo = value; }
		}

		public List<ShaderTextureProperty> customShaderPropNames
		{
			get { return _customShaderPropNames; }
			set { _customShaderPropNames = value; }
		}

		public bool considerNonTextureProperties
		{
			get { return _considerNonTextureProperties; }
			set { _considerNonTextureProperties = value; }
		}

		public static void RunCorutineWithoutPause(IEnumerator cor, int recursionDepth)
		{
			if (recursionDepth == 0)
			{
				_RunCorutineWithoutPauseIsRunning = true;
			}

			if (recursionDepth > 20)
			{
				UnityEngine.Debug.LogError("Recursion Depth Exceeded.");
				return;
			}

			while (cor.MoveNext())
			{
				object current = cor.Current;
				if (!(current is YieldInstruction) && current != null && current is IEnumerator)
				{
					RunCorutineWithoutPause((IEnumerator)cor.Current, recursionDepth + 1);
				}
			}

			_RunCorutineWithoutPauseIsRunning = false;
		}

		public bool CombineTexturesIntoAtlases(ProgressUpdateDelegate progressInfo,
			MB_AtlasesAndRects resultAtlasesAndRects, Material resultMaterial, List<GameObject> objsToMesh,
			List<Material> allowedMaterialsFilter, MB2_EditorMethodsInterface textureEditorMethods = null)
		{
			CombineTexturesIntoAtlasesCoroutineResult combineTexturesIntoAtlasesCoroutineResult =
				new CombineTexturesIntoAtlasesCoroutineResult();
			RunCorutineWithoutPause(
				_CombineTexturesIntoAtlases(progressInfo, combineTexturesIntoAtlasesCoroutineResult,
					resultAtlasesAndRects, resultMaterial, objsToMesh, allowedMaterialsFilter, textureEditorMethods),
				0);
			return combineTexturesIntoAtlasesCoroutineResult.success;
		}

		public IEnumerator CombineTexturesIntoAtlasesCoroutine(ProgressUpdateDelegate progressInfo,
			MB_AtlasesAndRects resultAtlasesAndRects, Material resultMaterial, List<GameObject> objsToMesh,
			List<Material> allowedMaterialsFilter, MB2_EditorMethodsInterface textureEditorMethods = null,
			CombineTexturesIntoAtlasesCoroutineResult coroutineResult = null, float maxTimePerFrame = 0.01f)
		{
			MBVersionConcrete mbv = new MBVersionConcrete();
			if (!_RunCorutineWithoutPauseIsRunning &&
			    (mbv.GetMajorVersion() < 5 || (mbv.GetMajorVersion() == 5 && mbv.GetMinorVersion() < 3)))
			{
				UnityEngine.Debug.LogError(
					"Running the texture combiner as a coroutine only works in Unity 5.3 and higher");
				yield return null;
			}

			coroutineResult.success = true;
			coroutineResult.isFinished = false;
			if (maxTimePerFrame <= 0f)
			{
				UnityEngine.Debug.LogError("maxTimePerFrame must be a value greater than zero");
				coroutineResult.isFinished = true;
			}
			else
			{
				yield return _CombineTexturesIntoAtlases(progressInfo, coroutineResult, resultAtlasesAndRects,
					resultMaterial, objsToMesh, allowedMaterialsFilter, textureEditorMethods);
				coroutineResult.isFinished = true;
			}
		}

		private static bool InterfaceFilter(Type typeObj, object criteriaObj)
		{
			return typeObj.ToString() == criteriaObj.ToString();
		}

		private void _LoadTextureBlenders()
		{
			string filterCriteria = "DigitalOpus.MB.Core.TextureBlender";
			TypeFilter filter = InterfaceFilter;
			List<Type> list = new List<Type>();
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				IEnumerable enumerable = null;
				try
				{
					enumerable = assembly.GetTypes();
				}
				catch (Exception ex)
				{
					ex.Equals(null);
				}

				if (enumerable == null)
				{
					continue;
				}

				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					Type[] array = type.FindInterfaces(filter, filterCriteria);
					if (array.Length > 0)
					{
						list.Add(type);
					}
				}
			}

			List<TextureBlender> list2 = new List<TextureBlender>();
			foreach (Type item2 in list)
			{
				if (!item2.IsAbstract && !item2.IsInterface)
				{
					TextureBlender item = (TextureBlender)Activator.CreateInstance(item2);
					list2.Add(item);
				}
			}

			textureBlenders = list2.ToArray();
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.Log($"Loaded {textureBlenders.Length} TextureBlenders.");
			}
		}

		private bool _CollectPropertyNames(Material resultMaterial, List<ShaderTextureProperty> texPropertyNames)
		{
			for (int i = 0; i < texPropertyNames.Count; i++)
			{
				ShaderTextureProperty shaderTextureProperty =
					_customShaderPropNames.Find((ShaderTextureProperty x) => x.name.Equals(texPropertyNames[i].name));
				if (shaderTextureProperty != null)
				{
					_customShaderPropNames.Remove(shaderTextureProperty);
				}
			}

			if (resultMaterial == null)
			{
				UnityEngine.Debug.LogError(
					"Please assign a result material. The combined mesh will use this material.");
				return false;
			}

			string text = string.Empty;
			for (int j = 0; j < shaderTexPropertyNames.Length; j++)
			{
				if (resultMaterial.HasProperty(shaderTexPropertyNames[j].name))
				{
					text = text + ", " + shaderTexPropertyNames[j].name;
					if (!texPropertyNames.Contains(shaderTexPropertyNames[j]))
					{
						texPropertyNames.Add(shaderTexPropertyNames[j]);
					}

					if (resultMaterial.GetTextureOffset(shaderTexPropertyNames[j].name) != new Vector2(0f, 0f) &&
					    LOG_LEVEL >= MB2_LogLevel.warn)
					{
						UnityEngine.Debug.LogWarning("Result material has non-zero offset. This is may be incorrect.");
					}

					if (resultMaterial.GetTextureScale(shaderTexPropertyNames[j].name) != new Vector2(1f, 1f) &&
					    LOG_LEVEL >= MB2_LogLevel.warn)
					{
						UnityEngine.Debug.LogWarning("Result material should have tiling of 1,1");
					}
				}
			}

			for (int k = 0; k < _customShaderPropNames.Count; k++)
			{
				if (resultMaterial.HasProperty(_customShaderPropNames[k].name))
				{
					text = text + ", " + _customShaderPropNames[k].name;
					texPropertyNames.Add(_customShaderPropNames[k]);
					if (resultMaterial.GetTextureOffset(_customShaderPropNames[k].name) != new Vector2(0f, 0f) &&
					    LOG_LEVEL >= MB2_LogLevel.warn)
					{
						UnityEngine.Debug.LogWarning(
							"Result material has non-zero offset. This is probably incorrect.");
					}

					if (resultMaterial.GetTextureScale(_customShaderPropNames[k].name) != new Vector2(1f, 1f) &&
					    LOG_LEVEL >= MB2_LogLevel.warn)
					{
						UnityEngine.Debug.LogWarning("Result material should probably have tiling of 1,1.");
					}
				}
				else if (LOG_LEVEL >= MB2_LogLevel.warn)
				{
					UnityEngine.Debug.LogWarning("Result material shader does not use property " +
					                             _customShaderPropNames[k].name +
					                             " in the list of custom shader property names");
				}
			}

			return true;
		}

		private IEnumerator _CombineTexturesIntoAtlases(ProgressUpdateDelegate progressInfo,
			CombineTexturesIntoAtlasesCoroutineResult result, MB_AtlasesAndRects resultAtlasesAndRects,
			Material resultMaterial, List<GameObject> objsToMesh, List<Material> allowedMaterialsFilter,
			MB2_EditorMethodsInterface textureEditorMethods)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			try
			{
				_temporaryTextures.Clear();
				textureEditorMethods?.Clear();
				if (objsToMesh == null || objsToMesh.Count == 0)
				{
					UnityEngine.Debug.LogError("No meshes to combine. Please assign some meshes to combine.");
					result.success = false;
					yield break;
				}

				if (_atlasPadding < 0)
				{
					UnityEngine.Debug.LogError("Atlas padding must be zero or greater.");
					result.success = false;
					yield break;
				}

				if (_maxTilingBakeSize < 2 || _maxTilingBakeSize > 4096)
				{
					UnityEngine.Debug.LogError("Invalid value for max tiling bake size.");
					result.success = false;
					yield break;
				}

				progressInfo?.Invoke("Collecting textures for " + objsToMesh.Count + " meshes.", 0.01f);
				List<ShaderTextureProperty> texPropertyNames = new List<ShaderTextureProperty>();
				if (!_CollectPropertyNames(resultMaterial, texPropertyNames))
				{
					result.success = false;
					yield break;
				}

				if (_considerNonTextureProperties)
				{
					_LoadTextureBlenders();
					resultMaterialTextureBlender = FindMatchingTextureBlender(resultMaterial.shader.name);
					if (resultMaterialTextureBlender != null)
					{
						if (LOG_LEVEL >= MB2_LogLevel.debug)
						{
							UnityEngine.Debug.Log(
								"Using _considerNonTextureProperties found a TextureBlender for result material. Using: " +
								resultMaterialTextureBlender);
						}
					}
					else
					{
						if (LOG_LEVEL >= MB2_LogLevel.error)
						{
							UnityEngine.Debug.LogWarning(
								"Using _considerNonTextureProperties could not find a TextureBlender that matches the shader on the result material. Using the Fallback Texture Blender.");
						}

						resultMaterialTextureBlender = new TextureBlenderFallback();
					}
				}

				yield return __CombineTexturesIntoAtlases(progressInfo, result, resultAtlasesAndRects, resultMaterial,
					texPropertyNames, objsToMesh, allowedMaterialsFilter, textureEditorMethods);
			}
			finally
			{
				_destroyTemporaryTextures();
				textureEditorMethods?.SetReadFlags(progressInfo);
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					UnityEngine.Debug.Log("Total time to create atlases " + sw.ElapsedMilliseconds.ToString("f5"));
				}
			}
		}

		private IEnumerator __CombineTexturesIntoAtlases(ProgressUpdateDelegate progressInfo,
			CombineTexturesIntoAtlasesCoroutineResult result, MB_AtlasesAndRects resultAtlasesAndRects,
			Material resultMaterial, List<ShaderTextureProperty> texPropertyNames, List<GameObject> objsToMesh,
			List<Material> allowedMaterialsFilter, MB2_EditorMethodsInterface textureEditorMethods)
		{
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.Log("__CombineTexturesIntoAtlases texture properties in shader:" +
				                      texPropertyNames.Count + " objsToMesh:" + objsToMesh.Count +
				                      " _fixOutOfBoundsUVs:" + _fixOutOfBoundsUVs);
			}

			progressInfo?.Invoke("Collecting textures ", 0.01f);
			List<MB_TexSet> distinctMaterialTextures = new List<MB_TexSet>();
			List<GameObject> usedObjsToMesh = new List<GameObject>();
			yield return __Step1_CollectDistinctMatTexturesAndUsedObjects(result, objsToMesh, allowedMaterialsFilter,
				texPropertyNames, textureEditorMethods, distinctMaterialTextures, usedObjsToMesh);
			if (!result.success)
			{
				yield break;
			}

			if (MB3_MeshCombiner.EVAL_VERSION)
			{
				bool flag = true;
				for (int i = 0; i < distinctMaterialTextures.Count; i++)
				{
					for (int j = 0; j < distinctMaterialTextures[i].mats.Count; j++)
					{
						if (!distinctMaterialTextures[i].mats[j].mat.shader.name.EndsWith("Diffuse") &&
						    !distinctMaterialTextures[i].mats[j].mat.shader.name.EndsWith("Bumped Diffuse"))
						{
							UnityEngine.Debug.LogError(
								"The free version of Mesh Baker only works with Diffuse and Bumped Diffuse Shaders. The full version can be used with any shader. Material " +
								distinctMaterialTextures[i].mats[j].mat.name + " uses shader " +
								distinctMaterialTextures[i].mats[j].mat.shader.name);
							flag = false;
						}
					}
				}

				if (!flag)
				{
					result.success = false;
					yield break;
				}
			}

			bool[] allTexturesAreNullAndSameColor = new bool[texPropertyNames.Count];
			yield return __Step2_CalculateIdealSizesForTexturesInAtlasAndPadding(result, distinctMaterialTextures,
				texPropertyNames, allTexturesAreNullAndSameColor, textureEditorMethods);
			if (result.success)
			{
				int _padding = __step2_CalculateIdealSizesForTexturesInAtlasAndPadding;
				yield return __Step3_BuildAndSaveAtlasesAndStoreResults(result, progressInfo, distinctMaterialTextures,
					texPropertyNames, allTexturesAreNullAndSameColor, _padding, textureEditorMethods,
					resultAtlasesAndRects, resultMaterial);
			}
		}

		private IEnumerator __Step1_CollectDistinctMatTexturesAndUsedObjects(
			CombineTexturesIntoAtlasesCoroutineResult result, List<GameObject> allObjsToMesh,
			List<Material> allowedMaterialsFilter, List<ShaderTextureProperty> texPropertyNames,
			MB2_EditorMethodsInterface textureEditorMethods, List<MB_TexSet> distinctMaterialTextures,
			List<GameObject> usedObjsToMesh)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			bool flag = false;
			Dictionary<int, MB_Utility.MeshAnalysisResult[]> dictionary =
				new Dictionary<int, MB_Utility.MeshAnalysisResult[]>();
			for (int i = 0; i < allObjsToMesh.Count; i++)
			{
				GameObject gameObject = allObjsToMesh[i];
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					UnityEngine.Debug.Log("Collecting textures for object " + gameObject);
				}

				if (gameObject == null)
				{
					UnityEngine.Debug.LogError("The list of objects to mesh contained nulls.");
					result.success = false;
					yield break;
				}

				Mesh mesh = MB_Utility.GetMesh(gameObject);
				if (mesh == null)
				{
					UnityEngine.Debug.LogError("Object " + gameObject.name +
					                           " in the list of objects to mesh has no mesh.");
					result.success = false;
					yield break;
				}

				Material[] gOMaterials = MB_Utility.GetGOMaterials(gameObject);
				if (gOMaterials == null)
				{
					UnityEngine.Debug.LogError(
						"Object " + gameObject.name + " in the list of objects has no materials.");
					result.success = false;
					yield break;
				}

				if (!dictionary.TryGetValue(mesh.GetInstanceID(), out var value))
				{
					value = new MB_Utility.MeshAnalysisResult[mesh.subMeshCount];
					for (int j = 0; j < mesh.subMeshCount; j++)
					{
						MB_Utility.hasOutOfBoundsUVs(mesh, ref value[j], j);
						if (_normalizeTexelDensity)
						{
							value[j].submeshArea = GetSubmeshArea(mesh, j);
						}
					}

					dictionary.Add(mesh.GetInstanceID(), value);
				}

				if (_fixOutOfBoundsUVs && LOG_LEVEL >= MB2_LogLevel.trace)
				{
					UnityEngine.Debug.Log(string.Concat("Mesh Analysis for object ", gameObject, " numSubmesh=",
						value.Length, " HasOBUV=", value[0].hasOutOfBoundsUVs, " UVrectSubmesh0=", value[0].uvRect));
				}

				for (int k = 0; k < gOMaterials.Length; k++)
				{
					Material material = gOMaterials[k];
					if (allowedMaterialsFilter != null && !allowedMaterialsFilter.Contains(material))
					{
						continue;
					}

					flag = flag || value[k].hasOutOfBoundsUVs;
					if (material.name.Contains("(Instance)"))
					{
						UnityEngine.Debug.LogError("The sharedMaterial on object " + gameObject.name +
						                           " has been 'Instanced'. This was probably caused by a script accessing the meshRender.material property in the editor.  The material to UV Rectangle mapping will be incorrect. To fix this recreate the object from its prefab or re-assign its material from the correct asset.");
						result.success = false;
						yield break;
					}

					if (_fixOutOfBoundsUVs && !MB_Utility.AreAllSharedMaterialsDistinct(gOMaterials) &&
					    LOG_LEVEL >= MB2_LogLevel.warn)
					{
						UnityEngine.Debug.LogWarning("Object " + gameObject.name +
						                             " uses the same material on multiple submeshes. This may generate strange resultAtlasesAndRects especially when used with fix out of bounds uvs. Try duplicating the material.");
					}

					MeshBakerMaterialTexture[] array = new MeshBakerMaterialTexture[texPropertyNames.Count];
					for (int l = 0; l < texPropertyNames.Count; l++)
					{
						Texture2D texture2D = null;
						Vector2 s = Vector2.one;
						Vector2 o = Vector2.zero;
						float texelDens = 0f;
						if (material.HasProperty(texPropertyNames[l].name))
						{
							Texture texture = material.GetTexture(texPropertyNames[l].name);
							if (texture != null)
							{
								if (!(texture is Texture2D))
								{
									UnityEngine.Debug.LogError("Object " + gameObject.name +
									                           " in the list of objects to mesh uses a Texture that is not a Texture2D. Cannot build atlases.");
									result.success = false;
									yield break;
								}

								texture2D = (Texture2D)texture;
								TextureFormat format = texture2D.format;
								bool flag2 = false;
								if (!Application.isPlaying && textureEditorMethods != null)
								{
									flag2 = textureEditorMethods.IsNormalMap(texture2D);
								}

								if ((format != TextureFormat.ARGB32 && format != TextureFormat.RGBA32 &&
								     format != TextureFormat.BGRA32 && format != TextureFormat.RGB24 &&
								     format != TextureFormat.Alpha8) || flag2)
								{
									if (Application.isPlaying && _packingAlgorithm !=
									    MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Fast)
									{
										UnityEngine.Debug.LogError(string.Concat("Object ", gameObject.name,
											" in the list of objects to mesh uses Texture ", texture2D.name,
											" uses format ", format,
											" that is not in: ARGB32, RGBA32, BGRA32, RGB24, Alpha8 or DXT. These textures cannot be resized at runtime. Try changing texture format. If format says 'compressed' try changing it to 'truecolor'"));
										result.success = false;
										yield break;
									}

									texture2D = (Texture2D)material.GetTexture(texPropertyNames[l].name);
								}
							}

							if (texture2D != null && _normalizeTexelDensity)
							{
								texelDens = ((value[l].submeshArea != 0f)
									? (texture2D.width * texture2D.height / value[l].submeshArea)
									: 0f);
							}

							s = material.GetTextureScale(texPropertyNames[l].name);
							o = material.GetTextureOffset(texPropertyNames[l].name);
						}

						array[l] = new MeshBakerMaterialTexture(texture2D, o, s, texelDens);
					}

					Vector2 uvScale = new Vector2(value[k].uvRect.width, value[k].uvRect.height);
					Vector2 uvOffset = new Vector2(value[k].uvRect.x, value[k].uvRect.y);
					MB_TexSet setOfTexs = new MB_TexSet(array, uvOffset, uvScale);
					MatAndTransformToMerged item = new MatAndTransformToMerged(material);
					setOfTexs.mats.Add(item);
					MB_TexSet mB_TexSet = distinctMaterialTextures.Find((MB_TexSet x) => x.IsEqual(setOfTexs,
						_fixOutOfBoundsUVs, _considerNonTextureProperties, resultMaterialTextureBlender));
					if (mB_TexSet != null)
					{
						setOfTexs = mB_TexSet;
					}
					else
					{
						distinctMaterialTextures.Add(setOfTexs);
					}

					if (!setOfTexs.mats.Contains(item))
					{
						setOfTexs.mats.Add(item);
					}

					if (!setOfTexs.gos.Contains(gameObject))
					{
						setOfTexs.gos.Add(gameObject);
						if (!usedObjsToMesh.Contains(gameObject))
						{
							usedObjsToMesh.Add(gameObject);
						}
					}
				}
			}

			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.Log(
					$"Step1_CollectDistinctTextures collected {distinctMaterialTextures.Count} sets of textures fixOutOfBoundsUV={_fixOutOfBoundsUVs} considerNonTextureProperties={_considerNonTextureProperties}");
			}

			MergeOverlappingDistinctMaterialTexturesAndCalcMaterialSubrects(distinctMaterialTextures,
				fixOutOfBoundsUVs);
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.Log("Total time Step1_CollectDistinctTextures " +
				                      stopwatch.ElapsedMilliseconds.ToString("f5"));
			}
		}

		private IEnumerator __Step2_CalculateIdealSizesForTexturesInAtlasAndPadding(
			CombineTexturesIntoAtlasesCoroutineResult result, List<MB_TexSet> distinctMaterialTextures,
			List<ShaderTextureProperty> texPropertyNames, bool[] allTexturesAreNullAndSameColor,
			MB2_EditorMethodsInterface textureEditorMethods)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			int num = _atlasPadding;
			if (distinctMaterialTextures.Count == 1 && !_fixOutOfBoundsUVs)
			{
				if (LOG_LEVEL >= MB2_LogLevel.info)
				{
					UnityEngine.Debug.Log(
						"All objects use the same textures in this set of atlases. Original textures will be reused instead of creating atlases.");
				}

				num = 0;
			}
			else
			{
				if (allTexturesAreNullAndSameColor.Length != texPropertyNames.Count)
				{
					UnityEngine.Debug.LogError(
						"allTexturesAreNullAndSameColor array must be the same length of texPropertyNames.");
				}

				for (int i = 0; i < distinctMaterialTextures.Count; i++)
				{
					if (LOG_LEVEL >= MB2_LogLevel.debug)
					{
						UnityEngine.Debug.Log("Calculating ideal sizes for texSet TexSet " + i + " of " +
						                      distinctMaterialTextures.Count);
					}

					MB_TexSet mB_TexSet = distinctMaterialTextures[i];
					mB_TexSet.idealWidth = 1;
					mB_TexSet.idealHeight = 1;
					int num2 = 1;
					int num3 = 1;
					if (mB_TexSet.ts.Length != texPropertyNames.Count)
					{
						UnityEngine.Debug.LogError(
							"length of arrays in each element of distinctMaterialTextures must be texPropertyNames.Count");
					}

					for (int j = 0; j < texPropertyNames.Count; j++)
					{
						MeshBakerMaterialTexture meshBakerMaterialTexture = mB_TexSet.ts[j];
						if (!meshBakerMaterialTexture.matTilingRect.size.Equals(Vector2.one) &&
						    distinctMaterialTextures.Count > 1 && LOG_LEVEL >= MB2_LogLevel.warn)
						{
							UnityEngine.Debug.LogWarning(string.Concat("Texture ", meshBakerMaterialTexture.t,
								"is tiled by ", meshBakerMaterialTexture.matTilingRect.size,
								" tiling will be baked into a texture with maxSize:", _maxTilingBakeSize));
						}

						if (!mB_TexSet.obUVscale.Equals(Vector2.one) && distinctMaterialTextures.Count > 1 &&
						    _fixOutOfBoundsUVs && LOG_LEVEL >= MB2_LogLevel.warn)
						{
							UnityEngine.Debug.LogWarning(string.Concat("Texture ", meshBakerMaterialTexture.t,
								"has out of bounds UVs that effectively tile by ", mB_TexSet.obUVscale,
								" tiling will be baked into a texture with maxSize:", _maxTilingBakeSize));
						}

						if (!(meshBakerMaterialTexture.t != null))
						{
							continue;
						}

						Vector2 adjustedForScaleAndOffset2Dimensions =
							GetAdjustedForScaleAndOffset2Dimensions(meshBakerMaterialTexture, mB_TexSet.obUVoffset,
								mB_TexSet.obUVscale);
						if ((int)(adjustedForScaleAndOffset2Dimensions.x * adjustedForScaleAndOffset2Dimensions.y) >
						    num2 * num3)
						{
							if (LOG_LEVEL >= MB2_LogLevel.trace)
							{
								UnityEngine.Debug.Log(string.Concat("    matTex ", meshBakerMaterialTexture.t, " ",
									adjustedForScaleAndOffset2Dimensions, " has a bigger size than ", num2, " ", num3));
							}

							num2 = (int)adjustedForScaleAndOffset2Dimensions.x;
							num3 = (int)adjustedForScaleAndOffset2Dimensions.y;
						}
					}

					if (_resizePowerOfTwoTextures)
					{
						if (num2 <= num * 5)
						{
							UnityEngine.Debug.LogWarning(string.Format(
								"Some of the textures have widths close to the size of the padding. It is not recommended to use _resizePowerOfTwoTextures with widths this small.",
								mB_TexSet.ToString()));
						}

						if (num3 <= num * 5)
						{
							UnityEngine.Debug.LogWarning(string.Format(
								"Some of the textures have heights close to the size of the padding. It is not recommended to use _resizePowerOfTwoTextures with heights this small.",
								mB_TexSet.ToString()));
						}

						if (IsPowerOfTwo(num2))
						{
							num2 -= num * 2;
						}

						if (IsPowerOfTwo(num3))
						{
							num3 -= num * 2;
						}

						if (num2 < 1)
						{
							num2 = 1;
						}

						if (num3 < 1)
						{
							num3 = 1;
						}
					}

					if (LOG_LEVEL >= MB2_LogLevel.trace)
					{
						UnityEngine.Debug.Log("    Ideal size is " + num2 + " " + num3);
					}

					mB_TexSet.idealWidth = num2;
					mB_TexSet.idealHeight = num3;
				}
			}

			for (int k = 0; k < texPropertyNames.Count; k++)
			{
				bool flag = true;
				bool flag2 = true;
				for (int l = 0; l < distinctMaterialTextures.Count; l++)
				{
					if (distinctMaterialTextures[l].ts[k].t != null)
					{
						flag = false;
						break;
					}

					if (!_considerNonTextureProperties)
					{
						continue;
					}

					for (int m = l + 1; m < distinctMaterialTextures.Count; m++)
					{
						Color colorIfNoTexture =
							resultMaterialTextureBlender.GetColorIfNoTexture(distinctMaterialTextures[l].mats[0].mat,
								texPropertyNames[k]);
						Color colorIfNoTexture2 =
							resultMaterialTextureBlender.GetColorIfNoTexture(distinctMaterialTextures[m].mats[0].mat,
								texPropertyNames[k]);
						if (colorIfNoTexture != colorIfNoTexture2)
						{
							flag2 = false;
							break;
						}
					}
				}

				allTexturesAreNullAndSameColor[k] = flag && flag2;
				if (LOG_LEVEL >= MB2_LogLevel.trace)
				{
					UnityEngine.Debug.Log(
						$"AllTexturesAreNullAndSameColor prop: {texPropertyNames[k].name} val:{allTexturesAreNullAndSameColor[k]}");
				}
			}

			if (distinctMaterialTextures.Count > 1 &&
			    _packingAlgorithm != MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Fast)
			{
				for (int n = 0; n < distinctMaterialTextures.Count; n++)
				{
					for (int num4 = 0; num4 < texPropertyNames.Count; num4++)
					{
						Texture2D t = distinctMaterialTextures[n].ts[num4].t;
						if (t != null)
						{
							textureEditorMethods?.AddTextureFormat(t, texPropertyNames[num4].isNormalMap);
						}
					}
				}
			}

			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.Log("Total time Step2 Calculate Ideal Sizes " +
				                      stopwatch.ElapsedMilliseconds.ToString("f5"));
			}

			__step2_CalculateIdealSizesForTexturesInAtlasAndPadding = num;
			yield break;
		}

		private IEnumerator __Step3_BuildAndSaveAtlasesAndStoreResults(CombineTexturesIntoAtlasesCoroutineResult result,
			ProgressUpdateDelegate progressInfo, List<MB_TexSet> distinctMaterialTextures,
			List<ShaderTextureProperty> texPropertyNames, bool[] allTexturesAreNullAndSameColor, int _padding,
			MB2_EditorMethodsInterface textureEditorMethods, MB_AtlasesAndRects resultAtlasesAndRects,
			Material resultMaterial)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			int numAtlases = texPropertyNames.Count;
			StringBuilder report = new StringBuilder();
			if (numAtlases > 0)
			{
				report = new StringBuilder();
				report.AppendLine("Report");
				for (int i = 0; i < distinctMaterialTextures.Count; i++)
				{
					MB_TexSet mB_TexSet = distinctMaterialTextures[i];
					report.AppendLine("----------");
					report.Append("This set of textures will be resized to:" + mB_TexSet.idealWidth + "x" +
					              mB_TexSet.idealHeight + "\n");
					for (int j = 0; j < mB_TexSet.ts.Length; j++)
					{
						if (mB_TexSet.ts[j].t != null)
						{
							report.Append("   [" + texPropertyNames[j].name + " " + mB_TexSet.ts[j].t.name + " " +
							              mB_TexSet.ts[j].t.width + "x" + mB_TexSet.ts[j].t.height + "]");
							if (mB_TexSet.ts[j].matTilingRect.size != Vector2.one ||
							    mB_TexSet.ts[j].matTilingRect.min != Vector2.zero)
							{
								report.AppendFormat(" material scale {0} offset{1} ",
									mB_TexSet.ts[j].matTilingRect.size.ToString("G4"),
									mB_TexSet.ts[j].matTilingRect.min.ToString("G4"));
							}

							if (mB_TexSet.obUVscale != Vector2.one || mB_TexSet.obUVoffset != Vector2.zero)
							{
								report.AppendFormat(" obUV scale {0} offset{1} ", mB_TexSet.obUVscale.ToString("G4"),
									mB_TexSet.obUVoffset.ToString("G4"));
							}

							report.AppendLine(string.Empty);
						}
						else
						{
							report.Append("   [" + texPropertyNames[j].name + " null ");
							if (allTexturesAreNullAndSameColor[j])
							{
								report.Append("no atlas will be created all textures null]\n");
							}
							else
							{
								report.AppendFormat("a 16x16 texture will be created]\n");
							}
						}
					}

					report.AppendLine(string.Empty);
					report.Append("Materials using:");
					for (int k = 0; k < mB_TexSet.mats.Count; k++)
					{
						report.Append(mB_TexSet.mats[k].mat.name + ", ");
					}

					report.AppendLine(string.Empty);
				}
			}

			progressInfo?.Invoke("Creating txture atlases.", 0.1f);
			GC.Collect();
			Texture2D[] atlases = new Texture2D[numAtlases];
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.Log("time Step 3 Create And Save Atlases part 1 " +
				                      sw.ElapsedMilliseconds.ToString("f5"));
			}

			Rect[] rectsInAtlas;
			if (_packingAlgorithm == MB2_PackingAlgorithmEnum.UnitysPackTextures)
			{
				rectsInAtlas = __CreateAtlasesUnityTexturePacker(progressInfo, numAtlases, distinctMaterialTextures,
					texPropertyNames, allTexturesAreNullAndSameColor, resultMaterial, atlases, textureEditorMethods,
					_padding);
			}
			else if (_packingAlgorithm == MB2_PackingAlgorithmEnum.MeshBakerTexturePacker)
			{
				yield return __CreateAtlasesMBTexturePacker(progressInfo, numAtlases, distinctMaterialTextures,
					texPropertyNames, allTexturesAreNullAndSameColor, resultMaterial, atlases, textureEditorMethods,
					_padding);
				rectsInAtlas = __createAtlasesMBTexturePacker;
			}
			else
			{
				rectsInAtlas = __CreateAtlasesMBTexturePackerFast(progressInfo, numAtlases, distinctMaterialTextures,
					texPropertyNames, allTexturesAreNullAndSameColor, resultMaterial, atlases, textureEditorMethods,
					_padding);
			}

			float t3 = sw.ElapsedMilliseconds;
			AdjustNonTextureProperties(resultMaterial, texPropertyNames, distinctMaterialTextures,
				_considerNonTextureProperties, textureEditorMethods);
			progressInfo?.Invoke("Building Report", 0.7f);
			StringBuilder atlasMessage = new StringBuilder();
			atlasMessage.AppendLine("---- Atlases ------");
			for (int l = 0; l < numAtlases; l++)
			{
				if (atlases[l] != null)
				{
					atlasMessage.AppendLine("Created Atlas For: " + texPropertyNames[l].name + " h=" +
					                        atlases[l].height + " w=" + atlases[l].width);
				}
				else if (allTexturesAreNullAndSameColor[l])
				{
					atlasMessage.AppendLine("Did not create atlas for " + texPropertyNames[l].name +
					                        " because all source textures were null.");
				}
			}

			report.Append(atlasMessage.ToString());
			List<MB_MaterialAndUVRect> mat2rect_map = new List<MB_MaterialAndUVRect>();
			for (int m = 0; m < distinctMaterialTextures.Count; m++)
			{
				List<MatAndTransformToMerged> mats = distinctMaterialTextures[m].mats;
				new Rect(0f, 0f, 1f, 1f);
				Rect rect = distinctMaterialTextures[m].ts[0].encapsulatingSamplingRect.GetRect();
				for (int n = 0; n < mats.Count; n++)
				{
					MB_MaterialAndUVRect item = new MB_MaterialAndUVRect(mats[n].mat, rectsInAtlas[m],
						mats[n].samplingRectMatAndUVTiling.GetRect(), mats[n].materialTiling.GetRect(), rect,
						mats[n].objName);
					if (!mat2rect_map.Contains(item))
					{
						mat2rect_map.Add(item);
					}
				}
			}

			resultAtlasesAndRects.atlases = atlases;
			resultAtlasesAndRects.texPropertyNames = ShaderTextureProperty.GetNames(texPropertyNames);
			resultAtlasesAndRects.mat2rect_map = mat2rect_map;
			progressInfo?.Invoke("Restoring Texture Formats & Read Flags", 0.8f);
			_destroyTemporaryTextures();
			textureEditorMethods?.SetReadFlags(progressInfo);
			if (report != null && LOG_LEVEL >= MB2_LogLevel.info)
			{
				UnityEngine.Debug.Log(report.ToString());
			}

			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.Log("Time Step 3 Create And Save Atlases part 3 " +
				                      (sw.ElapsedMilliseconds - t3).ToString("f5"));
			}

			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.Log("Total time Step 3 Create And Save Atlases " +
				                      sw.ElapsedMilliseconds.ToString("f5"));
			}
		}

		private IEnumerator __CreateAtlasesMBTexturePacker(ProgressUpdateDelegate progressInfo, int numAtlases,
			List<MB_TexSet> distinctMaterialTextures, List<ShaderTextureProperty> texPropertyNames,
			bool[] allTexturesAreNullAndSameColor, Material resultMaterial, Texture2D[] atlases,
			MB2_EditorMethodsInterface textureEditorMethods, int _padding)
		{
			Rect[] uvRects;
			if (distinctMaterialTextures.Count == 1 && !_fixOutOfBoundsUVs)
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					UnityEngine.Debug.Log("Only one image per atlas. Will re-use original texture");
				}

				uvRects = new Rect[1]
				{
					new Rect(0f, 0f, 1f, 1f)
				};
				for (int i = 0; i < numAtlases; i++)
				{
					MeshBakerMaterialTexture meshBakerMaterialTexture = distinctMaterialTextures[0].ts[i];
					atlases[i] = meshBakerMaterialTexture.t;
					resultMaterial.SetTexture(texPropertyNames[i].name, atlases[i]);
					resultMaterial.SetTextureScale(texPropertyNames[i].name,
						meshBakerMaterialTexture.matTilingRect.size);
					resultMaterial.SetTextureOffset(texPropertyNames[i].name,
						meshBakerMaterialTexture.matTilingRect.min);
				}
			}
			else
			{
				List<Vector2> imageSizes = new List<Vector2>();
				for (int j = 0; j < distinctMaterialTextures.Count; j++)
				{
					imageSizes.Add(new Vector2(distinctMaterialTextures[j].idealWidth,
						distinctMaterialTextures[j].idealHeight));
				}

				MB2_TexturePacker tp = new MB2_TexturePacker
				{
					doPowerOfTwoTextures = _meshBakerTexturePackerForcePowerOfTwo
				};
				int atlasSizeX = 1;
				int atlasSizeY = 1;
				int atlasMaxDimension = _maxAtlasSize;
				uvRects = tp.GetRects(imageSizes, atlasMaxDimension, _padding, out atlasSizeX, out atlasSizeY);
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					UnityEngine.Debug.Log("Generated atlas will be " + atlasSizeX + "x" + atlasSizeY +
					                      " (Max atlas size for platform: " + atlasMaxDimension + ")");
				}

				for (int propIdx = 0; propIdx < numAtlases; propIdx++)
				{
					Texture2D atlas2 = null;
					if (allTexturesAreNullAndSameColor[propIdx])
					{
						atlas2 = null;
						if (LOG_LEVEL >= MB2_LogLevel.debug)
						{
							UnityEngine.Debug.Log("=== Not creating atlas for " + texPropertyNames[propIdx].name +
							                      " because textures are null and default value parameters are the same.");
						}
					}
					else
					{
						if (LOG_LEVEL >= MB2_LogLevel.debug)
						{
							UnityEngine.Debug.Log("=== Creating atlas for " + texPropertyNames[propIdx].name);
						}

						GC.Collect();
						progressInfo?.Invoke("Creating Atlas '" + texPropertyNames[propIdx].name + "'", 0.01f);
						Color[][] atlasPixels = new Color[atlasSizeY][];
						for (int k = 0; k < atlasPixels.Length; k++)
						{
							atlasPixels[k] = new Color[atlasSizeX];
						}

						bool isNormalMap = false;
						if (texPropertyNames[propIdx].isNormalMap)
						{
							isNormalMap = true;
						}

						for (int texSetIdx = 0; texSetIdx < distinctMaterialTextures.Count; texSetIdx++)
						{
							MB_TexSet texSet = distinctMaterialTextures[texSetIdx];
							if (LOG_LEVEL >= MB2_LogLevel.trace)
							{
								UnityEngine.Debug.Log(string.Format("Adding texture {0} to atlas {1}",
									(!(texSet.ts[propIdx].t == null)) ? texSet.ts[propIdx].t.ToString() : "null",
									texPropertyNames[propIdx]));
							}

							Rect r = uvRects[texSetIdx];
							Texture2D t = texSet.ts[propIdx].t;
							int x = Mathf.RoundToInt(r.x * atlasSizeX);
							int y = Mathf.RoundToInt(r.y * atlasSizeY);
							int ww = Mathf.RoundToInt(r.width * atlasSizeX);
							int hh = Mathf.RoundToInt(r.height * atlasSizeY);
							if (ww == 0 || hh == 0)
							{
								UnityEngine.Debug.LogError("Image in atlas has no height or width");
							}

							textureEditorMethods?.SetReadWriteFlag(t, isReadable: true, addToList: true);
							progressInfo?.Invoke(string.Concat("Copying to atlas: '", texSet.ts[propIdx].t, "'"),
								0.02f);
							yield return CopyScaledAndTiledToAtlas(
								srcSamplingRect: texSet.ts[propIdx].encapsulatingSamplingRect,
								source: texSet.ts[propIdx], sourceMaterial: texSet,
								shaderPropertyName: texPropertyNames[propIdx], targX: x, targY: y, targW: ww, targH: hh,
								_fixOutOfBoundsUVs: _fixOutOfBoundsUVs, maxSize: _maxTilingBakeSize,
								atlasPixels: atlasPixels, atlasWidth: atlasSizeX, isNormalMap: isNormalMap,
								progressInfo: progressInfo);
						}

						yield return numAtlases;
						progressInfo?.Invoke("Applying changes to atlas: '" + texPropertyNames[propIdx].name + "'",
							0.03f);
						atlas2 = new Texture2D(atlasSizeX, atlasSizeY, TextureFormat.ARGB32, mipChain: true);
						for (int l = 0; l < atlasPixels.Length; l++)
						{
							atlas2.SetPixels(0, l, atlasSizeX, 1, atlasPixels[l]);
						}

						atlas2.Apply();
						if (LOG_LEVEL >= MB2_LogLevel.debug)
						{
							UnityEngine.Debug.Log("Saving atlas " + texPropertyNames[propIdx].name + " w=" +
							                      atlas2.width + " h=" + atlas2.height);
						}
					}

					atlases[propIdx] = atlas2;
					progressInfo?.Invoke("Saving atlas: '" + texPropertyNames[propIdx].name + "'", 0.04f);
					if (_saveAtlasesAsAssets && textureEditorMethods != null)
					{
						textureEditorMethods.SaveAtlasToAssetDatabase(atlases[propIdx], texPropertyNames[propIdx],
							propIdx, resultMaterial);
					}
					else
					{
						resultMaterial.SetTexture(texPropertyNames[propIdx].name, atlases[propIdx]);
					}

					resultMaterial.SetTextureOffset(texPropertyNames[propIdx].name, Vector2.zero);
					resultMaterial.SetTextureScale(texPropertyNames[propIdx].name, Vector2.one);
					_destroyTemporaryTextures();
				}
			}

			__createAtlasesMBTexturePacker = uvRects;
		}

		private Rect[] __CreateAtlasesMBTexturePackerFast(ProgressUpdateDelegate progressInfo, int numAtlases,
			List<MB_TexSet> distinctMaterialTextures, List<ShaderTextureProperty> texPropertyNames,
			bool[] allTexturesAreNullAndSameColor, Material resultMaterial, Texture2D[] atlases,
			MB2_EditorMethodsInterface textureEditorMethods, int _padding)
		{
			Rect[] result;
			if (distinctMaterialTextures.Count == 1 && !_fixOutOfBoundsUVs)
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					UnityEngine.Debug.Log("Only one image per atlas. Will re-use original texture");
				}

				result = new Rect[1]
				{
					new Rect(0f, 0f, 1f, 1f)
				};
				for (int i = 0; i < numAtlases; i++)
				{
					MeshBakerMaterialTexture meshBakerMaterialTexture = distinctMaterialTextures[0].ts[i];
					atlases[i] = meshBakerMaterialTexture.t;
					resultMaterial.SetTexture(texPropertyNames[i].name, atlases[i]);
					resultMaterial.SetTextureScale(texPropertyNames[i].name,
						meshBakerMaterialTexture.matTilingRect.size);
					resultMaterial.SetTextureOffset(texPropertyNames[i].name,
						meshBakerMaterialTexture.matTilingRect.min);
				}

				return result;
			}

			List<Vector2> list = new List<Vector2>();
			for (int j = 0; j < distinctMaterialTextures.Count; j++)
			{
				list.Add(new Vector2(distinctMaterialTextures[j].idealWidth, distinctMaterialTextures[j].idealHeight));
			}

			MB2_TexturePacker mB2_TexturePacker = new MB2_TexturePacker();
			mB2_TexturePacker.doPowerOfTwoTextures = _meshBakerTexturePackerForcePowerOfTwo;
			int outW = 1;
			int outH = 1;
			int num = _maxAtlasSize;
			result = mB2_TexturePacker.GetRects(list, num, _padding, out outW, out outH);
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.Log("Generated atlas will be " + outW + "x" + outH +
				                      " (Max atlas size for platform: " + num + ")");
			}

			GameObject gameObject = null;
			try
			{
				gameObject = new GameObject("MBrenderAtlasesGO");
				MB3_AtlasPackerRenderTexture mB3_AtlasPackerRenderTexture =
					gameObject.AddComponent<MB3_AtlasPackerRenderTexture>();
				gameObject.AddComponent<Camera>();
				if (_considerNonTextureProperties && LOG_LEVEL >= MB2_LogLevel.warn)
				{
					UnityEngine.Debug.LogWarning(
						"Blend Non-Texture Properties has limited functionality when used with Mesh Baker Texture Packer Fast.");
				}

				for (int k = 0; k < numAtlases; k++)
				{
					Texture2D texture2D = null;
					if (allTexturesAreNullAndSameColor[k])
					{
						texture2D = null;
						if (LOG_LEVEL >= MB2_LogLevel.debug)
						{
							UnityEngine.Debug.Log("Not creating atlas for " + texPropertyNames[k].name +
							                      " because textures are null and default value parameters are the same.");
						}
					}
					else
					{
						GC.Collect();
						progressInfo?.Invoke("Creating Atlas '" + texPropertyNames[k].name + "'", 0.01f);
						if (LOG_LEVEL >= MB2_LogLevel.debug)
						{
							UnityEngine.Debug.Log("About to render " + texPropertyNames[k].name + " isNormal=" +
							                      texPropertyNames[k].isNormalMap);
						}

						mB3_AtlasPackerRenderTexture.LOG_LEVEL = LOG_LEVEL;
						mB3_AtlasPackerRenderTexture.width = outW;
						mB3_AtlasPackerRenderTexture.height = outH;
						mB3_AtlasPackerRenderTexture.padding = _padding;
						mB3_AtlasPackerRenderTexture.rects = result;
						mB3_AtlasPackerRenderTexture.textureSets = distinctMaterialTextures;
						mB3_AtlasPackerRenderTexture.indexOfTexSetToRender = k;
						mB3_AtlasPackerRenderTexture.texPropertyName = texPropertyNames[k];
						mB3_AtlasPackerRenderTexture.isNormalMap = texPropertyNames[k].isNormalMap;
						mB3_AtlasPackerRenderTexture.fixOutOfBoundsUVs = _fixOutOfBoundsUVs;
						mB3_AtlasPackerRenderTexture.considerNonTextureProperties = _considerNonTextureProperties;
						mB3_AtlasPackerRenderTexture.resultMaterialTextureBlender = resultMaterialTextureBlender;
						texture2D = mB3_AtlasPackerRenderTexture.OnRenderAtlas(this);
						if (LOG_LEVEL >= MB2_LogLevel.debug)
						{
							UnityEngine.Debug.Log("Saving atlas " + texPropertyNames[k].name + " w=" + texture2D.width +
							                      " h=" + texture2D.height + " id=" + texture2D.GetInstanceID());
						}
					}

					atlases[k] = texture2D;
					progressInfo?.Invoke("Saving atlas: '" + texPropertyNames[k].name + "'", 0.04f);
					if (_saveAtlasesAsAssets && textureEditorMethods != null)
					{
						textureEditorMethods.SaveAtlasToAssetDatabase(atlases[k], texPropertyNames[k], k,
							resultMaterial);
					}
					else
					{
						resultMaterial.SetTexture(texPropertyNames[k].name, atlases[k]);
					}

					resultMaterial.SetTextureOffset(texPropertyNames[k].name, Vector2.zero);
					resultMaterial.SetTextureScale(texPropertyNames[k].name, Vector2.one);
					_destroyTemporaryTextures();
				}

				return result;
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
				return result;
			}
			finally
			{
				if (gameObject != null)
				{
					MB_Utility.Destroy(gameObject);
				}
			}
		}

		private Rect[] __CreateAtlasesUnityTexturePacker(ProgressUpdateDelegate progressInfo, int numAtlases,
			List<MB_TexSet> distinctMaterialTextures, List<ShaderTextureProperty> texPropertyNames,
			bool[] allTexturesAreNullAndSameColor, Material resultMaterial, Texture2D[] atlases,
			MB2_EditorMethodsInterface textureEditorMethods, int _padding)
		{
			Rect[] array;
			if (distinctMaterialTextures.Count == 1 && !_fixOutOfBoundsUVs)
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					UnityEngine.Debug.Log("Only one image per atlas. Will re-use original texture");
				}

				array = new Rect[1]
				{
					new Rect(0f, 0f, 1f, 1f)
				};
				for (int i = 0; i < numAtlases; i++)
				{
					MeshBakerMaterialTexture meshBakerMaterialTexture = distinctMaterialTextures[0].ts[i];
					atlases[i] = meshBakerMaterialTexture.t;
					resultMaterial.SetTexture(texPropertyNames[i].name, atlases[i]);
					resultMaterial.SetTextureScale(texPropertyNames[i].name,
						meshBakerMaterialTexture.matTilingRect.size);
					resultMaterial.SetTextureOffset(texPropertyNames[i].name,
						meshBakerMaterialTexture.matTilingRect.min);
				}
			}
			else
			{
				long num = 0L;
				int w = 1;
				int h = 1;
				array = null;
				for (int j = 0; j < numAtlases; j++)
				{
					Texture2D texture2D = null;
					if (allTexturesAreNullAndSameColor[j])
					{
						texture2D = null;
					}
					else
					{
						if (LOG_LEVEL >= MB2_LogLevel.debug)
						{
							UnityEngine.Debug.LogWarning("Beginning loop " + j + " num temporary textures " +
							                             _temporaryTextures.Count);
						}

						for (int k = 0; k < distinctMaterialTextures.Count; k++)
						{
							MB_TexSet mB_TexSet = distinctMaterialTextures[k];
							int idealWidth = mB_TexSet.idealWidth;
							int idealHeight = mB_TexSet.idealHeight;
							Texture2D texture2D2 = mB_TexSet.ts[j].t;
							if (texture2D2 == null)
							{
								texture2D2 = (mB_TexSet.ts[j].t = _createTemporaryTexture(idealWidth, idealHeight,
									TextureFormat.ARGB32, mipMaps: true));
								if (_considerNonTextureProperties && resultMaterialTextureBlender != null)
								{
									Color colorIfNoTexture =
										resultMaterialTextureBlender.GetColorIfNoTexture(mB_TexSet.mats[0].mat,
											texPropertyNames[j]);
									if (LOG_LEVEL >= MB2_LogLevel.trace)
									{
										UnityEngine.Debug.Log("Setting texture to solid color " + colorIfNoTexture);
									}

									MB_Utility.setSolidColor(texture2D2, colorIfNoTexture);
								}
								else
								{
									Color colorIfNoTexture2 = GetColorIfNoTexture(texPropertyNames[j]);
									MB_Utility.setSolidColor(texture2D2, colorIfNoTexture2);
								}
							}

							progressInfo?.Invoke("Adjusting for scale and offset " + texture2D2, 0.01f);
							textureEditorMethods?.SetReadWriteFlag(texture2D2, isReadable: true, addToList: true);
							texture2D2 = GetAdjustedForScaleAndOffset2(mB_TexSet.ts[j], mB_TexSet.obUVoffset,
								mB_TexSet.obUVscale);
							if (texture2D2.width != idealWidth || texture2D2.height != idealHeight)
							{
								progressInfo?.Invoke(string.Concat("Resizing texture '", texture2D2, "'"), 0.01f);
								if (LOG_LEVEL >= MB2_LogLevel.debug)
								{
									UnityEngine.Debug.LogWarning("Copying and resizing texture " +
									                             texPropertyNames[j].name + " from " +
									                             texture2D2.width + "x" + texture2D2.height + " to " +
									                             idealWidth + "x" + idealHeight);
								}

								texture2D2 = _resizeTexture(texture2D2, idealWidth, idealHeight);
							}

							mB_TexSet.ts[j].t = texture2D2;
						}

						Texture2D[] array2 = new Texture2D[distinctMaterialTextures.Count];
						for (int l = 0; l < distinctMaterialTextures.Count; l++)
						{
							Texture2D texture2D3 = distinctMaterialTextures[l].ts[j].t;
							num += texture2D3.width * texture2D3.height;
							if (_considerNonTextureProperties)
							{
								texture2D3 = TintTextureWithTextureCombiner(texture2D3, distinctMaterialTextures[l],
									texPropertyNames[j]);
							}

							array2[l] = texture2D3;
						}

						textureEditorMethods?.CheckBuildSettings(num);
						if (Math.Sqrt(num) > 3500.0 && LOG_LEVEL >= MB2_LogLevel.warn)
						{
							UnityEngine.Debug.LogWarning(
								"The maximum possible atlas size is 4096. Textures may be shrunk");
						}

						texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: true);
						progressInfo?.Invoke("Packing texture atlas " + texPropertyNames[j].name, 0.25f);
						if (j == 0)
						{
							progressInfo?.Invoke("Estimated min size of atlases: " + Math.Sqrt(num).ToString("F0"),
								0.1f);
							if (LOG_LEVEL >= MB2_LogLevel.info)
							{
								UnityEngine.Debug.Log("Estimated atlas minimum size:" + Math.Sqrt(num).ToString("F0"));
							}

							_addWatermark(array2);
							if (distinctMaterialTextures.Count == 1 && !_fixOutOfBoundsUVs)
							{
								array = new Rect[1]
								{
									new Rect(0f, 0f, 1f, 1f)
								};
								texture2D = _copyTexturesIntoAtlas(array2, _padding, array, array2[0].width,
									array2[0].height);
							}
							else
							{
								int maximumAtlasSize = 4096;
								array = texture2D.PackTextures(array2, _padding, maximumAtlasSize,
									makeNoLongerReadable: false);
							}

							if (LOG_LEVEL >= MB2_LogLevel.info)
							{
								UnityEngine.Debug.Log("After pack textures atlas size " + texture2D.width + " " +
								                      texture2D.height);
							}

							w = texture2D.width;
							h = texture2D.height;
							texture2D.Apply();
						}
						else
						{
							progressInfo?.Invoke("Copying Textures Into: " + texPropertyNames[j].name, 0.1f);
							texture2D = _copyTexturesIntoAtlas(array2, _padding, array, w, h);
						}
					}

					atlases[j] = texture2D;
					if (_saveAtlasesAsAssets)
					{
						textureEditorMethods?.SaveAtlasToAssetDatabase(atlases[j], texPropertyNames[j], j,
							resultMaterial);
					}

					resultMaterial.SetTextureOffset(texPropertyNames[j].name, Vector2.zero);
					resultMaterial.SetTextureScale(texPropertyNames[j].name, Vector2.one);
					_destroyTemporaryTextures();
					GC.Collect();
				}
			}

			return array;
		}

		private void _addWatermark(Texture2D[] texToPack)
		{
		}

		private Texture2D _addWatermark(Texture2D texToPack)
		{
			return texToPack;
		}

		private Texture2D _copyTexturesIntoAtlas(Texture2D[] texToPack, int padding, Rect[] rs, int w, int h)
		{
			Texture2D texture2D = new Texture2D(w, h, TextureFormat.ARGB32, mipChain: true);
			MB_Utility.setSolidColor(texture2D, Color.clear);
			for (int i = 0; i < rs.Length; i++)
			{
				Rect rect = rs[i];
				Texture2D texture2D2 = texToPack[i];
				int x = Mathf.RoundToInt(rect.x * w);
				int y = Mathf.RoundToInt(rect.y * h);
				int num = Mathf.RoundToInt(rect.width * w);
				int num2 = Mathf.RoundToInt(rect.height * h);
				if (texture2D2.width != num && texture2D2.height != num2)
				{
					texture2D2 = MB_Utility.resampleTexture(texture2D2, num, num2);
					_temporaryTextures.Add(texture2D2);
				}

				texture2D.SetPixels(x, y, num, num2, texture2D2.GetPixels());
			}

			texture2D.Apply();
			return texture2D;
		}

		private bool IsPowerOfTwo(int x)
		{
			return (x & (x - 1)) == 0;
		}

		private void MergeOverlappingDistinctMaterialTexturesAndCalcMaterialSubrects(
			List<MB_TexSet> distinctMaterialTextures, bool fixOutOfBoundsUVs)
		{
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.Log("MergeOverlappingDistinctMaterialTexturesAndCalcMaterialSubrects");
			}

			int num = 0;
			for (int i = 0; i < distinctMaterialTextures.Count; i++)
			{
				MB_TexSet mB_TexSet = distinctMaterialTextures[i];
				int num2 = -1;
				bool flag = true;
				DRect dRect = default(DRect);
				for (int j = 0; j < mB_TexSet.ts.Length; j++)
				{
					if (num2 != -1)
					{
						if (mB_TexSet.ts[j].t != null && dRect != mB_TexSet.ts[j].matTilingRect)
						{
							flag = false;
						}
					}
					else if (mB_TexSet.ts[j].t != null)
					{
						num2 = j;
						dRect = mB_TexSet.ts[j].matTilingRect;
					}
				}

				if (flag)
				{
					mB_TexSet.allTexturesUseSameMatTiling = true;
					continue;
				}

				if (LOG_LEVEL <= MB2_LogLevel.info)
				{
					UnityEngine.Debug.Log(
						$"Textures in material(s) do not all use the same material tiling. This set of textures will not be considered for merge: {mB_TexSet.GetDescription()} ");
				}

				mB_TexSet.allTexturesUseSameMatTiling = false;
			}

			for (int k = 0; k < distinctMaterialTextures.Count; k++)
			{
				MB_TexSet mB_TexSet2 = distinctMaterialTextures[k];
				DRect obUVRectIfTilingSame = ((!fixOutOfBoundsUVs)
					? new DRect(0.0, 0.0, 1.0, 1.0)
					: new DRect(mB_TexSet2.obUVoffset, mB_TexSet2.obUVscale));
				for (int l = 0; l < mB_TexSet2.mats.Count; l++)
				{
					mB_TexSet2.mats[l].obUVRectIfTilingSame = obUVRectIfTilingSame;
					mB_TexSet2.mats[l].objName = distinctMaterialTextures[k].gos[0].name;
				}

				mB_TexSet2.CalcInitialFullSamplingRects(fixOutOfBoundsUVs);
				if (mB_TexSet2.allTexturesUseSameMatTiling)
				{
					mB_TexSet2.CalcMatAndUVSamplingRectsIfAllMatTilingSame();
				}
			}

			List<int> list = new List<int>();
			for (int m = 0; m < distinctMaterialTextures.Count; m++)
			{
				MB_TexSet mB_TexSet3 = distinctMaterialTextures[m];
				for (int n = m + 1; n < distinctMaterialTextures.Count; n++)
				{
					MB_TexSet mB_TexSet4 = distinctMaterialTextures[n];
					if (!mB_TexSet4.AllTexturesAreSameForMerge(mB_TexSet3, _considerNonTextureProperties,
						    resultMaterialTextureBlender))
					{
						continue;
					}

					double num3 = 0.0;
					double num4 = 0.0;
					DRect dRect2 = default(DRect);
					int num5 = -1;
					for (int num6 = 0; num6 < mB_TexSet3.ts.Length; num6++)
					{
						if (mB_TexSet3.ts[num6].t != null && num5 == -1)
						{
							num5 = num6;
						}
					}

					if (num5 != -1)
					{
						DRect uvRect = mB_TexSet4.mats[0].samplingRectMatAndUVTiling;
						for (int num7 = 1; num7 < mB_TexSet4.mats.Count; num7++)
						{
							uvRect = MB3_UVTransformUtility.GetEncapsulatingRect(ref uvRect,
								ref mB_TexSet4.mats[num7].samplingRectMatAndUVTiling);
						}

						DRect uvRect2 = mB_TexSet3.mats[0].samplingRectMatAndUVTiling;
						for (int num8 = 1; num8 < mB_TexSet3.mats.Count; num8++)
						{
							uvRect2 = MB3_UVTransformUtility.GetEncapsulatingRect(ref uvRect2,
								ref mB_TexSet3.mats[num8].samplingRectMatAndUVTiling);
						}

						dRect2 = MB3_UVTransformUtility.GetEncapsulatingRect(ref uvRect, ref uvRect2);
						num3 += dRect2.width * dRect2.height;
						num4 += uvRect.width * uvRect.height + uvRect2.width * uvRect2.height;
					}
					else
					{
						dRect2 = new DRect(0f, 0f, 1f, 1f);
					}

					if (num3 < num4)
					{
						num++;
						StringBuilder stringBuilder = null;
						if (LOG_LEVEL >= MB2_LogLevel.info)
						{
							stringBuilder = new StringBuilder();
							stringBuilder.AppendFormat("About To Merge:\n   TextureSet1 {0}\n   TextureSet2 {1}\n",
								mB_TexSet4.GetDescription(), mB_TexSet3.GetDescription());
							if (LOG_LEVEL >= MB2_LogLevel.trace)
							{
								for (int num9 = 0; num9 < mB_TexSet4.mats.Count; num9++)
								{
									stringBuilder.AppendFormat(
										"tx1 Mat {0} matAndMeshUVRect {1} fullSamplingRect {2}\n",
										mB_TexSet4.mats[num9].mat, mB_TexSet4.mats[num9].samplingRectMatAndUVTiling,
										mB_TexSet4.ts[0].encapsulatingSamplingRect);
								}

								for (int num10 = 0; num10 < mB_TexSet3.mats.Count; num10++)
								{
									stringBuilder.AppendFormat(
										"tx2 Mat {0} matAndMeshUVRect {1} fullSamplingRect {2}\n",
										mB_TexSet3.mats[num10].mat, mB_TexSet3.mats[num10].samplingRectMatAndUVTiling,
										mB_TexSet3.ts[0].encapsulatingSamplingRect);
								}
							}
						}

						for (int num11 = 0; num11 < mB_TexSet3.gos.Count; num11++)
						{
							if (!mB_TexSet4.gos.Contains(mB_TexSet3.gos[num11]))
							{
								mB_TexSet4.gos.Add(mB_TexSet3.gos[num11]);
							}
						}

						for (int num12 = 0; num12 < mB_TexSet3.mats.Count; num12++)
						{
							mB_TexSet4.mats.Add(mB_TexSet3.mats[num12]);
						}

						mB_TexSet4.mats.Sort(new SamplingRectEnclosesComparer());
						for (int num13 = 0; num13 < mB_TexSet4.ts.Length; num13++)
						{
							mB_TexSet4.ts[num13].encapsulatingSamplingRect = dRect2;
						}

						if (!list.Contains(m))
						{
							list.Add(m);
						}

						if (LOG_LEVEL < MB2_LogLevel.debug)
						{
							break;
						}

						if (LOG_LEVEL >= MB2_LogLevel.trace)
						{
							stringBuilder.AppendFormat("=== After Merge TextureSet {0}\n", mB_TexSet4.GetDescription());
							for (int num14 = 0; num14 < mB_TexSet4.mats.Count; num14++)
							{
								stringBuilder.AppendFormat("tx1 Mat {0} matAndMeshUVRect {1} fullSamplingRect {2}\n",
									mB_TexSet4.mats[num14].mat, mB_TexSet4.mats[num14].samplingRectMatAndUVTiling,
									mB_TexSet4.ts[0].encapsulatingSamplingRect);
							}

							DRect r = mB_TexSet4.ts[0].encapsulatingSamplingRect;
							MB3_UVTransformUtility.Canonicalize(ref r, 0.0, 0.0);
							for (int num15 = 0; num15 < mB_TexSet4.mats.Count; num15++)
							{
								DRect r2 = mB_TexSet4.mats[num15].samplingRectMatAndUVTiling;
								MB3_UVTransformUtility.Canonicalize(ref r2, r.x, r.y);
								Rect rect = default(Rect);
								DRect r3 = mB_TexSet4.mats[num15].obUVRectIfTilingSame;
								DRect r4 = mB_TexSet4.mats[num15].materialTiling;
								Rect rect2 = r.GetRect();
								rect = MB3_UVTransformUtility.CombineTransforms(ref r3, ref r4).GetRect();
								MB3_UVTransformUtility.Canonicalize(ref rect, (float)r.x, (float)r.y);
								if (!mB_TexSet4.ts[0].encapsulatingSamplingRect
									    .Encloses(mB_TexSet4.mats[num15].samplingRectMatAndUVTiling))
								{
									stringBuilder.AppendFormat(string.Concat("mesh ", mB_TexSet4.mats[num15].objName,
										"\n uv=", r3, "\n mat=", r4.GetRect().ToString("f5"), "\n samplingRect=",
										mB_TexSet4.mats[num15].samplingRectMatAndUVTiling.GetRect().ToString("f4"),
										"\n samplingRectCannonical=", r2.GetRect().ToString("f4"),
										"\n potentialRect (cannonicalized)=", rect.ToString("f4"),
										"\n encapsulatingRect ",
										mB_TexSet4.ts[0].encapsulatingSamplingRect.GetRect().ToString("f4"),
										"\n encapsulatingRectCannonical=", rect2.ToString("f4"), "\n\n"));
									stringBuilder.AppendFormat(string.Format("Integrity check failed. " +
									                                         mB_TexSet4.mats[num15].objName +
									                                         " Encapsulating rect cannonical failed to contain samplingRectMatAndUVTiling cannonical\n"));
								}
							}
						}

						UnityEngine.Debug.Log(stringBuilder.ToString());
						break;
					}

					if (LOG_LEVEL >= MB2_LogLevel.debug)
					{
						UnityEngine.Debug.Log(
							$"Considered merging {mB_TexSet4.GetDescription()} and {mB_TexSet3.GetDescription()} but there was not enough overlap. It is more efficient to bake these to separate rectangles.");
					}
				}
			}

			for (int num16 = list.Count - 1; num16 >= 0; num16--)
			{
				distinctMaterialTextures.RemoveAt(list[num16]);
			}

			list.Clear();
			if (LOG_LEVEL >= MB2_LogLevel.info)
			{
				UnityEngine.Debug.Log(
					$"MergeOverlappingDistinctMaterialTexturesAndCalcMaterialSubrects complete merged {num}");
			}
		}

		private Vector2 GetAdjustedForScaleAndOffset2Dimensions(MeshBakerMaterialTexture source, Vector2 obUVoffset,
			Vector2 obUVscale)
		{
			if (source.matTilingRect.x == 0.0 && source.matTilingRect.y == 0.0 && source.matTilingRect.width == 1.0 &&
			    source.matTilingRect.height == 1.0)
			{
				if (!_fixOutOfBoundsUVs)
				{
					return new Vector2(source.t.width, source.t.height);
				}

				if (obUVoffset.x == 0f && obUVoffset.y == 0f && obUVscale.x == 1f && obUVscale.y == 1f)
				{
					return new Vector2(source.t.width, source.t.height);
				}
			}

			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.Log(string.Concat("GetAdjustedForScaleAndOffset2Dimensions: ", source.t, " ",
					obUVoffset, " ", obUVscale));
			}

			float num = (float)source.encapsulatingSamplingRect.width * source.t.width;
			float num2 = (float)source.encapsulatingSamplingRect.height * source.t.height;
			if (num > _maxTilingBakeSize)
			{
				num = _maxTilingBakeSize;
			}

			if (num2 > _maxTilingBakeSize)
			{
				num2 = _maxTilingBakeSize;
			}

			if (num < 1f)
			{
				num = 1f;
			}

			if (num2 < 1f)
			{
				num2 = 1f;
			}

			return new Vector2(num, num2);
		}

		public Texture2D GetAdjustedForScaleAndOffset2(MeshBakerMaterialTexture source, Vector2 obUVoffset,
			Vector2 obUVscale)
		{
			if (source.matTilingRect.x == 0.0 && source.matTilingRect.y == 0.0 && source.matTilingRect.width == 1.0 &&
			    source.matTilingRect.height == 1.0)
			{
				if (!_fixOutOfBoundsUVs)
				{
					return source.t;
				}

				if (obUVoffset.x == 0f && obUVoffset.y == 0f && obUVscale.x == 1f && obUVscale.y == 1f)
				{
					return source.t;
				}
			}

			Vector2 adjustedForScaleAndOffset2Dimensions =
				GetAdjustedForScaleAndOffset2Dimensions(source, obUVoffset, obUVscale);
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.LogWarning(string.Concat("GetAdjustedForScaleAndOffset2: ", source.t, " ", obUVoffset,
					" ", obUVscale));
			}

			float x = adjustedForScaleAndOffset2Dimensions.x;
			float y = adjustedForScaleAndOffset2Dimensions.y;
			float num = (float)source.matTilingRect.width;
			float num2 = (float)source.matTilingRect.height;
			float num3 = (float)source.matTilingRect.x;
			float num4 = (float)source.matTilingRect.y;
			if (_fixOutOfBoundsUVs)
			{
				num *= obUVscale.x;
				num2 *= obUVscale.y;
				num3 = (float)(source.matTilingRect.x * obUVscale.x + obUVoffset.x);
				num4 = (float)(source.matTilingRect.y * obUVscale.y + obUVoffset.y);
			}

			Texture2D texture2D = _createTemporaryTexture((int)x, (int)y, TextureFormat.ARGB32, mipMaps: true);
			for (int i = 0; i < texture2D.width; i++)
			{
				for (int j = 0; j < texture2D.height; j++)
				{
					float x2 = i / x * num + num3;
					float y2 = j / y * num2 + num4;
					texture2D.SetPixel(i, j, source.t.GetPixelBilinear(x2, y2));
				}
			}

			texture2D.Apply();
			return texture2D;
		}

		internal static DRect GetSourceSamplingRect(MeshBakerMaterialTexture source, Vector2 obUVoffset,
			Vector2 obUVscale)
		{
			DRect r = source.matTilingRect;
			DRect r2 = new DRect(obUVoffset, obUVscale);
			return MB3_UVTransformUtility.CombineTransforms(ref r, ref r2);
		}

		private Texture2D TintTextureWithTextureCombiner(Texture2D t, MB_TexSet sourceMaterial,
			ShaderTextureProperty shaderPropertyName)
		{
			if (LOG_LEVEL >= MB2_LogLevel.trace)
			{
				UnityEngine.Debug.Log(
					$"Blending texture {t.name} mat {sourceMaterial.mats[0].mat} with non-texture properties using TextureBlender {resultMaterialTextureBlender}");
			}

			resultMaterialTextureBlender.OnBeforeTintTexture(sourceMaterial.mats[0].mat, shaderPropertyName.name);
			t = _createTextureCopy(t);
			for (int i = 0; i < t.height; i++)
			{
				Color[] pixels = t.GetPixels(0, i, t.width, 1);
				for (int j = 0; j < pixels.Length; j++)
				{
					ref Color reference = ref pixels[j];
					reference = resultMaterialTextureBlender.OnBlendTexturePixel(shaderPropertyName.name, pixels[j]);
				}

				t.SetPixels(0, i, t.width, 1, pixels);
			}

			t.Apply();
			return t;
		}

		public IEnumerator CopyScaledAndTiledToAtlas(MeshBakerMaterialTexture source, MB_TexSet sourceMaterial,
			ShaderTextureProperty shaderPropertyName, DRect srcSamplingRect, int targX, int targY, int targW, int targH,
			bool _fixOutOfBoundsUVs, int maxSize, Color[][] atlasPixels, int atlasWidth, bool isNormalMap,
			ProgressUpdateDelegate progressInfo = null)
		{
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.Log(string.Concat("CopyScaledAndTiledToAtlas: ", source.t, " inAtlasX=", targX,
					" inAtlasY=", targY, " inAtlasW=", targW, " inAtlasH=", targH));
			}

			float newWidth = targW;
			float newHeight = targH;
			float scx = (float)srcSamplingRect.width;
			float scy = (float)srcSamplingRect.height;
			float ox = (float)srcSamplingRect.x;
			float oy = (float)srcSamplingRect.y;
			int w = (int)newWidth;
			int h = (int)newHeight;
			Texture2D t2 = source.t;
			if (t2 == null)
			{
				if (LOG_LEVEL >= MB2_LogLevel.trace)
				{
					UnityEngine.Debug.Log("No source texture creating a 16x16 texture.");
				}

				t2 = _createTemporaryTexture(16, 16, TextureFormat.ARGB32, mipMaps: true);
				scx = 1f;
				scy = 1f;
				if (_considerNonTextureProperties && resultMaterialTextureBlender != null)
				{
					Color colorIfNoTexture =
						resultMaterialTextureBlender.GetColorIfNoTexture(sourceMaterial.mats[0].mat,
							shaderPropertyName);
					if (LOG_LEVEL >= MB2_LogLevel.trace)
					{
						UnityEngine.Debug.Log("Setting texture to solid color " + colorIfNoTexture);
					}

					MB_Utility.setSolidColor(t2, colorIfNoTexture);
				}
				else
				{
					Color colorIfNoTexture2 = GetColorIfNoTexture(shaderPropertyName);
					MB_Utility.setSolidColor(t2, colorIfNoTexture2);
				}
			}

			if (_considerNonTextureProperties && resultMaterialTextureBlender != null)
			{
				t2 = TintTextureWithTextureCombiner(t2, sourceMaterial, shaderPropertyName);
			}

			t2 = _addWatermark(t2);
			for (int k = 0; k < w; k++)
			{
				if (progressInfo != null && w > 0)
				{
					progressInfo("CopyScaledAndTiledToAtlas " + (k / (float)w * 100f).ToString("F0"), 0.2f);
				}

				for (int l = 0; l < h; l++)
				{
					float x = k / newWidth * scx + ox;
					float y = l / newHeight * scy + oy;
					Color reference = atlasPixels[targY + l][targX + k];
					reference = t2.GetPixelBilinear(x, y);
				}
			}

			for (int m = 0; m < w; m++)
			{
				for (int n = 1; n <= atlasPadding; n++)
				{
					Color reference2 = atlasPixels[targY - n][targX + m];
					reference2 = atlasPixels[targY][targX + m];
					Color reference3 = atlasPixels[targY + h - 1 + n][targX + m];
					reference3 = atlasPixels[targY + h - 1][targX + m];
				}
			}

			for (int num = 0; num < h; num++)
			{
				for (int num2 = 1; num2 <= _atlasPadding; num2++)
				{
					Color reference4 = atlasPixels[targY + num][targX - num2];
					reference4 = atlasPixels[targY + num][targX];
					Color reference5 = atlasPixels[targY + num][targX + w + num2 - 1];
					reference5 = atlasPixels[targY + num][targX + w - 1];
				}
			}

			for (int i = 1; i <= _atlasPadding; i++)
			{
				for (int j = 1; j <= _atlasPadding; j++)
				{
					Color reference6 = atlasPixels[targY - j][targX - i];
					reference6 = atlasPixels[targY][targX];
					Color reference7 = atlasPixels[targY + h - 1 + j][targX - i];
					reference7 = atlasPixels[targY + h - 1][targX];
					Color reference8 = atlasPixels[targY + h - 1 + j][targX + w + i - 1];
					reference8 = atlasPixels[targY + h - 1][targX + w - 1];
					Color reference9 = atlasPixels[targY - j][targX + w + i - 1];
					reference9 = atlasPixels[targY][targX + w - 1];
					yield return null;
				}

				yield return null;
			}
		}

		public Texture2D _createTemporaryTexture(int w, int h, TextureFormat texFormat, bool mipMaps)
		{
			Texture2D texture2D = new Texture2D(w, h, texFormat, mipMaps);
			MB_Utility.setSolidColor(texture2D, Color.clear);
			_temporaryTextures.Add(texture2D);
			return texture2D;
		}

		internal Texture2D _createTextureCopy(Texture2D t)
		{
			Texture2D texture2D = MB_Utility.createTextureCopy(t);
			_temporaryTextures.Add(texture2D);
			return texture2D;
		}

		private Texture2D _resizeTexture(Texture2D t, int w, int h)
		{
			Texture2D texture2D = MB_Utility.resampleTexture(t, w, h);
			_temporaryTextures.Add(texture2D);
			return texture2D;
		}

		private void _destroyTemporaryTextures()
		{
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.Log("Destroying " + _temporaryTextures.Count + " temporary textures");
			}

			for (int i = 0; i < _temporaryTextures.Count; i++)
			{
				MB_Utility.Destroy(_temporaryTextures[i]);
			}

			_temporaryTextures.Clear();
		}

		public void SuggestTreatment(List<GameObject> objsToMesh, Material[] resultMaterials,
			List<ShaderTextureProperty> _customShaderPropNames)
		{
			this._customShaderPropNames = _customShaderPropNames;
			StringBuilder stringBuilder = new StringBuilder();
			Dictionary<int, MB_Utility.MeshAnalysisResult[]> dictionary =
				new Dictionary<int, MB_Utility.MeshAnalysisResult[]>();
			for (int i = 0; i < objsToMesh.Count; i++)
			{
				GameObject gameObject = objsToMesh[i];
				if (gameObject == null)
				{
					continue;
				}

				Material[] gOMaterials = MB_Utility.GetGOMaterials(objsToMesh[i]);
				if (gOMaterials.Length > 1)
				{
					stringBuilder.AppendFormat("\nObject {0} uses {1} materials. Possible treatments:\n",
						objsToMesh[i].name, gOMaterials.Length);
					stringBuilder.AppendFormat(
						"  1) Collapse the submeshes together into one submesh in the combined mesh. Each of the original submesh materials will map to a different UV rectangle in the atlas(es) used by the combined material.\n");
					stringBuilder.AppendFormat(
						"  2) Use the multiple materials feature to map submeshes in the source mesh to submeshes in the combined mesh.\n");
				}

				Mesh mesh = MB_Utility.GetMesh(gameObject);
				if (!dictionary.TryGetValue(mesh.GetInstanceID(), out var value))
				{
					value = new MB_Utility.MeshAnalysisResult[mesh.subMeshCount];
					MB_Utility.doSubmeshesShareVertsOrTris(mesh, ref value[0]);
					for (int j = 0; j < mesh.subMeshCount; j++)
					{
						MB_Utility.hasOutOfBoundsUVs(mesh, ref value[j], j);
						value[j].hasOverlappingSubmeshTris = value[0].hasOverlappingSubmeshTris;
						value[j].hasOverlappingSubmeshVerts = value[0].hasOverlappingSubmeshVerts;
					}

					dictionary.Add(mesh.GetInstanceID(), value);
				}

				for (int k = 0; k < gOMaterials.Length; k++)
				{
					if (value[k].hasOutOfBoundsUVs)
					{
						DRect dRect = new DRect(value[k].uvRect);
						stringBuilder.AppendFormat(
							"\nObject {0} submesh={1} material={2} uses UVs outside the range 0,0 .. 1,1 to create tiling that tiles the box {3},{4} .. {5},{6}. This is a problem because the UVs outside the 0,0 .. 1,1 rectangle will pick up neighboring textures in the atlas. Possible Treatments:\n",
							gameObject, k, gOMaterials[k], dRect.x.ToString("G4"), dRect.y.ToString("G4"),
							(dRect.x + dRect.width).ToString("G4"), (dRect.y + dRect.height).ToString("G4"));
						stringBuilder.AppendFormat(
							"    1) Ignore the problem. The tiling may not affect result significantly.\n");
						stringBuilder.AppendFormat(
							"    2) Use the 'fix out of bounds UVs' feature to bake the tiling and scale the UVs to fit in the 0,0 .. 1,1 rectangle.\n");
						stringBuilder.AppendFormat(
							"    3) Use the Multiple Materials feature to map the material on this submesh to its own submesh in the combined mesh. No other materials should map to this submesh. This will result in only one texture in the atlas(es) and the UVs should tile correctly.\n");
						stringBuilder.AppendFormat(
							"    4) Combine only meshes that use the same (or subset of) the set of materials on this mesh. The original material(s) can be applied to the result\n");
					}
				}

				if (value[0].hasOverlappingSubmeshVerts)
				{
					stringBuilder.AppendFormat(
						"\nObject {0} has submeshes that share vertices. This is a problem because each vertex can have only one UV coordinate and may be required to map to different positions in the various atlases that are generated. Possible treatments:\n",
						objsToMesh[i]);
					stringBuilder.AppendFormat(" 1) Ignore the problem. The vertices may not affect the result.\n");
					stringBuilder.AppendFormat(
						" 2) Use the Multiple Materials feature to map the submeshs that overlap to their own submeshs in the combined mesh. No other materials should map to this submesh. This will result in only one texture in the atlas(es) and the UVs should tile correctly.\n");
					stringBuilder.AppendFormat(
						" 3) Combine only meshes that use the same (or subset of) the set of materials on this mesh. The original material(s) can be applied to the result\n");
				}
			}

			Dictionary<Material, List<GameObject>> dictionary2 = new Dictionary<Material, List<GameObject>>();
			for (int l = 0; l < objsToMesh.Count; l++)
			{
				if (!(objsToMesh[l] != null))
				{
					continue;
				}

				Material[] gOMaterials2 = MB_Utility.GetGOMaterials(objsToMesh[l]);
				for (int m = 0; m < gOMaterials2.Length; m++)
				{
					if (gOMaterials2[m] != null)
					{
						if (!dictionary2.TryGetValue(gOMaterials2[m], out var value2))
						{
							value2 = new List<GameObject>();
							dictionary2.Add(gOMaterials2[m], value2);
						}

						if (!value2.Contains(objsToMesh[l]))
						{
							value2.Add(objsToMesh[l]);
						}
					}
				}
			}

			List<ShaderTextureProperty> list = new List<ShaderTextureProperty>();
			for (int n = 0; n < resultMaterials.Length; n++)
			{
				_CollectPropertyNames(resultMaterials[n], list);
				foreach (Material key in dictionary2.Keys)
				{
					for (int num = 0; num < list.Count; num++)
					{
						if (!key.HasProperty(list[num].name))
						{
							continue;
						}

						Texture texture = key.GetTexture(list[num].name);
						if (texture != null)
						{
							Vector2 textureOffset = key.GetTextureOffset(list[num].name);
							Vector3 vector = key.GetTextureScale(list[num].name);
							if (textureOffset.x < 0f || textureOffset.x + vector.x > 1f || textureOffset.y < 0f ||
							    textureOffset.y + vector.y > 1f)
							{
								stringBuilder.AppendFormat(
									"\nMaterial {0} used by objects {1} uses texture {2} that is tiled (scale={3} offset={4}). If there is more than one texture in the atlas  then Mesh Baker will bake the tiling into the atlas. If the baked tiling is large then quality can be lost. Possible treatments:\n",
									key, PrintList(dictionary2[key]), texture, vector, textureOffset);
								stringBuilder.AppendFormat("  1) Use the baked tiling.\n");
								stringBuilder.AppendFormat(
									"  2) Use the Multiple Materials feature to map the material on this object/submesh to its own submesh in the combined mesh. No other materials should map to this submesh. The original material can be applied to this submesh.\n");
								stringBuilder.AppendFormat(
									"  3) Combine only meshes that use the same (or subset of) the set of textures on this mesh. The original material can be applied to the result.\n");
							}
						}
					}
				}
			}

			string empty = string.Empty;
			empty = ((stringBuilder.Length != 0)
				? (
					"====== There are possible problems with these meshes that may prevent them from combining well. TREATMENT SUGGESTIONS (copy and paste to text editor if too big) =====\n" +
					stringBuilder.ToString())
				: "====== No problems detected. These meshes should combine well ====\n  If there are problems with the combined meshes please report the problem to digitalOpus.ca so we can improve Mesh Baker.");
			UnityEngine.Debug.Log(empty);
		}

		private TextureBlender FindMatchingTextureBlender(string shaderName)
		{
			for (int i = 0; i < textureBlenders.Length; i++)
			{
				if (textureBlenders[i].DoesShaderNameMatch(shaderName))
				{
					return textureBlenders[i];
				}
			}

			return null;
		}

		private void AdjustNonTextureProperties(Material mat, List<ShaderTextureProperty> texPropertyNames,
			List<MB_TexSet> distinctMaterialTextures, bool considerTintColor, MB2_EditorMethodsInterface editorMethods)
		{
			if (mat == null || texPropertyNames == null)
			{
				return;
			}

			if (_considerNonTextureProperties)
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					UnityEngine.Debug.Log("Adjusting non texture properties using TextureBlender for shader: " +
					                      mat.shader.name);
				}

				resultMaterialTextureBlender.SetNonTexturePropertyValuesOnResultMaterial(mat);
				return;
			}

			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.Log("Adjusting non texture properties on result material");
			}

			for (int i = 0; i < texPropertyNames.Count; i++)
			{
				string name = texPropertyNames[i].name;
				if (name.Equals("_MainTex") && mat.HasProperty("_Color"))
				{
					try
					{
						if (considerTintColor)
						{
							mat.SetColor("_Color", Color.white);
						}
					}
					catch (Exception)
					{
					}
				}

				if (name.Equals("_BumpMap") && mat.HasProperty("_BumpScale"))
				{
					try
					{
						mat.SetFloat("_BumpScale", 1f);
					}
					catch (Exception)
					{
					}
				}

				if (name.Equals("_ParallaxMap") && mat.HasProperty("_Parallax"))
				{
					try
					{
						mat.SetFloat("_Parallax", 0.02f);
					}
					catch (Exception)
					{
					}
				}

				if (name.Equals("_OcclusionMap") && mat.HasProperty("_OcclusionStrength"))
				{
					try
					{
						mat.SetFloat("_OcclusionStrength", 1f);
					}
					catch (Exception)
					{
					}
				}

				if (!name.Equals("_EmissionMap"))
				{
					continue;
				}

				if (mat.HasProperty("_EmissionColor"))
				{
					try
					{
						mat.SetColor("_EmissionColor", new Color(0f, 0f, 0f, 0f));
					}
					catch (Exception)
					{
					}
				}

				if (mat.HasProperty("_EmissionScaleUI"))
				{
					try
					{
						mat.SetFloat("_EmissionScaleUI", 1f);
					}
					catch (Exception)
					{
					}
				}
			}

			editorMethods?.CommitChangesToAssets();
		}

		public static Color GetColorIfNoTexture(ShaderTextureProperty texProperty)
		{
			if (texProperty.isNormalMap)
			{
				return new Color(0.5f, 0.5f, 1f);
			}

			if (texProperty.name.Equals("_ParallaxMap"))
			{
				return new Color(0f, 0f, 0f, 0f);
			}

			if (texProperty.name.Equals("_OcclusionMap"))
			{
				return new Color(1f, 1f, 1f, 1f);
			}

			if (texProperty.name.Equals("_EmissionMap"))
			{
				return new Color(0f, 0f, 0f, 0f);
			}

			if (texProperty.name.Equals("_DetailMask"))
			{
				return new Color(0f, 0f, 0f, 0f);
			}

			return new Color(1f, 1f, 1f, 0f);
		}

		private Color32 ConvertNormalFormatFromUnity_ToStandard(Color32 c)
		{
			Vector3 zero = Vector3.zero;
			zero.x = c.a * 2f - 1f;
			zero.y = c.g * 2f - 1f;
			zero.z = Mathf.Sqrt(1f - zero.x * zero.x - zero.y * zero.y);
			Color32 result = default(Color32);
			result.a = 1;
			result.r = (byte)((zero.x + 1f) * 0.5f);
			result.g = (byte)((zero.y + 1f) * 0.5f);
			result.b = (byte)((zero.z + 1f) * 0.5f);
			return result;
		}

		private float GetSubmeshArea(Mesh m, int submeshIdx)
		{
			if (submeshIdx >= m.subMeshCount || submeshIdx < 0)
			{
				return 0f;
			}

			Vector3[] vertices = m.vertices;
			int[] indices = m.GetIndices(submeshIdx);
			float num = 0f;
			for (int i = 0; i < indices.Length; i += 3)
			{
				Vector3 vector = vertices[indices[i]];
				Vector3 vector2 = vertices[indices[i + 1]];
				Vector3 vector3 = vertices[indices[i + 2]];
				num += Vector3.Cross(vector2 - vector, vector3 - vector).magnitude / 2f;
			}

			return num;
		}

		private string PrintList(List<GameObject> gos)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < gos.Count; i++)
			{
				stringBuilder.Append(string.Concat(gos[i], ","));
			}

			return stringBuilder.ToString();
		}
	}
}
