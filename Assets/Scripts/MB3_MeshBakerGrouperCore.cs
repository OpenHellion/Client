using System;
using System.Collections.Generic;
using DigitalOpus.MB.Core;
using UnityEngine;

[Serializable]
public class MB3_MeshBakerGrouperCore
{
	public enum ClusterType
	{
		none = 0,
		grid = 1,
		pie = 2
	}

	[Serializable]
	public class ClusterGrouper
	{
		public ClusterType clusterType;

		public Vector3 origin;

		public Vector3 cellSize;

		public int pieNumSegments = 4;

		public Vector3 pieAxis = Vector3.up;

		public Dictionary<string, List<Renderer>> FilterIntoGroups(List<GameObject> selection)
		{
			if (clusterType == ClusterType.none)
			{
				return FilterIntoGroupsNone(selection);
			}
			if (clusterType == ClusterType.grid)
			{
				return FilterIntoGroupsGrid(selection);
			}
			if (clusterType == ClusterType.pie)
			{
				return FilterIntoGroupsPie(selection);
			}
			return new Dictionary<string, List<Renderer>>();
		}

		public Dictionary<string, List<Renderer>> FilterIntoGroupsNone(List<GameObject> selection)
		{
			Debug.Log("Filtering into groups none");
			Dictionary<string, List<Renderer>> dictionary = new Dictionary<string, List<Renderer>>();
			List<Renderer> list = new List<Renderer>();
			for (int i = 0; i < selection.Count; i++)
			{
				list.Add(selection[i].GetComponent<Renderer>());
			}
			dictionary.Add("MeshBaker", list);
			return dictionary;
		}

		public Dictionary<string, List<Renderer>> FilterIntoGroupsGrid(List<GameObject> selection)
		{
			Dictionary<string, List<Renderer>> dictionary = new Dictionary<string, List<Renderer>>();
			if (cellSize.x <= 0f || cellSize.y <= 0f || cellSize.z <= 0f)
			{
				Debug.LogError("cellSize x,y,z must all be greater than zero.");
				return dictionary;
			}
			Debug.Log("Collecting renderers in each cell");
			foreach (GameObject item in selection)
			{
				GameObject gameObject = item;
				Renderer component = gameObject.GetComponent<Renderer>();
				if (component is MeshRenderer || component is SkinnedMeshRenderer)
				{
					Vector3 position = component.transform.position;
					position.x = Mathf.Floor((position.x - origin.x) / cellSize.x) * cellSize.x;
					position.y = Mathf.Floor((position.y - origin.y) / cellSize.y) * cellSize.y;
					position.z = Mathf.Floor((position.z - origin.z) / cellSize.z) * cellSize.z;
					List<Renderer> list = null;
					string key = position.ToString();
					if (dictionary.ContainsKey(key))
					{
						list = dictionary[key];
					}
					else
					{
						list = new List<Renderer>();
						dictionary.Add(key, list);
					}
					if (!list.Contains(component))
					{
						list.Add(component);
					}
				}
			}
			return dictionary;
		}

		public Dictionary<string, List<Renderer>> FilterIntoGroupsPie(List<GameObject> selection)
		{
			Dictionary<string, List<Renderer>> dictionary = new Dictionary<string, List<Renderer>>();
			if (pieNumSegments == 0)
			{
				Debug.LogError("pieNumSegments must be greater than zero.");
				return dictionary;
			}
			if (pieAxis.magnitude <= 1E-06f)
			{
				Debug.LogError("Pie axis must have length greater than zero.");
				return dictionary;
			}
			pieAxis.Normalize();
			Quaternion quaternion = Quaternion.FromToRotation(pieAxis, Vector3.up);
			Debug.Log("Collecting renderers in each cell");
			foreach (GameObject item in selection)
			{
				GameObject gameObject = item;
				Renderer component = gameObject.GetComponent<Renderer>();
				if (!(component is MeshRenderer) && !(component is SkinnedMeshRenderer))
				{
					continue;
				}
				Vector3 vector = component.transform.position - origin;
				vector.Normalize();
				vector = quaternion * vector;
				float num = 0f;
				if (Mathf.Abs(vector.x) < 0.0001f && Mathf.Abs(vector.z) < 0.0001f)
				{
					num = 0f;
				}
				else
				{
					num = Mathf.Atan2(vector.z, vector.x) * 57.29578f;
					if (num < 0f)
					{
						num = 360f + num;
					}
				}
				int num2 = Mathf.FloorToInt(num / 360f * (float)pieNumSegments);
				List<Renderer> list = null;
				string key = "seg_" + num2;
				if (dictionary.ContainsKey(key))
				{
					list = dictionary[key];
				}
				else
				{
					list = new List<Renderer>();
					dictionary.Add(key, list);
				}
				if (!list.Contains(component))
				{
					list.Add(component);
				}
			}
			return dictionary;
		}
	}

	public ClusterGrouper clusterGrouper;

	public bool clusterOnLMIndex;

	public void DoClustering(MB3_TextureBaker tb)
	{
		if (clusterGrouper == null)
		{
			Debug.LogError("Cluster Grouper was null.");
			return;
		}
		Dictionary<string, List<Renderer>> dictionary = clusterGrouper.FilterIntoGroups(tb.GetObjectsToCombine());
		Debug.Log("Found " + dictionary.Count + " cells with Renderers. Creating bakers.");
		if (clusterOnLMIndex)
		{
			Dictionary<string, List<Renderer>> dictionary2 = new Dictionary<string, List<Renderer>>();
			foreach (string key2 in dictionary.Keys)
			{
				List<Renderer> gaws = dictionary[key2];
				Dictionary<int, List<Renderer>> dictionary3 = GroupByLightmapIndex(gaws);
				foreach (int key3 in dictionary3.Keys)
				{
					string key = key2 + "-LM-" + key3;
					dictionary2.Add(key, dictionary3[key3]);
				}
			}
			dictionary = dictionary2;
		}
		foreach (string key4 in dictionary.Keys)
		{
			List<Renderer> gaws2 = dictionary[key4];
			AddMeshBaker(tb, key4, gaws2);
		}
	}

	private Dictionary<int, List<Renderer>> GroupByLightmapIndex(List<Renderer> gaws)
	{
		Dictionary<int, List<Renderer>> dictionary = new Dictionary<int, List<Renderer>>();
		for (int i = 0; i < gaws.Count; i++)
		{
			List<Renderer> list = null;
			if (dictionary.ContainsKey(gaws[i].lightmapIndex))
			{
				list = dictionary[gaws[i].lightmapIndex];
			}
			else
			{
				list = new List<Renderer>();
				dictionary.Add(gaws[i].lightmapIndex, list);
			}
			list.Add(gaws[i]);
		}
		return dictionary;
	}

	private void AddMeshBaker(MB3_TextureBaker tb, string key, List<Renderer> gaws)
	{
		int num = 0;
		for (int i = 0; i < gaws.Count; i++)
		{
			Mesh mesh = MB_Utility.GetMesh(gaws[i].gameObject);
			if (mesh != null)
			{
				num += mesh.vertexCount;
			}
		}
		GameObject gameObject = new GameObject("MeshBaker-" + key);
		gameObject.transform.position = Vector3.zero;
		MB3_MeshBakerCommon mB3_MeshBakerCommon;
		if (num >= 65535)
		{
			mB3_MeshBakerCommon = gameObject.AddComponent<MB3_MultiMeshBaker>();
			mB3_MeshBakerCommon.useObjsToMeshFromTexBaker = false;
		}
		else
		{
			mB3_MeshBakerCommon = gameObject.AddComponent<MB3_MeshBaker>();
			mB3_MeshBakerCommon.useObjsToMeshFromTexBaker = false;
		}
		mB3_MeshBakerCommon.textureBakeResults = tb.textureBakeResults;
		mB3_MeshBakerCommon.transform.parent = tb.transform;
		for (int j = 0; j < gaws.Count; j++)
		{
			mB3_MeshBakerCommon.GetObjectsToCombine().Add(gaws[j].gameObject);
		}
	}
}
