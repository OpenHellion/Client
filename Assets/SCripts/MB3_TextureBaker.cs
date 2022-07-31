using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DigitalOpus.MB.Core;
using UnityEngine;

public class MB3_TextureBaker : MB3_MeshBakerRoot
{
	public delegate void OnCombinedTexturesCoroutineSuccess();

	public delegate void OnCombinedTexturesCoroutineFail();

	public class CreateAtlasesCoroutineResult
	{
		public bool success = true;

		public bool isFinished;
	}

	public MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;

	[SerializeField]
	protected MB2_TextureBakeResults _textureBakeResults;

	[SerializeField]
	protected int _atlasPadding = 1;

	[SerializeField]
	protected int _maxAtlasSize = 4096;

	[SerializeField]
	protected bool _resizePowerOfTwoTextures;

	[SerializeField]
	protected bool _fixOutOfBoundsUVs;

	[SerializeField]
	protected int _maxTilingBakeSize = 1024;

	[SerializeField]
	protected MB2_PackingAlgorithmEnum _packingAlgorithm = MB2_PackingAlgorithmEnum.MeshBakerTexturePacker;

	[SerializeField]
	protected bool _meshBakerTexturePackerForcePowerOfTwo = true;

	[SerializeField]
	protected List<ShaderTextureProperty> _customShaderProperties = new List<ShaderTextureProperty>();

	[SerializeField]
	protected List<string> _customShaderPropNames_Depricated = new List<string>();

	[SerializeField]
	protected bool _doMultiMaterial;

	[SerializeField]
	protected Material _resultMaterial;

	[SerializeField]
	protected bool _considerNonTextureProperties = true;

	[SerializeField]
	protected bool _doSuggestTreatment = true;

	public MB_MultiMaterial[] resultMaterials = new MB_MultiMaterial[0];

	public List<GameObject> objsToMesh;

	public OnCombinedTexturesCoroutineSuccess onBuiltAtlasesSuccess;

	public OnCombinedTexturesCoroutineFail onBuiltAtlasesFail;

	public MB_AtlasesAndRects[] OnCombinedTexturesCoroutineAtlasesAndRects;

	public override MB2_TextureBakeResults textureBakeResults
	{
		get
		{
			return _textureBakeResults;
		}
		set
		{
			_textureBakeResults = value;
		}
	}

	public virtual int atlasPadding
	{
		get
		{
			return _atlasPadding;
		}
		set
		{
			_atlasPadding = value;
		}
	}

	public virtual int maxAtlasSize
	{
		get
		{
			return _maxAtlasSize;
		}
		set
		{
			_maxAtlasSize = value;
		}
	}

	public virtual bool resizePowerOfTwoTextures
	{
		get
		{
			return _resizePowerOfTwoTextures;
		}
		set
		{
			_resizePowerOfTwoTextures = value;
		}
	}

	public virtual bool fixOutOfBoundsUVs
	{
		get
		{
			return _fixOutOfBoundsUVs;
		}
		set
		{
			_fixOutOfBoundsUVs = value;
		}
	}

	public virtual int maxTilingBakeSize
	{
		get
		{
			return _maxTilingBakeSize;
		}
		set
		{
			_maxTilingBakeSize = value;
		}
	}

	public virtual MB2_PackingAlgorithmEnum packingAlgorithm
	{
		get
		{
			return _packingAlgorithm;
		}
		set
		{
			_packingAlgorithm = value;
		}
	}

	public bool meshBakerTexturePackerForcePowerOfTwo
	{
		get
		{
			return _meshBakerTexturePackerForcePowerOfTwo;
		}
		set
		{
			_meshBakerTexturePackerForcePowerOfTwo = value;
		}
	}

	public virtual List<ShaderTextureProperty> customShaderProperties
	{
		get
		{
			return _customShaderProperties;
		}
		set
		{
			_customShaderProperties = value;
		}
	}

	public virtual List<string> customShaderPropNames
	{
		get
		{
			return _customShaderPropNames_Depricated;
		}
		set
		{
			_customShaderPropNames_Depricated = value;
		}
	}

	public virtual bool doMultiMaterial
	{
		get
		{
			return _doMultiMaterial;
		}
		set
		{
			_doMultiMaterial = value;
		}
	}

	public virtual Material resultMaterial
	{
		get
		{
			return _resultMaterial;
		}
		set
		{
			_resultMaterial = value;
		}
	}

	public bool considerNonTextureProperties
	{
		get
		{
			return _considerNonTextureProperties;
		}
		set
		{
			_considerNonTextureProperties = value;
		}
	}

	public bool doSuggestTreatment
	{
		get
		{
			return _doSuggestTreatment;
		}
		set
		{
			_doSuggestTreatment = value;
		}
	}

	public override List<GameObject> GetObjectsToCombine()
	{
		if (objsToMesh == null)
		{
			objsToMesh = new List<GameObject>();
		}
		return objsToMesh;
	}

	public MB_AtlasesAndRects[] CreateAtlases()
	{
		return CreateAtlases(null);
	}

	public IEnumerator CreateAtlasesCoroutine(ProgressUpdateDelegate progressInfo, CreateAtlasesCoroutineResult coroutineResult, bool saveAtlasesAsAssets = false, MB2_EditorMethodsInterface editorMethods = null, float maxTimePerFrame = 0.01f)
	{
		MBVersionConcrete mbv = new MBVersionConcrete();
		if (!MB3_TextureCombiner._RunCorutineWithoutPauseIsRunning && (mbv.GetMajorVersion() < 5 || (mbv.GetMajorVersion() == 5 && mbv.GetMinorVersion() < 3)))
		{
			Debug.LogError("Running the texture combiner as a coroutine only works in Unity 5.3 and higher");
			yield return null;
		}
		OnCombinedTexturesCoroutineAtlasesAndRects = null;
		if (maxTimePerFrame <= 0f)
		{
			Debug.LogError("maxTimePerFrame must be a value greater than zero");
			coroutineResult.isFinished = true;
			yield break;
		}
		MB2_ValidationLevel vl = (Application.isPlaying ? MB2_ValidationLevel.quick : MB2_ValidationLevel.robust);
		if (!MB3_MeshBakerRoot.DoCombinedValidate(this, MB_ObjsToCombineTypes.dontCare, null, vl))
		{
			coroutineResult.isFinished = true;
			yield break;
		}
		if (_doMultiMaterial && !_ValidateResultMaterials())
		{
			coroutineResult.isFinished = true;
			yield break;
		}
		if (!_doMultiMaterial)
		{
			if (_resultMaterial == null)
			{
				Debug.LogError("Combined Material is null please create and assign a result material.");
				coroutineResult.isFinished = true;
				yield break;
			}
			Shader shader = _resultMaterial.shader;
			for (int j = 0; j < objsToMesh.Count; j++)
			{
				Material[] gOMaterials = MB_Utility.GetGOMaterials(objsToMesh[j]);
				foreach (Material material in gOMaterials)
				{
					if (material != null && material.shader != shader)
					{
						Debug.LogWarning(string.Concat("Game object ", objsToMesh[j], " does not use shader ", shader, " it may not have the required textures. If not small solid color textures will be generated."));
					}
				}
			}
		}
		for (int l = 0; l < objsToMesh.Count; l++)
		{
			Material[] gOMaterials2 = MB_Utility.GetGOMaterials(objsToMesh[l]);
			foreach (Material material2 in gOMaterials2)
			{
				if (material2 == null)
				{
					Debug.LogError(string.Concat("Game object ", objsToMesh[l], " has a null material. Can't build atlases"));
					coroutineResult.isFinished = true;
					yield break;
				}
			}
		}
		MB3_TextureCombiner combiner = new MB3_TextureCombiner
		{
			LOG_LEVEL = LOG_LEVEL,
			atlasPadding = _atlasPadding,
			maxAtlasSize = _maxAtlasSize,
			customShaderPropNames = _customShaderProperties,
			fixOutOfBoundsUVs = _fixOutOfBoundsUVs,
			maxTilingBakeSize = _maxTilingBakeSize,
			packingAlgorithm = _packingAlgorithm,
			meshBakerTexturePackerForcePowerOfTwo = _meshBakerTexturePackerForcePowerOfTwo,
			resizePowerOfTwoTextures = _resizePowerOfTwoTextures,
			saveAtlasesAsAssets = saveAtlasesAsAssets,
			considerNonTextureProperties = _considerNonTextureProperties
		};
		int numResults = 1;
		if (_doMultiMaterial)
		{
			numResults = resultMaterials.Length;
		}
		OnCombinedTexturesCoroutineAtlasesAndRects = new MB_AtlasesAndRects[numResults];
		for (int n = 0; n < OnCombinedTexturesCoroutineAtlasesAndRects.Length; n++)
		{
			OnCombinedTexturesCoroutineAtlasesAndRects[n] = new MB_AtlasesAndRects();
		}
		for (int i = 0; i < OnCombinedTexturesCoroutineAtlasesAndRects.Length; i++)
		{
			Material resMatToPass2 = null;
			List<Material> sourceMats = null;
			if (_doMultiMaterial)
			{
				sourceMats = resultMaterials[i].sourceMaterials;
				resMatToPass2 = resultMaterials[i].combinedMaterial;
			}
			else
			{
				resMatToPass2 = _resultMaterial;
			}
			Debug.Log(string.Format("Creating atlases for result material {0} using shader {1}", resMatToPass2, resMatToPass2.shader));
			MB3_TextureCombiner.CombineTexturesIntoAtlasesCoroutineResult coroutineResult2 = new MB3_TextureCombiner.CombineTexturesIntoAtlasesCoroutineResult();
			yield return combiner.CombineTexturesIntoAtlasesCoroutine(progressInfo, OnCombinedTexturesCoroutineAtlasesAndRects[i], resMatToPass2, objsToMesh, sourceMats, editorMethods, coroutineResult2, maxTimePerFrame);
			coroutineResult.success = coroutineResult2.success;
			if (!coroutineResult.success)
			{
				coroutineResult.isFinished = true;
				yield break;
			}
		}
		textureBakeResults.combinedMaterialInfo = OnCombinedTexturesCoroutineAtlasesAndRects;
		textureBakeResults.doMultiMaterial = _doMultiMaterial;
		textureBakeResults.resultMaterial = _resultMaterial;
		textureBakeResults.resultMaterials = resultMaterials;
		textureBakeResults.fixOutOfBoundsUVs = combiner.fixOutOfBoundsUVs;
		unpackMat2RectMap(textureBakeResults);
		MB3_MeshBakerCommon[] mb = GetComponentsInChildren<MB3_MeshBakerCommon>();
		for (int num = 0; num < mb.Length; num++)
		{
			mb[num].textureBakeResults = textureBakeResults;
		}
		if (LOG_LEVEL >= MB2_LogLevel.info)
		{
			Debug.Log("Created Atlases");
		}
		coroutineResult.isFinished = true;
		if (coroutineResult.success && onBuiltAtlasesSuccess != null)
		{
			onBuiltAtlasesSuccess();
		}
		if (!coroutineResult.success && onBuiltAtlasesFail != null)
		{
			onBuiltAtlasesFail();
		}
	}

	public MB_AtlasesAndRects[] CreateAtlases(ProgressUpdateDelegate progressInfo, bool saveAtlasesAsAssets = false, MB2_EditorMethodsInterface editorMethods = null)
	{
		MB_AtlasesAndRects[] array = null;
		try
		{
			CreateAtlasesCoroutineResult createAtlasesCoroutineResult = new CreateAtlasesCoroutineResult();
			MB3_TextureCombiner.RunCorutineWithoutPause(CreateAtlasesCoroutine(progressInfo, createAtlasesCoroutineResult, saveAtlasesAsAssets, editorMethods, 1000f), 0);
			if (createAtlasesCoroutineResult.success)
			{
				array = textureBakeResults.combinedMaterialInfo;
			}
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
		finally
		{
			if (saveAtlasesAsAssets && array != null)
			{
				foreach (MB_AtlasesAndRects mB_AtlasesAndRects in array)
				{
					if (mB_AtlasesAndRects == null || mB_AtlasesAndRects.atlases == null)
					{
						continue;
					}
					for (int j = 0; j < mB_AtlasesAndRects.atlases.Length; j++)
					{
						if (mB_AtlasesAndRects.atlases[j] != null)
						{
							if (editorMethods != null)
							{
								editorMethods.Destroy(mB_AtlasesAndRects.atlases[j]);
							}
							else
							{
								MB_Utility.Destroy(mB_AtlasesAndRects.atlases[j]);
							}
						}
					}
				}
			}
		}
		return array;
	}

	private void unpackMat2RectMap(MB2_TextureBakeResults tbr)
	{
		List<Material> list = new List<Material>();
		List<MB_MaterialAndUVRect> list2 = new List<MB_MaterialAndUVRect>();
		List<Rect> list3 = new List<Rect>();
		for (int i = 0; i < tbr.combinedMaterialInfo.Length; i++)
		{
			MB_AtlasesAndRects mB_AtlasesAndRects = tbr.combinedMaterialInfo[i];
			List<MB_MaterialAndUVRect> mat2rect_map = mB_AtlasesAndRects.mat2rect_map;
			for (int j = 0; j < mat2rect_map.Count; j++)
			{
				list2.Add(mat2rect_map[j]);
				list.Add(mat2rect_map[j].material);
				list3.Add(mat2rect_map[j].atlasRect);
			}
		}
		tbr.materialsAndUVRects = list2.ToArray();
		tbr.materials = list.ToArray();
		tbr.prefabUVRects = list3.ToArray();
	}

	public static void ConfigureNewMaterialToMatchOld(Material newMat, Material original)
	{
		if (original == null)
		{
			Debug.LogWarning(string.Concat("Original material is null, could not copy properties to ", newMat, ". Setting shader to ", newMat.shader));
			return;
		}
		newMat.shader = original.shader;
		newMat.CopyPropertiesFromMaterial(original);
		ShaderTextureProperty[] shaderTexPropertyNames = MB3_TextureCombiner.shaderTexPropertyNames;
		for (int i = 0; i < shaderTexPropertyNames.Length; i++)
		{
			Vector2 one = Vector2.one;
			Vector2 zero = Vector2.zero;
			if (newMat.HasProperty(shaderTexPropertyNames[i].name))
			{
				newMat.SetTextureOffset(shaderTexPropertyNames[i].name, zero);
				newMat.SetTextureScale(shaderTexPropertyNames[i].name, one);
			}
		}
	}

	private string PrintSet(HashSet<Material> s)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Material item in s)
		{
			stringBuilder.Append(string.Concat(item, ","));
		}
		return stringBuilder.ToString();
	}

	private bool _ValidateResultMaterials()
	{
		HashSet<Material> hashSet = new HashSet<Material>();
		for (int i = 0; i < objsToMesh.Count; i++)
		{
			if (!(objsToMesh[i] != null))
			{
				continue;
			}
			Material[] gOMaterials = MB_Utility.GetGOMaterials(objsToMesh[i]);
			for (int j = 0; j < gOMaterials.Length; j++)
			{
				if (gOMaterials[j] != null)
				{
					hashSet.Add(gOMaterials[j]);
				}
			}
		}
		HashSet<Material> hashSet2 = new HashSet<Material>();
		for (int k = 0; k < resultMaterials.Length; k++)
		{
			MB_MultiMaterial mB_MultiMaterial = resultMaterials[k];
			if (mB_MultiMaterial.combinedMaterial == null)
			{
				Debug.LogError("Combined Material is null please create and assign a result material.");
				return false;
			}
			Shader shader = mB_MultiMaterial.combinedMaterial.shader;
			for (int l = 0; l < mB_MultiMaterial.sourceMaterials.Count; l++)
			{
				if (mB_MultiMaterial.sourceMaterials[l] == null)
				{
					Debug.LogError("There are null entries in the list of Source Materials");
					return false;
				}
				if (shader != mB_MultiMaterial.sourceMaterials[l].shader)
				{
					Debug.LogWarning(string.Concat("Source material ", mB_MultiMaterial.sourceMaterials[l], " does not use shader ", shader, " it may not have the required textures. If not empty textures will be generated."));
				}
				if (hashSet2.Contains(mB_MultiMaterial.sourceMaterials[l]))
				{
					Debug.LogError(string.Concat("A Material ", mB_MultiMaterial.sourceMaterials[l], " appears more than once in the list of source materials in the source material to combined mapping. Each source material must be unique."));
					return false;
				}
				hashSet2.Add(mB_MultiMaterial.sourceMaterials[l]);
			}
		}
		if (hashSet.IsProperSubsetOf(hashSet2))
		{
			hashSet2.ExceptWith(hashSet);
			Debug.LogWarning("There are materials in the mapping that are not used on your source objects: " + PrintSet(hashSet2));
		}
		if (hashSet2.IsProperSubsetOf(hashSet))
		{
			hashSet.ExceptWith(hashSet2);
			Debug.LogError("There are materials on the objects to combine that are not in the mapping: " + PrintSet(hashSet));
			return false;
		}
		return true;
	}
}
