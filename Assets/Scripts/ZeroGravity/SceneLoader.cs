using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenHellion;
using OpenHellion.IO;
using OpenHellion.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;

namespace ZeroGravity
{
	public class SceneLoader : MonoBehaviour
	{
		public enum SceneType
		{
			Structure,
			CelestialBody
		}

		private class SceneReference
		{
			public SceneType Type;

			public long GUID;

			public GameObject RootObject;
		}

		private readonly List<SceneReference> _loadedSceneReferences = new List<SceneReference>();

		private readonly Dictionary<long, string> _structureScenes = new Dictionary<long, string>();

		private readonly Dictionary<long, string> _celestialScenes = new Dictionary<long, string>();

		public static bool IsPreloading { get; private set; }

		private Transform _geometryCacheTransform;

		private readonly List<long> _scenesCurrentlyLoading = new List<long>();

		// Only call this when preloading.
		[SerializeField] private StartupGUI _StartupGUI;

		private void Awake()
		{
			IsPreloading = true;

			DontDestroyOnLoad(this);

			_geometryCacheTransform = transform.root;
			List<StructureSceneData> structureJson =
				JsonSerialiser.LoadResource<List<StructureSceneData>>("Data/Structures");
			if (structureJson != null)
			{
				foreach (StructureSceneData item in structureJson)
				{
					_structureScenes.Add(item.ItemID, item.SceneName);
				}
			}

			List<CelestialSceneData> celestialSceneJson =
				JsonSerialiser.LoadResource<List<CelestialSceneData>>("Data/Asteroids");
			if (celestialSceneJson == null)
			{
				return;
			}

			foreach (CelestialSceneData sceneData in celestialSceneJson)
			{
				_celestialScenes.Add(sceneData.ItemID, sceneData.SceneName);
			}
		}

		/// <summary>
		/// 	Starts loading, and initializes objects for pooling.
		/// </summary>
		public void InitializeScenes()
		{
			if (InitialisingSceneManager.SceneLoadType != InitialisingSceneManager.SceneLoadTypeValue.PreloadWithCopy)
			{
				Dbg.Log("Skipping preloading.");
				_StartupGUI.ClosePreloading();
				IsPreloading = false;
				return;
			}

			// Start loading if we haven't already.
			GameObject geometryCache = GameObject.Find("VesselsGeometryCache");
			if (geometryCache == null)
			{
				geometryCache = new GameObject("VesselsGeometryCache");
				geometryCache.transform.SetParent(null);
				DontDestroyOnLoad(geometryCache);
				_geometryCacheTransform = geometryCache.transform;
				_StartupGUI.OpenPreloading();
				IsPreloading = true;

				// Do the loading.
				StartCoroutine(PreLoadScenesCoroutine());
				Dbg.Log("Started preloading...");
				return;
			}

			_geometryCacheTransform = geometryCache.transform;

			// Initialize structures.
			StructureScene[] structureCache =
				_geometryCacheTransform.gameObject.GetComponentsInChildren<StructureScene>(includeInactive: true);
			foreach (StructureScene structureScene in structureCache)
			{
				_loadedSceneReferences.Add(new SceneReference
				{
					GUID = structureScene.GUID,
					Type = SceneType.Structure,
					RootObject = structureScene.gameObject
				});
			}

			// Initialize celestial bodies.
			AsteroidScene[] celestialScene =
				_geometryCacheTransform.gameObject.GetComponentsInChildren<AsteroidScene>(includeInactive: true);
			foreach (AsteroidScene asteroidScene in celestialScene)
			{
				_loadedSceneReferences.Add(new SceneReference
				{
					GUID = asteroidScene.GUID,
					Type = SceneType.CelestialBody,
					RootObject = asteroidScene.gameObject
				});
			}
		}

		/// <summary>
		/// 	Handles the loading of objects to pool.
		/// </summary>
		private IEnumerator PreLoadScenesCoroutine()
		{
			float totalNumberOfScenes = _structureScenes.Count + _celestialScenes.Count;
			float currentSceneNumber = 0f;

			// Load _celestialScenes.
			foreach (KeyValuePair<long, string> celestial in _celestialScenes)
			{
				currentSceneNumber += 1f;
				_StartupGUI.UpdateProgressBar(currentSceneNumber / totalNumberOfScenes);
				yield return StartCoroutine(LoadSceneCoroutine(SceneType.CelestialBody, celestial.Key));
			}

			// Load _structureScenes.
			foreach (KeyValuePair<long, string> str in _structureScenes)
			{
				currentSceneNumber += 1f;
				_StartupGUI.UpdateProgressBar(currentSceneNumber / totalNumberOfScenes);
				yield return StartCoroutine(LoadSceneCoroutine(SceneType.Structure, str.Key));
			}

			_StartupGUI.ClosePreloading();
			IsPreloading = false;
			Dbg.Log("Done preloading.");
		}

		public void LoadScenesWithIDs(List<GameScenes.SceneID> sceneIDs)
		{
			StartCoroutine(PreLoadScenesWithIDsCoroutine(sceneIDs));
		}

		private IEnumerator PreLoadScenesWithIDsCoroutine(List<GameScenes.SceneID> sceneIDs)
		{
			if (sceneIDs == null || sceneIDs.Count == 0)
			{
				yield break;
			}

			foreach (GameScenes.SceneID sceneID in sceneIDs)
			{
				KeyValuePair<long, string> kv2 =
					_structureScenes.FirstOrDefault((KeyValuePair<long, string> m) => m.Key == (long)sceneID);
				if (kv2.Key != 0)
				{
					yield return StartCoroutine(LoadSceneCoroutine(SceneType.Structure, kv2.Key));
					continue;
				}

				kv2 = _celestialScenes.FirstOrDefault((KeyValuePair<long, string> m) => m.Key == (long)sceneID);
				if (kv2.Key != 0)
				{
					yield return StartCoroutine(LoadSceneCoroutine(SceneType.CelestialBody, kv2.Key));
				}
			}
		}

		public GameObject GetLoadedScene(SceneType type, long guid)
		{
			SceneReference sceneItem =
				_loadedSceneReferences.Find((SceneReference m) => m.GUID == guid && m.Type == type);
			if (sceneItem != null)
			{
				if (InitialisingSceneManager.SceneLoadType ==
				    InitialisingSceneManager.SceneLoadTypeValue.PreloadWithCopy)
				{
					return Instantiate(sceneItem.RootObject);
				}

				_loadedSceneReferences.Remove(sceneItem);
				return sceneItem.RootObject;
			}

			Dbg.Error("Cannot find loaded scene of type", type, "with guid", guid);
			return null;
		}

		public IEnumerator LoadSceneCoroutine(SceneType type, long guid)
		{
			if (InitialisingSceneManager.SceneLoadType == InitialisingSceneManager.SceneLoadTypeValue.PreloadWithCopy &&
			    _loadedSceneReferences.FirstOrDefault((SceneReference m) => m.GUID == guid && m.Type == type) != null)
			{
				yield break;
			}

			string sceneName = ((type != 0) ? _celestialScenes[guid] : _structureScenes[guid]);
			if (sceneName.IsNullOrEmpty())
			{
				yield break;
			}

			if (_scenesCurrentlyLoading.Contains(guid))
			{
				yield return new WaitWhile(() => _scenesCurrentlyLoading.Contains(guid));
			}
			else
			{
				_scenesCurrentlyLoading.Add(guid);
				AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
				yield return new WaitUntil(() => asyncLoad.isDone);
				UnityEngine.SceneManagement.Scene scene = SceneManager.GetSceneByName(sceneName);

				if (type == SceneType.Structure)
				{
					GameObject[] rootGameObjects = scene.GetRootGameObjects();
					foreach (GameObject rootObject in rootGameObjects)
					{
						StructureScene sceneScript = rootObject.GetComponent<StructureScene>();
						if (sceneScript is not null)
						{
							_loadedSceneReferences.Add(new SceneReference
							{
								Type = SceneType.Structure,
								GUID = sceneScript.GUID,
								RootObject = sceneScript.gameObject
							});
							sceneScript.transform.SetParent(_geometryCacheTransform.transform);
							SceneManager.UnloadSceneAsync(scene);
							break;
						}
					}
				}
				else if (type == SceneType.CelestialBody)
				{
					GameObject[] rootGameObjects = scene.GetRootGameObjects();
					foreach (GameObject rootObject in rootGameObjects)
					{
						AsteroidScene sceneScript = rootObject.GetComponent<AsteroidScene>();
						if (sceneScript is not null)
						{
							_loadedSceneReferences.Add(new SceneReference
							{
								Type = SceneType.CelestialBody,
								GUID = sceneScript.GUID,
								RootObject = sceneScript.gameObject
							});
							sceneScript.transform.SetParent(_geometryCacheTransform.transform);
							SceneManager.UnloadSceneAsync(scene);
							break;
						}
					}
				}

				_scenesCurrentlyLoading.Remove(guid);
			}
		}
	}
}
