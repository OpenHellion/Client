using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Math;

namespace ZeroGravity
{
	public class SceneLoader : MonoBehaviour
	{
		public enum SceneType
		{
			Structure,
			Asteroid
		}

		private class SceneItem
		{
			public SceneType Type;

			public long GUID;

			public GameObject RootObject;
		}

		private List<SceneItem> loadedScenes = new List<SceneItem>();

		public GameObject LoadedScenesRoot;

		private Dictionary<long, string> structures = new Dictionary<long, string>();

		private Dictionary<long, string> asteroids = new Dictionary<long, string>();

		public static DateTime PreloadStartTime;

		public static bool IsPreloading = true;

		private Transform vesselsGeometryCacheTransform;

		public Image PreloadBackgorund;

		public Image PreloadFiller;

		public Text PreloadText;

		public List<Sprite> PreloadImages = new List<Sprite>();

		private List<long> loadingScenes = new List<long>();

		private void Awake()
		{
			vesselsGeometryCacheTransform = base.transform.root;
			List<StructureSceneData> list = Json.LoadResource<List<StructureSceneData>>("Data/Structures");
			if (list != null)
			{
				foreach (StructureSceneData item in list)
				{
					structures.Add(item.ItemID, item.SceneName);
				}
			}
			List<AsteroidSceneData> list2 = Json.LoadResource<List<AsteroidSceneData>>("Data/Asteroids");
			if (list2 == null)
			{
				return;
			}
			foreach (AsteroidSceneData item2 in list2)
			{
				asteroids.Add(item2.ItemID, item2.SceneName);
			}
		}

		/// <summary>
		/// 	Starts loading of objects that will be used to create the world.
		/// </summary>
		public void InitializeScenes()
		{
			if (Client.SceneLoadType != Client.SceneLoadTypeValue.PreloadWithCopy)
			{
				UnityEngine.Object.Destroy(Client.Instance.PreloadingScreen);
				IsPreloading = false;
				return;
			}
			GameObject gameObject = GameObject.Find("VesselsGeometryCache");
			if (gameObject == null)
			{
				gameObject = new GameObject("VesselsGeometryCache");
				gameObject.transform.SetParent(null);
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				vesselsGeometryCacheTransform = gameObject.transform;
				Client.Instance.PreloadingScreen.SetActive(value: true);
				PreloadStartTime = DateTime.UtcNow;
				IsPreloading = true;
				StartCoroutine(PreLoadScenesCoroutine());
				return;
			}
			vesselsGeometryCacheTransform = gameObject.transform;
			StructureScene[] componentsInChildren = vesselsGeometryCacheTransform.gameObject.GetComponentsInChildren<StructureScene>(includeInactive: true);
			foreach (StructureScene structureScene in componentsInChildren)
			{
				loadedScenes.Add(new SceneItem
				{
					GUID = structureScene.GUID,
					Type = SceneType.Structure,
					RootObject = structureScene.gameObject
				});
			}
			AsteroidScene[] componentsInChildren2 = vesselsGeometryCacheTransform.gameObject.GetComponentsInChildren<AsteroidScene>(includeInactive: true);
			foreach (AsteroidScene asteroidScene in componentsInChildren2)
			{
				loadedScenes.Add(new SceneItem
				{
					GUID = asteroidScene.GUID,
					Type = SceneType.Asteroid,
					RootObject = asteroidScene.gameObject
				});
			}
		}

		/// <summary>
		/// 	Handles the loading of objects to use later.
		/// </summary>
		private IEnumerator PreLoadScenesCoroutine()
		{
			float startTime = float.MinValue;
			float sceneNum = structures.Count + asteroids.Count;
			float currentSceneNum = 0f;
			IEnumerator<string> shuffledTexts = Localization.PreloadText.OrderBy((string m) => MathHelper.RandomNextDouble()).GetEnumerator();
			IEnumerator<Sprite> shuffledImages = PreloadImages.OrderBy((Sprite m) => MathHelper.RandomNextDouble()).ToList().GetEnumerator();

			// Load asteroids.
			foreach (KeyValuePair<long, string> ast in asteroids)
			{
				if (Time.time - startTime > 10f)
				{
					PreloadText.text = shuffledTexts.GetNextInLoop();
					PreloadBackgorund.sprite = shuffledImages.GetNextInLoop();
					startTime = Time.time;
				}
				currentSceneNum += 1f;
				PreloadFiller.fillAmount = currentSceneNum / sceneNum;
				yield return StartCoroutine(LoadSceneCoroutine(SceneType.Asteroid, ast.Key));
			}

			// Load structures.
			foreach (KeyValuePair<long, string> str in structures)
			{
				if (Time.time - startTime > 10f)
				{
					PreloadText.text = shuffledTexts.GetNextInLoop();
					PreloadBackgorund.sprite = shuffledImages.GetNextInLoop();
					startTime = Time.time;
				}
				currentSceneNum += 1f;
				PreloadFiller.fillAmount = currentSceneNum / sceneNum;
				yield return StartCoroutine(LoadSceneCoroutine(SceneType.Structure, str.Key));
			}
			UnityEngine.Object.Destroy(Client.Instance.PreloadingScreen);
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
			SceneItem sceneItem = loadedScenes.Find((SceneItem m) => m.GUID == GUID && m.Type == type);
			if (sceneItem != null)
			{
				if (Client.SceneLoadType == Client.SceneLoadTypeValue.PreloadWithCopy)
				{
					return UnityEngine.Object.Instantiate(sceneItem.RootObject);
				}
				loadedScenes.Remove(sceneItem);
				return sceneItem.RootObject;
			}
			Dbg.Error("Cannot find loaded scene", type, GUID);
			return null;
		}

		public IEnumerator LoadSceneCoroutine(SceneType type, long GUID)
		{
			if (Client.SceneLoadType == Client.SceneLoadTypeValue.PreloadWithCopy && loadedScenes.FirstOrDefault((SceneItem m) => m.GUID == GUID && m.Type == type) != null)
			{
				yield break;
			}
			string sceneName = ((type != 0) ? asteroids[GUID] : structures[GUID]);
			if (sceneName.IsNullOrEmpty())
			{
				yield break;
			}
			Client.Instance.LoadingScenesCount++;
			if (loadingScenes.Contains(GUID))
			{
				yield return new WaitWhile(() => loadingScenes.Contains(GUID));
			}
			else
			{
				loadingScenes.Add(GUID);
				AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
				yield return new WaitUntil(() => ao.isDone);
				UnityEngine.SceneManagement.Scene sc = SceneManager.GetSceneByName(sceneName);
				if (type == SceneType.Structure)
				{
					GameObject[] rootGameObjects = sc.GetRootGameObjects();
					foreach (GameObject gameObject in rootGameObjects)
					{
						StructureScene component = gameObject.GetComponent<StructureScene>();
						if (component != null)
						{
							loadedScenes.Add(new SceneItem
							{
								Type = SceneType.Structure,
								GUID = component.GUID,
								RootObject = component.gameObject
							});
							component.transform.SetParent(vesselsGeometryCacheTransform.transform);
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
							loadedScenes.Add(new SceneItem
							{
								Type = SceneType.Asteroid,
								GUID = component2.GUID,
								RootObject = component2.gameObject
							});
							component2.transform.SetParent(vesselsGeometryCacheTransform.transform);
							SceneManager.UnloadSceneAsync(sc);
							break;
						}
					}
				}
				loadingScenes.Remove(GUID);
			}
			Client.Instance.LoadingScenesCount--;
		}
	}
}
