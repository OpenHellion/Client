using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalOpus.MB.Core
{
	[Serializable]
	public class MB3_MultiMeshCombiner : MB3_MeshCombiner
	{
		[Serializable]
		public class CombinedMesh
		{
			public MB3_MeshCombinerSingle combinedMesh;

			public int extraSpace = -1;

			public int numVertsInListToDelete;

			public int numVertsInListToAdd;

			public List<GameObject> gosToAdd;

			public List<int> gosToDelete;

			public List<GameObject> gosToUpdate;

			public bool isDirty;

			public CombinedMesh(int maxNumVertsInMesh, GameObject resultSceneObject, MB2_LogLevel ll)
			{
				combinedMesh = new MB3_MeshCombinerSingle();
				combinedMesh.resultSceneObject = resultSceneObject;
				combinedMesh.LOG_LEVEL = ll;
				extraSpace = maxNumVertsInMesh;
				numVertsInListToDelete = 0;
				numVertsInListToAdd = 0;
				gosToAdd = new List<GameObject>();
				gosToDelete = new List<int>();
				gosToUpdate = new List<GameObject>();
			}

			public bool isEmpty()
			{
				List<GameObject> list = new List<GameObject>();
				list.AddRange(combinedMesh.GetObjectsInCombined());
				for (int i = 0; i < gosToDelete.Count; i++)
				{
					for (int j = 0; j < list.Count; j++)
					{
						if (list[j].GetInstanceID() == gosToDelete[i])
						{
							list.RemoveAt(j);
							break;
						}
					}
				}
				if (list.Count == 0)
				{
					return true;
				}
				return false;
			}
		}

		private static GameObject[] empty = new GameObject[0];

		private static int[] emptyIDs = new int[0];

		public Dictionary<int, CombinedMesh> obj2MeshCombinerMap = new Dictionary<int, CombinedMesh>();

		[SerializeField]
		public List<CombinedMesh> meshCombiners = new List<CombinedMesh>();

		[SerializeField]
		private int _maxVertsInMesh = 65535;

		public override MB2_LogLevel LOG_LEVEL
		{
			get
			{
				return _LOG_LEVEL;
			}
			set
			{
				_LOG_LEVEL = value;
				for (int i = 0; i < meshCombiners.Count; i++)
				{
					meshCombiners[i].combinedMesh.LOG_LEVEL = value;
				}
			}
		}

		public override MB2_ValidationLevel validationLevel
		{
			get
			{
				return _validationLevel;
			}
			set
			{
				_validationLevel = value;
				for (int i = 0; i < meshCombiners.Count; i++)
				{
					meshCombiners[i].combinedMesh.validationLevel = _validationLevel;
				}
			}
		}

		public int maxVertsInMesh
		{
			get
			{
				return _maxVertsInMesh;
			}
			set
			{
				if (obj2MeshCombinerMap.Count <= 0)
				{
					if (value < 3)
					{
						Debug.LogError("Max verts in mesh must be greater than three.");
					}
					else if (value > 65535)
					{
						Debug.LogError("Meshes in unity cannot have more than 65535 vertices.");
					}
					else
					{
						_maxVertsInMesh = value;
					}
				}
			}
		}

		public override int GetNumObjectsInCombined()
		{
			return obj2MeshCombinerMap.Count;
		}

		public override int GetNumVerticesFor(GameObject go)
		{
			CombinedMesh value = null;
			if (obj2MeshCombinerMap.TryGetValue(go.GetInstanceID(), out value))
			{
				return value.combinedMesh.GetNumVerticesFor(go);
			}
			return -1;
		}

		public override int GetNumVerticesFor(int gameObjectID)
		{
			CombinedMesh value = null;
			if (obj2MeshCombinerMap.TryGetValue(gameObjectID, out value))
			{
				return value.combinedMesh.GetNumVerticesFor(gameObjectID);
			}
			return -1;
		}

		public override void SetObjectsToCombine(List<GameObject> objs)
		{
		}

		public override List<GameObject> GetObjectsInCombined()
		{
			List<GameObject> list = new List<GameObject>();
			for (int i = 0; i < meshCombiners.Count; i++)
			{
				list.AddRange(meshCombiners[i].combinedMesh.GetObjectsInCombined());
			}
			return list;
		}

		public override int GetLightmapIndex()
		{
			if (meshCombiners.Count > 0)
			{
				return meshCombiners[0].combinedMesh.GetLightmapIndex();
			}
			return -1;
		}

		public override bool CombinedMeshContains(GameObject go)
		{
			return obj2MeshCombinerMap.ContainsKey(go.GetInstanceID());
		}

		private bool _validateTextureBakeResults()
		{
			if (textureBakeResults == null)
			{
				Debug.LogError("Material Bake Results is null. Can't combine meshes.");
				return false;
			}
			if ((textureBakeResults.materialsAndUVRects == null || textureBakeResults.materialsAndUVRects.Length == 0) && (textureBakeResults.materials == null || textureBakeResults.materials.Length == 0))
			{
				Debug.LogError("Material Bake Results has no materials in material to sourceUVRect map. Try baking materials. Can't combine meshes.");
				return false;
			}
			if (textureBakeResults.doMultiMaterial)
			{
				if (textureBakeResults.resultMaterials == null || textureBakeResults.resultMaterials.Length == 0)
				{
					Debug.LogError("Material Bake Results has no result materials. Try baking materials. Can't combine meshes.");
					return false;
				}
			}
			else if (textureBakeResults.resultMaterial == null)
			{
				Debug.LogError("Material Bake Results has no result material. Try baking materials. Can't combine meshes.");
				return false;
			}
			return true;
		}

		public override void Apply(GenerateUV2Delegate uv2GenerationMethod)
		{
			for (int i = 0; i < meshCombiners.Count; i++)
			{
				if (meshCombiners[i].isDirty)
				{
					meshCombiners[i].combinedMesh.Apply(uv2GenerationMethod);
					meshCombiners[i].isDirty = false;
				}
			}
		}

		public override void Apply(bool triangles, bool vertices, bool normals, bool tangents, bool uvs, bool uv2, bool uv3, bool uv4, bool colors, bool bones = false, bool blendShapesFlag = false, GenerateUV2Delegate uv2GenerationMethod = null)
		{
			for (int i = 0; i < meshCombiners.Count; i++)
			{
				if (meshCombiners[i].isDirty)
				{
					meshCombiners[i].combinedMesh.Apply(triangles, vertices, normals, tangents, uvs, uv2, uv3, uv4, colors, bones, blendShapesFlag, uv2GenerationMethod);
					meshCombiners[i].isDirty = false;
				}
			}
		}

		public override void UpdateSkinnedMeshApproximateBounds()
		{
			for (int i = 0; i < meshCombiners.Count; i++)
			{
				meshCombiners[i].combinedMesh.UpdateSkinnedMeshApproximateBounds();
			}
		}

		public override void UpdateSkinnedMeshApproximateBoundsFromBones()
		{
			for (int i = 0; i < meshCombiners.Count; i++)
			{
				meshCombiners[i].combinedMesh.UpdateSkinnedMeshApproximateBoundsFromBones();
			}
		}

		public override void UpdateSkinnedMeshApproximateBoundsFromBounds()
		{
			for (int i = 0; i < meshCombiners.Count; i++)
			{
				meshCombiners[i].combinedMesh.UpdateSkinnedMeshApproximateBoundsFromBounds();
			}
		}

		public override void UpdateGameObjects(GameObject[] gos, bool recalcBounds = true, bool updateVertices = true, bool updateNormals = true, bool updateTangents = true, bool updateUV = false, bool updateUV2 = false, bool updateUV3 = false, bool updateUV4 = false, bool updateColors = false, bool updateSkinningInfo = false)
		{
			if (gos == null)
			{
				Debug.LogError("list of game objects cannot be null");
				return;
			}
			for (int i = 0; i < meshCombiners.Count; i++)
			{
				meshCombiners[i].gosToUpdate.Clear();
			}
			for (int j = 0; j < gos.Length; j++)
			{
				CombinedMesh value = null;
				obj2MeshCombinerMap.TryGetValue(gos[j].GetInstanceID(), out value);
				if (value != null)
				{
					value.gosToUpdate.Add(gos[j]);
				}
				else
				{
					Debug.LogWarning(string.Concat("Object ", gos[j], " is not in the combined mesh."));
				}
			}
			for (int k = 0; k < meshCombiners.Count; k++)
			{
				if (meshCombiners[k].gosToUpdate.Count > 0)
				{
					meshCombiners[k].isDirty = true;
					GameObject[] gos2 = meshCombiners[k].gosToUpdate.ToArray();
					meshCombiners[k].combinedMesh.UpdateGameObjects(gos2, recalcBounds, updateVertices, updateNormals, updateTangents, updateUV, updateUV2, updateUV3, updateUV4, updateColors, updateSkinningInfo);
				}
			}
		}

		public override bool AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource = true)
		{
			int[] array = null;
			if (deleteGOs != null)
			{
				array = new int[deleteGOs.Length];
				for (int i = 0; i < deleteGOs.Length; i++)
				{
					if (deleteGOs[i] == null)
					{
						Debug.LogError("The " + i + "th object on the list of objects to delete is 'Null'");
					}
					else
					{
						array[i] = deleteGOs[i].GetInstanceID();
					}
				}
			}
			return AddDeleteGameObjectsByID(gos, array, disableRendererInSource);
		}

		public override bool AddDeleteGameObjectsByID(GameObject[] gos, int[] deleteGOinstanceIDs, bool disableRendererInSource = true)
		{
			if (_usingTemporaryTextureBakeResult && gos != null && gos.Length > 0)
			{
				MB_Utility.Destroy(_textureBakeResults);
				_textureBakeResults = null;
				_usingTemporaryTextureBakeResult = false;
			}
			if (_textureBakeResults == null && gos != null && gos.Length > 0 && gos[0] != null && !_CheckIfAllObjsToAddUseSameMaterialsAndCreateTemporaryTextrueBakeResult(gos))
			{
				return false;
			}
			if (!_validate(gos, deleteGOinstanceIDs))
			{
				return false;
			}
			_distributeAmongBakers(gos, deleteGOinstanceIDs);
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug(string.Concat("MB2_MultiMeshCombiner.AddDeleteGameObjects numCombinedMeshes: ", meshCombiners.Count, " added:", gos, " deleted:", deleteGOinstanceIDs, " disableRendererInSource:", disableRendererInSource, " maxVertsPerCombined:", _maxVertsInMesh));
			}
			return _bakeStep1(gos, deleteGOinstanceIDs, disableRendererInSource);
		}

		private bool _validate(GameObject[] gos, int[] deleteGOinstanceIDs)
		{
			if (_validationLevel == MB2_ValidationLevel.none)
			{
				return true;
			}
			if (_maxVertsInMesh < 3)
			{
				Debug.LogError("Invalid value for maxVertsInMesh=" + _maxVertsInMesh);
			}
			_validateTextureBakeResults();
			if (gos != null)
			{
				for (int i = 0; i < gos.Length; i++)
				{
					if (gos[i] == null)
					{
						Debug.LogError("The " + i + "th object on the list of objects to combine is 'None'. Use Command-Delete on Mac OS X; Delete or Shift-Delete on Windows to remove this one element.");
						return false;
					}
					if (_validationLevel < MB2_ValidationLevel.robust)
					{
						continue;
					}
					for (int j = i + 1; j < gos.Length; j++)
					{
						if (gos[i] == gos[j])
						{
							Debug.LogError(string.Concat("GameObject ", gos[i], "appears twice in list of game objects to add"));
							return false;
						}
					}
					if (!obj2MeshCombinerMap.ContainsKey(gos[i].GetInstanceID()))
					{
						continue;
					}
					bool flag = false;
					if (deleteGOinstanceIDs != null)
					{
						for (int k = 0; k < deleteGOinstanceIDs.Length; k++)
						{
							if (deleteGOinstanceIDs[k] == gos[i].GetInstanceID())
							{
								flag = true;
							}
						}
					}
					if (!flag)
					{
						Debug.LogError(string.Concat("GameObject ", gos[i], " is already in the combined mesh ", gos[i].GetInstanceID()));
						return false;
					}
				}
			}
			if (deleteGOinstanceIDs != null && _validationLevel >= MB2_ValidationLevel.robust)
			{
				for (int l = 0; l < deleteGOinstanceIDs.Length; l++)
				{
					for (int m = l + 1; m < deleteGOinstanceIDs.Length; m++)
					{
						if (deleteGOinstanceIDs[l] == deleteGOinstanceIDs[m])
						{
							Debug.LogError("GameObject " + deleteGOinstanceIDs[l] + "appears twice in list of game objects to delete");
							return false;
						}
					}
					if (!obj2MeshCombinerMap.ContainsKey(deleteGOinstanceIDs[l]))
					{
						Debug.LogWarning("GameObject with instance ID " + deleteGOinstanceIDs[l] + " on the list of objects to delete is not in the combined mesh.");
					}
				}
			}
			return true;
		}

		private void _distributeAmongBakers(GameObject[] gos, int[] deleteGOinstanceIDs)
		{
			if (gos == null)
			{
				gos = empty;
			}
			if (deleteGOinstanceIDs == null)
			{
				deleteGOinstanceIDs = emptyIDs;
			}
			if (resultSceneObject == null)
			{
				resultSceneObject = new GameObject("CombinedMesh-" + base.name);
			}
			for (int i = 0; i < meshCombiners.Count; i++)
			{
				meshCombiners[i].extraSpace = _maxVertsInMesh - meshCombiners[i].combinedMesh.GetMesh().vertexCount;
			}
			for (int j = 0; j < deleteGOinstanceIDs.Length; j++)
			{
				CombinedMesh value = null;
				if (obj2MeshCombinerMap.TryGetValue(deleteGOinstanceIDs[j], out value))
				{
					if (LOG_LEVEL >= MB2_LogLevel.debug)
					{
						MB2_Log.LogDebug("MB2_MultiMeshCombiner.Removing " + deleteGOinstanceIDs[j] + " from meshCombiner " + meshCombiners.IndexOf(value));
					}
					value.numVertsInListToDelete += value.combinedMesh.GetNumVerticesFor(deleteGOinstanceIDs[j]);
					value.gosToDelete.Add(deleteGOinstanceIDs[j]);
				}
				else
				{
					Debug.LogWarning("Object " + deleteGOinstanceIDs[j] + " in the list of objects to delete is not in the combined mesh.");
				}
			}
			for (int k = 0; k < gos.Length; k++)
			{
				GameObject gameObject = gos[k];
				int vertexCount = MB_Utility.GetMesh(gameObject).vertexCount;
				CombinedMesh combinedMesh = null;
				for (int l = 0; l < meshCombiners.Count; l++)
				{
					if (meshCombiners[l].extraSpace + meshCombiners[l].numVertsInListToDelete - meshCombiners[l].numVertsInListToAdd > vertexCount)
					{
						combinedMesh = meshCombiners[l];
						if (LOG_LEVEL >= MB2_LogLevel.debug)
						{
							MB2_Log.LogDebug(string.Concat("MB2_MultiMeshCombiner.Added ", gos[k], " to combinedMesh ", l), LOG_LEVEL);
						}
						break;
					}
				}
				if (combinedMesh == null)
				{
					combinedMesh = new CombinedMesh(maxVertsInMesh, _resultSceneObject, _LOG_LEVEL);
					_setMBValues(combinedMesh.combinedMesh);
					meshCombiners.Add(combinedMesh);
					if (LOG_LEVEL >= MB2_LogLevel.debug)
					{
						MB2_Log.LogDebug("MB2_MultiMeshCombiner.Created new combinedMesh");
					}
				}
				combinedMesh.gosToAdd.Add(gameObject);
				combinedMesh.numVertsInListToAdd += vertexCount;
			}
		}

		private bool _bakeStep1(GameObject[] gos, int[] deleteGOinstanceIDs, bool disableRendererInSource)
		{
			for (int i = 0; i < meshCombiners.Count; i++)
			{
				CombinedMesh combinedMesh = meshCombiners[i];
				if (combinedMesh.combinedMesh.targetRenderer == null)
				{
					combinedMesh.combinedMesh.resultSceneObject = _resultSceneObject;
					combinedMesh.combinedMesh.BuildSceneMeshObject(gos, true);
					if (_LOG_LEVEL >= MB2_LogLevel.debug)
					{
						MB2_Log.LogDebug("BuildSO combiner {0} goID {1} targetRenID {2} meshID {3}", i, combinedMesh.combinedMesh.targetRenderer.gameObject.GetInstanceID(), combinedMesh.combinedMesh.targetRenderer.GetInstanceID(), combinedMesh.combinedMesh.GetMesh().GetInstanceID());
					}
				}
				else if (combinedMesh.combinedMesh.targetRenderer.transform.parent != resultSceneObject.transform)
				{
					Debug.LogError("targetRender objects must be children of resultSceneObject");
					return false;
				}
				if (combinedMesh.gosToAdd.Count > 0 || combinedMesh.gosToDelete.Count > 0)
				{
					combinedMesh.combinedMesh.AddDeleteGameObjectsByID(combinedMesh.gosToAdd.ToArray(), combinedMesh.gosToDelete.ToArray(), disableRendererInSource);
					if (_LOG_LEVEL >= MB2_LogLevel.debug)
					{
						MB2_Log.LogDebug("Baked combiner {0} obsAdded {1} objsRemoved {2} goID {3} targetRenID {4} meshID {5}", i, combinedMesh.gosToAdd.Count, combinedMesh.gosToDelete.Count, combinedMesh.combinedMesh.targetRenderer.gameObject.GetInstanceID(), combinedMesh.combinedMesh.targetRenderer.GetInstanceID(), combinedMesh.combinedMesh.GetMesh().GetInstanceID());
					}
				}
				Renderer renderer = combinedMesh.combinedMesh.targetRenderer;
				Mesh mesh = combinedMesh.combinedMesh.GetMesh();
				if (renderer is MeshRenderer)
				{
					MeshFilter component = renderer.gameObject.GetComponent<MeshFilter>();
					component.sharedMesh = mesh;
				}
				else
				{
					SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)renderer;
					skinnedMeshRenderer.sharedMesh = mesh;
				}
			}
			for (int j = 0; j < meshCombiners.Count; j++)
			{
				CombinedMesh combinedMesh2 = meshCombiners[j];
				for (int k = 0; k < combinedMesh2.gosToDelete.Count; k++)
				{
					obj2MeshCombinerMap.Remove(combinedMesh2.gosToDelete[k]);
				}
			}
			for (int l = 0; l < meshCombiners.Count; l++)
			{
				CombinedMesh combinedMesh3 = meshCombiners[l];
				for (int m = 0; m < combinedMesh3.gosToAdd.Count; m++)
				{
					obj2MeshCombinerMap.Add(combinedMesh3.gosToAdd[m].GetInstanceID(), combinedMesh3);
				}
				if (combinedMesh3.gosToAdd.Count > 0 || combinedMesh3.gosToDelete.Count > 0)
				{
					combinedMesh3.gosToDelete.Clear();
					combinedMesh3.gosToAdd.Clear();
					combinedMesh3.numVertsInListToDelete = 0;
					combinedMesh3.numVertsInListToAdd = 0;
					combinedMesh3.isDirty = true;
				}
			}
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				string text = "Meshes in combined:";
				for (int n = 0; n < meshCombiners.Count; n++)
				{
					string text2 = text;
					text = text2 + " mesh" + n + "(" + meshCombiners[n].combinedMesh.GetObjectsInCombined().Count + ")\n";
				}
				text = text + "children in result: " + resultSceneObject.transform.childCount;
				MB2_Log.LogDebug(text, LOG_LEVEL);
			}
			if (meshCombiners.Count > 0)
			{
				return true;
			}
			return false;
		}

		public override Dictionary<MBBlendShapeKey, MBBlendShapeValue> BuildSourceBlendShapeToCombinedIndexMap()
		{
			Dictionary<MBBlendShapeKey, MBBlendShapeValue> dictionary = new Dictionary<MBBlendShapeKey, MBBlendShapeValue>();
			for (int i = 0; i < meshCombiners.Count; i++)
			{
				for (int j = 0; j < meshCombiners[i].combinedMesh.blendShapes.Length; j++)
				{
					MB3_MeshCombinerSingle.MBBlendShape mBBlendShape = meshCombiners[i].combinedMesh.blendShapes[j];
					MBBlendShapeValue mBBlendShapeValue = new MBBlendShapeValue();
					mBBlendShapeValue.combinedMeshGameObject = meshCombiners[i].combinedMesh.targetRenderer.gameObject;
					mBBlendShapeValue.blendShapeIndex = j;
					dictionary.Add(new MBBlendShapeKey(mBBlendShape.gameObjectID, mBBlendShape.indexInSource), mBBlendShapeValue);
				}
			}
			return dictionary;
		}

		public override void ClearBuffers()
		{
			for (int i = 0; i < meshCombiners.Count; i++)
			{
				meshCombiners[i].combinedMesh.ClearBuffers();
			}
		}

		public override void ClearMesh()
		{
			DestroyMesh();
		}

		public override void DestroyMesh()
		{
			for (int i = 0; i < meshCombiners.Count; i++)
			{
				if (meshCombiners[i].combinedMesh.targetRenderer != null)
				{
					MB_Utility.Destroy(meshCombiners[i].combinedMesh.targetRenderer.gameObject);
				}
				meshCombiners[i].combinedMesh.ClearMesh();
			}
			obj2MeshCombinerMap.Clear();
			meshCombiners.Clear();
		}

		public override void DestroyMeshEditor(MB2_EditorMethodsInterface editorMethods)
		{
			for (int i = 0; i < meshCombiners.Count; i++)
			{
				if (meshCombiners[i].combinedMesh.targetRenderer != null)
				{
					editorMethods.Destroy(meshCombiners[i].combinedMesh.targetRenderer.gameObject);
				}
				meshCombiners[i].combinedMesh.ClearMesh();
			}
			obj2MeshCombinerMap.Clear();
			meshCombiners.Clear();
		}

		private void _setMBValues(MB3_MeshCombinerSingle targ)
		{
			targ.validationLevel = _validationLevel;
			targ.renderType = renderType;
			targ.outputOption = MB2_OutputOptions.bakeIntoSceneObject;
			targ.lightmapOption = lightmapOption;
			targ.textureBakeResults = textureBakeResults;
			targ.doNorm = doNorm;
			targ.doTan = doTan;
			targ.doCol = doCol;
			targ.doUV = doUV;
			targ.doUV3 = doUV3;
			targ.doUV4 = doUV4;
			targ.doBlendShapes = doBlendShapes;
			targ.uv2UnwrappingParamsHardAngle = uv2UnwrappingParamsHardAngle;
			targ.uv2UnwrappingParamsPackMargin = uv2UnwrappingParamsPackMargin;
		}

		public override void CheckIntegrity()
		{
			for (int i = 0; i < meshCombiners.Count; i++)
			{
				meshCombiners[i].combinedMesh.CheckIntegrity();
			}
		}
	}
}
