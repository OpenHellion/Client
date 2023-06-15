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
			Asteroid
		}

		private class SceneReference
		{
			public SceneType Type;

			public long GUID;

			public GameObject RootObject;
		}

		private List<SceneReference> _LoadedSceneReferences = new List<SceneReference>();

		private Dictionary<long, string> structures = new Dictionary<long, string>();

		private Dictionary<long, string> asteroids = new Dictionary<long, string>();

		public static bool IsPreloading = true;

		private Transform _GeometryCacheTransform;

		private List<long> _LoadingScenes = new List<long>();

		[SerializeField] private StartupGUI _StartupGUI;

		public static SceneLoader Instance { get; private set; }

		private void Awake()
		{
			if (Instance is not null)
			{
				Dbg.Error("Created new scene loader when one already exists. Removing...");
				Destroy(this);
			}
			Instance = this;
			DontDestroyOnLoad(this);

			_GeometryCacheTransform = base.transform.root;
			List<StructureSceneData> structureJson = JsonSerialiser.LoadResource<List<StructureSceneData>>("Data/Structures");
			if (structureJson != null)
			{
				foreach (StructureSceneData item in structureJson)
				{
					structures.Add(item.ItemID, item.SceneName);
				}
			}
			List<AsteroidSceneData> asteroidsJson = JsonSerialiser.LoadResource<List<AsteroidSceneData>>("Data/Asteroids");
			if (asteroidsJson == null)
			{
				return;
			}
			foreach (AsteroidSceneData item2 in asteroidsJson)
			{
				asteroids.Add(item2.ItemID, item2.SceneName);
			}
		}

		/// <summary>
		/// 	Starts loading, and initializes objects for pooling.
		/// </summary>
		public void InitializeScenes()
		{
			if (StartupManager.SceneLoadType != StartupManager.SceneLoadTypeValue.PreloadWithCopy)
			{
				_StartupGUI.ClosePreloading();
				IsPreloading = false;
				return;
			}

			GameObject geometryCache = GameObject.Find("VesselsGeometryCache");

			// Create new VesselsGeometryCarche if it doesn't exist.
			if (geometryCache == null)
			{
				geometryCache = new GameObject("VesselsGeometryCache");
				geometryCache.transform.SetParent(null);
				DontDestroyOnLoad(geometryCache);
				_GeometryCacheTransform = geometryCache.transform;
				_StartupGUI.OpenPreloading();
				IsPreloading = true;

				// Do the loading.
				StartCoroutine(PreLoadScenesCoroutine());
				return;
			}
			_GeometryCacheTransform = geometryCache.transform;

			// Initialize structures.
			StructureScene[] structureCache = _GeometryCacheTransform.gameObject.GetComponentsInChildren<StructureScene>(includeInactive: true);
			foreach (StructureScene structureScene in structureCache)
			{
				_LoadedSceneReferences.Add(new SceneReference
				{
					GUID = structureScene.GUID,
					Type = SceneType.Structure,
					RootObject = structureScene.gameObject
				});
			}

			// Initialize asteroids.
			AsteroidScene[] astreoidCache = _GeometryCacheTransform.gameObject.GetComponentsInChildren<AsteroidScene>(includeInactive: true);
			foreach (AsteroidScene asteroidScene in astreoidCache)
			{
				_LoadedSceneReferences.Add(new SceneReference
				{
					GUID = asteroidScene.GUID,
					Type = SceneType.Asteroid,
					RootObject = asteroidScene.gameObject
				});
			}
		}

		/// <summary>
		/// 	Handles the loading of objects to pool.
		/// </summary>
		private IEnumerator PreLoadScenesCoroutine()
		{
			float totalNumberOfScenes = structures.Count + asteroids.Count;
			float currentSceneNumber = 0f;

			// Load asteroids.
			foreach (KeyValuePair<long, string> ast in asteroids)
			{
				currentSceneNumber += 1f;
				_StartupGUI.UpdateProgressBar(currentSceneNumber / totalNumberOfScenes);
				yield return StartCoroutine(LoadSceneCoroutine(SceneType.Asteroid, ast.Key));
			}

			// Load structures.
			foreach (KeyValuePair<long, string> str in structures)
			{
				currentSceneNumber += 1f;
				_StartupGUI.UpdateProgressBar(currentSceneNumber / totalNumberOfScenes);
				yield return StartCoroutine(LoadSceneCoroutine(SceneType.Structure, str.Key));
			}
			_StartupGUI.ClosePreloading();
			IsPreloading = false;
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
				KeyValuePair<long, string> kv2 = structures.FirstOrDefault((KeyValuePair<long, string> m) => m.Key == (long)sceneID);
				if (kv2.Key != 0)
				{
					yield return StartCoroutine(LoadSceneCoroutine(SceneType.Structure, kv2.Key));
					continue;
				}
				kv2 = asteroids.FirstOrDefault((KeyValuePair<long, string> m) => m.Key == (long)sceneID);
				if (kv2.Key != 0)
				{
					yield return StartCoroutine(LoadSceneCoroutine(SceneType.Asteroid, kv2.Key));
				}
			}
		}

		public GameObject GetLoadedScene(SceneType type, long GUID)
		{
			SceneReference sceneItem = _LoadedSceneReferences.Find((SceneReference m) => m.GUID == GUID && m.Type == type);
			if (sceneItem != null)
			{
				if (StartupManager.SceneLoadType == StartupManager.SceneLoadTypeValue.PreloadWithCopy)
				{
					return Instantiate(sceneItem.RootObject);
				}
				_LoadedSceneReferences.Remove(sceneItem);
				return sceneItem.RootObject;
			}
			Dbg.Error("Cannot find loaded scene", type, GUID);
			return null;
		}

		public IEnumerator LoadSceneCoroutine(SceneType type, long GUID)
		{
			if (StartupManager.SceneLoadType == StartupManager.SceneLoadTypeValue.PreloadWithCopy && _LoadedSceneReferences.FirstOrDefault((SceneReference m) => m.GUID == GUID && m.Type == type) != null)
			{
				yield break;
			}
			string sceneName = ((type != 0) ? asteroids[GUID] : structures[GUID]);
			if (sceneName.IsNullOrEmpty())
			{
				yield break;
			}
			Client.Instance.LoadingScenesCount++;
			if (_LoadingScenes.Contains(GUID))
			{
				yield return new WaitWhile(() => _LoadingScenes.Contains(GUID));
			}
			else
			{
				_LoadingScenes.Add(GUID);
				AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
				yield return new WaitUntil(() => asyncLoad.isDone);
				UnityEngine.SceneManagement.Scene sc = SceneManager.GetSceneByName(sceneName);
				if (type == SceneType.Structure)
				{
					GameObject[] rootGameObjects = sc.GetRootGameObjects();
					foreach (GameObject gameObject in rootGameObjects)
					{
						StructureScene component = gameObject.GetComponent<StructureScene>();
						if (component != null)
						{
							_LoadedSceneReferences.Add(new SceneReference
							{
								Type = SceneType.Structure,
								GUID = component.GUID,
								RootObject = component.gameObject
							});
							component.transform.SetParent(_GeometryCacheTransform.transform);
							SceneManager.UnloadSceneAsync(sc);
							break;
						}
					}
				}
				else
				{
					GameObject[] rootGameObjects2 = sc.GetRootGameObjects();
					foreach (GameObject gameObject2 in rootGameObjects2)
					{
						AsteroidScene component2 = gameObject2.GetComponent<AsteroidScene>();
						if (component2 != null)
						{
							_LoadedSceneReferences.Add(new SceneReference
							{
								Type = SceneType.Asteroid,
								GUID = component2.GUID,
								RootObject = component2.gameObject
							});
							component2.transform.SetParent(_GeometryCacheTransform.transform);
							SceneManager.UnloadSceneAsync(sc);
							break;
						}
					}
				}
				_LoadingScenes.Remove(GUID);
			}
			Client.Instance.LoadingScenesCount--;
		}
	}
}
