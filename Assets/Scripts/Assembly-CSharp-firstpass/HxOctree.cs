using System.Collections.Generic;
using UnityEngine;

public class HxOctree<T>
{
	private HxOctreeNode<T> Root;

	private float Overlap;

	private float InitialSize;

	private float MinNodeSize;

	private Dictionary<T, HxOctreeNode<T>.NodeObject> NodeMap;

	public int Count { get; private set; }

	public HxOctree(Vector3 origin = default(Vector3), float initialSize = 10f, float overlap = 0f, float minNodeSize = 1f)
	{
		Count = 0;
		InitialSize = Mathf.Max(minNodeSize, initialSize);
		MinNodeSize = Mathf.Min(minNodeSize, initialSize);
		Overlap = Mathf.Clamp(overlap, 0f, 1f);
		Root = new HxOctreeNode<T>(InitialSize, overlap, MinNodeSize, origin, null);
		NodeMap = new Dictionary<T, HxOctreeNode<T>.NodeObject>();
	}

	public HxOctreeNode<T>.NodeObject Add(T value, Vector3 boundsMin, Vector3 boundsMax)
	{
		int num = 0;
		while (!HxOctreeNode<T>.BoundsContains(Root.BoundsMin, Root.BoundsMax, boundsMin, boundsMax))
		{
			ExpandRoot((boundsMin + boundsMax) / 2f);
			if (++num > 16)
			{
				Debug.LogError("The octree could not contain the bounds.");
				return null;
			}
		}
		HxOctreeNode<T>.NodeObject nodeObject = new HxOctreeNode<T>.NodeObject(value, boundsMin, boundsMax);
		NodeMap[value] = nodeObject;
		Root.Add(nodeObject);
		Count++;
		return nodeObject;
	}

	public void Print()
	{
		Debug.Log("=============================");
		string empty = string.Empty;
		foreach (KeyValuePair<T, HxOctreeNode<T>.NodeObject> item in NodeMap)
		{
			empty = ((item.Value.Node.Children != null) ? "branch" : "leaf");
			Debug.Log(string.Concat(item.Key, " is in ", item.Value.Node.ID, ", a ", empty, "."));
		}
	}

	public void Move(HxOctreeNode<T>.NodeObject value, Vector3 boundsMin, Vector3 boundsMax)
	{
		if (value == null)
		{
			Debug.Log("null");
		}
		value.BoundsMin = boundsMin;
		value.BoundsMax = boundsMax;
		HxOctreeNode<T> hxOctreeNode = value.Node;
		if (HxOctreeNode<T>.BoundsContains(hxOctreeNode.BoundsMin, hxOctreeNode.BoundsMax, boundsMin, boundsMax))
		{
			return;
		}
		hxOctreeNode.Remove(value.Value);
		int num = 0;
		while (!HxOctreeNode<T>.BoundsContains(hxOctreeNode.BoundsMin, hxOctreeNode.BoundsMax, boundsMin, boundsMax))
		{
			if (hxOctreeNode.Parent != null)
			{
				hxOctreeNode = hxOctreeNode.Parent;
				continue;
			}
			num++;
			ExpandRoot((boundsMin + boundsMax) / 2f);
			hxOctreeNode = Root;
			if (num <= 16)
			{
				continue;
			}
			Debug.LogError("The octree could not contain the bounds.");
			return;
		}
		hxOctreeNode.Add(value);
	}

	public void Move(T value, Vector3 boundsMin, Vector3 boundsMax)
	{
		HxOctreeNode<T>.NodeObject value2;
		if (NodeMap.TryGetValue(value, out value2))
		{
			Move(value2, boundsMin, boundsMax);
		}
	}

	public void TryShrink()
	{
		Root = Root.TryShrink(InitialSize);
	}

	public bool Remove(T value)
	{
		if (Root.Remove(value))
		{
			NodeMap.Remove(value);
			Count--;
			Root = Root.TryShrink(InitialSize);
			return true;
		}
		return false;
	}

	private void ExpandRoot(Vector3 center)
	{
		Vector3 vector = Root.Origin - center;
		int num = ((!(vector.x < 0f)) ? 1 : (-1));
		int num2 = ((!(vector.y < 0f)) ? 1 : (-1));
		int num3 = ((!(vector.z < 0f)) ? 1 : (-1));
		HxOctreeNode<T> root = Root;
		float num4 = Root.Size / 2f;
		Vector3 vector2 = Root.Origin - new Vector3(num, num2, num3) * num4;
		Root = new HxOctreeNode<T>(Root.Size * 2f, Overlap, MinNodeSize, vector2, null);
		root.Parent = Root;
		int num5 = 0;
		if (num > 0)
		{
			num5++;
		}
		if (num3 > 0)
		{
			num5 += 2;
		}
		if (num2 < 0)
		{
			num5 += 4;
		}
		HxOctreeNode<T>[] array = new HxOctreeNode<T>[8];
		for (int i = 0; i < 8; i++)
		{
			if (i == num5)
			{
				array[i] = root;
				continue;
			}
			num = ((i % 2 != 0) ? 1 : (-1));
			num2 = ((i <= 3) ? 1 : (-1));
			num3 = ((i >= 2 && (i <= 3 || i >= 6)) ? 1 : (-1));
			array[i] = new HxOctreeNode<T>(root.Size, Overlap, MinNodeSize, vector2 + new Vector3(num, num2, num3) * num4, Root);
		}
		Root.Children = array;
	}

	public void GetObjects(Vector3 boundsMin, Vector3 boundsMax, List<T> items)
	{
		Root.GetObjects2(boundsMin, boundsMax, items);
	}

	public void GetObjectsBoundsPlane(ref Plane[] planes, Vector3 min, Vector3 max, List<T> items)
	{
		Root.GetObjects2BoundsPlane(ref planes, min, max, items);
	}

	public void Draw()
	{
		Root.Draw();
	}
}
