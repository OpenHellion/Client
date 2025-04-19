using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using OpenHellion;
using OpenHellion.IO;
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

			public long Guid;

			public GameObject RootObject;
		}

		private readonly List<SceneReference> _loadedSceneReferences = new List<SceneReference>();

		private readonly Dictionary<long, string> _structureScenes = new Dictionary<long, string>();

		private readonly Dictionary<long, string> _celestialScenes = new Dictionary<long, string>();

		public bool IsPreloading { get; private set; }

		private Transform _geometryCacheTransform;

		private readonly List<long> _scenesCurrentlyLoading = new List<long>();

		public Action OnStartPreload { get; set; }
		public Action OnEndPreload { get; set; }
		public Action<float> OnPreloadUpdate { get; set; }

		private void Awake()
		{
			IsPreloading = true;

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
				OnEndPreload?.Invoke();
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
				OnStartPreload?.Invoke();
				IsPreloading = true;

				// Do the loading.
				PreLoadScenes().Forget();
				Debug.Log("Started preloading...");
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
					Guid = structureScene.GUID,
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
					Guid = asteroidScene.GUID,
					Type = SceneType.CelestialBody,
					RootObject = asteroidScene.gameObject
				});
			}
		}

		/// <summary>
		/// 	Handles the loading of objects to pool.
		/// </summary>
		private async UniTaskVoid PreLoadScenes()
		{
			float totalNumberOfScenes = _structureScenes.Count + _celestialScenes.Count;
			float currentSceneNumber = 0f;

			// Load _celestialScenes.
			foreach (KeyValuePair<long, string> celestial in _celestialScenes)
			{
				currentSceneNumber += 1f;
				OnPreloadUpdate?.Invoke(currentSceneNumber / totalNumberOfScenes);
				await LoadSceneAsync(SceneType.CelestialBody, celestial.Key);
			}

			// Load _structureScenes.
			foreach (KeyValuePair<long, string> structureScene in _structureScenes)
			{
				currentSceneNumber += 1f;
				OnPreloadUpdate?.Invoke(currentSceneNumber / totalNumberOfScenes);
				await LoadSceneAsync(SceneType.Structure, structureScene.Key);
			}

			OnEndPreload?.Invoke();
			IsPreloading = false;
			Debug.Log("Done preloading.");
		}

		public void LoadScenesWithIDs(List<GameScenes.SceneId> sceneIDs)
		{
			if (sceneIDs == null || sceneIDs.Count == 0)
			{
				return;
			}

			foreach (GameScenes.SceneId sceneID in sceneIDs)
			{
				KeyValuePair<long, string> scene =
					_structureScenes.FirstOrDefault((KeyValuePair<long, string> m) => m.Key == (long)sceneID);
				if (scene.Key != 0)
				{
					LoadSceneAsync(SceneType.Structure, scene.Key).Forget();
					continue;
				}

				scene = _celestialScenes.FirstOrDefault((KeyValuePair<long, string> m) => m.Key == (long)sceneID);
				if (scene.Key != 0)
				{
					LoadSceneAsync(SceneType.CelestialBody, scene.Key).Forget();
				}
			}
		}

		public GameObject GetLoadedScene(SceneType type, GameScenes.SceneId sceneId)
		{
			SceneReference sceneItem = _loadedSceneReferences.Find((SceneReference m) => m.Guid == (long) sceneId && m.Type == type);
			if (sceneItem == null)
			{
				Debug.LogErrorFormat("Cannot find loaded scene of scene id {0}", sceneId);
				return null;
			}
			_loadedSceneReferences.Remove(sceneItem);

			if (InitialisingSceneManager.SceneLoadType == InitialisingSceneManager.SceneLoadTypeValue.PreloadWithCopy)
			{
				return Instantiate(sceneItem.RootObject);
			}

			return sceneItem.RootObject;
		}

		public async UniTask LoadSceneAsync(SceneType type, long sceneId)
		{
			if (InitialisingSceneManager.SceneLoadType == InitialisingSceneManager.SceneLoadTypeValue.PreloadWithCopy &&
			    _loadedSceneReferences.Any((SceneReference m) => m.Guid == sceneId && m.Type == type))
			{
				return;
			}

			string sceneName = type != 0 ? _celestialScenes[sceneId] : _structureScenes[sceneId];
			if (sceneName.IsNullOrEmpty())
			{
				Debug.LogError("Stopped loading scene because it is not defined.");
				return;
			}

			if (_scenesCurrentlyLoading.Contains(sceneId))
			{
				await UniTask.WaitWhile(() => _scenesCurrentlyLoading.Contains(sceneId));
			}

			_scenesCurrentlyLoading.Add(sceneId);
			await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
			UnityEngine.SceneManagement.Scene scene = SceneManager.GetSceneByName(sceneName);

			if (type == SceneType.Structure)
			{
				GameObject[] rootGameObjects = scene.GetRootGameObjects();
				foreach (GameObject rootObject in rootGameObjects)
				{
					var found = rootObject.TryGetComponent(out AsteroidScene sceneScript);
					if (found)
					{
						_loadedSceneReferences.Add(new SceneReference
						{
							Type = SceneType.Structure,
							Guid = sceneScript.GUID,
							RootObject = rootObject
						});
						rootObject.transform.SetParent(_geometryCacheTransform.transform);
						await SceneManager.UnloadSceneAsync(scene);
						break;
					}
				}
			}
			else if (type == SceneType.CelestialBody)
			{
				GameObject[] rootGameObjects = scene.GetRootGameObjects();
				foreach (GameObject rootObject in rootGameObjects)
				{
					var found = rootObject.TryGetComponent(out AsteroidScene sceneScript);
					if (found)
					{
						_loadedSceneReferences.Add(new SceneReference
						{
							Type = SceneType.CelestialBody,
							Guid = sceneScript.GUID,
							RootObject = sceneScript.gameObject
						});
						sceneScript.transform.SetParent(_geometryCacheTransform.transform);
						await SceneManager.UnloadSceneAsync(scene);
						break;
					}
				}
			}

			_scenesCurrentlyLoading.Remove(sceneId);
		}
	}
}
