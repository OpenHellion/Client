using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HxOctreeNode<T>
{
	[Serializable]
	public class NodeObject
	{
		public HxOctreeNode<T> Node;

		public T Value;

		public Vector3 BoundsMin;

		public Vector3 BoundsMax;

		public Vector3 Center;

		public NodeObject(T value, Vector3 boundsMin, Vector3 boundsMax)
		{
			Value = value;
			BoundsMin = boundsMin;
			BoundsMax = boundsMax;
			Center = (BoundsMax + BoundsMin) / 2f;
		}
	}

	public HxOctreeNode<T> Parent;

	private float MinSize;

	private float Overlap;

	private float SizeWithOverlap;

	public Vector3 BoundsMin;

	public Vector3 BoundsMax;

	private readonly List<NodeObject> Objects = new List<NodeObject>();

	private const int MaxObjectCount = 8;

	public HxOctreeNode<T>[] Children;

	private Vector3[] ChildrenBoundsMin;

	private Vector3[] ChildrenBoundsMax;

	public int ID;

	private static int _idCtr;

	public Vector3 Origin { get; private set; }

	public float Size { get; private set; }

	public HxOctreeNode(float size, float overlap, float minSize, Vector3 origin, HxOctreeNode<T> parent)
	{
		ID = _idCtr++;
		Init(size, overlap, minSize, origin, parent);
	}

	private void Init(float size, float overlap, float minSize, Vector3 origin, HxOctreeNode<T> parent)
	{
		Parent = parent;
		Size = size;
		MinSize = minSize;
		Overlap = overlap;
		Origin = origin;
		SizeWithOverlap = (1f + Overlap) * Size;
		Vector3 vector = new Vector3(SizeWithOverlap, SizeWithOverlap, SizeWithOverlap) / 2f;
		BoundsMin = Origin - vector;
		BoundsMax = Origin + vector;
		Vector3 vector2 = Vector3.one * (Size / 2f) * (1f + Overlap) / 2f;
		float num = Size / 4f;
		ChildrenBoundsMin = new Vector3[8];
		ChildrenBoundsMax = new Vector3[8];
		Vector3 vector3 = Origin + new Vector3(-1f, 1f, -1f) * num;
		ChildrenBoundsMin[0] = vector3 - vector2;
		ChildrenBoundsMax[0] = vector3 + vector2;
		vector3 = Origin + new Vector3(1f, 1f, -1f) * num;
		ChildrenBoundsMin[1] = vector3 - vector2;
		ChildrenBoundsMax[1] = vector3 + vector2;
		vector3 = Origin + new Vector3(-1f, 1f, 1f) * num;
		ChildrenBoundsMin[2] = vector3 - vector2;
		ChildrenBoundsMax[2] = vector3 + vector2;
		vector3 = Origin + new Vector3(1f, 1f, 1f) * num;
		ChildrenBoundsMin[3] = vector3 - vector2;
		ChildrenBoundsMax[3] = vector3 + vector2;
		vector3 = Origin + new Vector3(-1f, -1f, -1f) * num;
		ChildrenBoundsMin[4] = vector3 - vector2;
		ChildrenBoundsMax[4] = vector3 + vector2;
		vector3 = Origin + new Vector3(1f, -1f, -1f) * num;
		ChildrenBoundsMin[5] = vector3 - vector2;
		ChildrenBoundsMax[5] = vector3 + vector2;
		vector3 = Origin + new Vector3(-1f, -1f, 1f) * num;
		ChildrenBoundsMin[6] = vector3 - vector2;
		ChildrenBoundsMax[6] = vector3 + vector2;
		vector3 = Origin + new Vector3(1f, -1f, 1f) * num;
		ChildrenBoundsMin[7] = vector3 - vector2;
		ChildrenBoundsMax[7] = vector3 + vector2;
	}

	public void Add(NodeObject node)
	{
		if (Objects.Count < 8 || Size < MinSize * 2f)
		{
			node.Node = this;
			Objects.Add(node);
			return;
		}
		int num3;
		if (Children == null)
		{
			float size = Size / 2f;
			float num = Size / 4f;
			Children = new HxOctreeNode<T>[8];
			Children[0] = new HxOctreeNode<T>(size, Overlap, MinSize, Origin + new Vector3(-1f, 1f, -1f) * num, this);
			Children[1] = new HxOctreeNode<T>(size, Overlap, MinSize, Origin + new Vector3(1f, 1f, -1f) * num, this);
			Children[2] = new HxOctreeNode<T>(size, Overlap, MinSize, Origin + new Vector3(-1f, 1f, 1f) * num, this);
			Children[3] = new HxOctreeNode<T>(size, Overlap, MinSize, Origin + new Vector3(1f, 1f, 1f) * num, this);
			Children[4] = new HxOctreeNode<T>(size, Overlap, MinSize, Origin + new Vector3(-1f, -1f, -1f) * num, this);
			Children[5] = new HxOctreeNode<T>(size, Overlap, MinSize, Origin + new Vector3(1f, -1f, -1f) * num, this);
			Children[6] = new HxOctreeNode<T>(size, Overlap, MinSize, Origin + new Vector3(-1f, -1f, 1f) * num, this);
			Children[7] = new HxOctreeNode<T>(size, Overlap, MinSize, Origin + new Vector3(1f, -1f, 1f) * num, this);
			for (int num2 = Objects.Count - 1; num2 >= 0; num2--)
			{
				NodeObject nodeObject = Objects[num2];
				num3 = OctantIndex(nodeObject.Center);
				if (BoundsContains(Children[num3].BoundsMin, Children[num3].BoundsMax, nodeObject.BoundsMin, nodeObject.BoundsMax))
				{
					Children[num3].Add(nodeObject);
					Objects.Remove(nodeObject);
				}
			}
		}
		num3 = OctantIndex(node.Center);
		if (BoundsContains(Children[num3].BoundsMin, Children[num3].BoundsMax, node.BoundsMin, node.BoundsMax))
		{
			Children[num3].Add(node);
			return;
		}
		node.Node = this;
		Objects.Add(node);
	}

	public bool Remove(T value)
	{
		bool flag = false;
		for (int i = 0; i < Objects.Count; i++)
		{
			if (Objects[i].Value.Equals(value))
			{
				flag = Objects.Remove(Objects[i]);
				break;
			}
		}
		if (!flag && Children != null)
		{
			for (int j = 0; j < 8; j++)
			{
				if (Children[j].Remove(value))
				{
					flag = true;
					break;
				}
			}
		}
		if (flag && Children != null)
		{
			int num = Objects.Count;
			if (Children != null)
			{
				for (int k = 0; k < 8; k++)
				{
					HxOctreeNode<T> hxOctreeNode = Children[k];
					if (hxOctreeNode.Children != null)
					{
						return flag;
					}
					num += hxOctreeNode.Objects.Count;
				}
			}
			if (num <= 8)
			{
				for (int l = 0; l < 8; l++)
				{
					List<NodeObject> objects = Children[l].Objects;
					for (int m = 0; m < objects.Count; m++)
					{
						objects[m].Node = this;
						Objects.Add(objects[m]);
					}
				}
				Children = null;
			}
		}
		return flag;
	}

	public void GetObjects(Vector3 boundsMin, Vector3 boundsMax, List<T> items)
	{
		if (!BoundsIntersects(boundsMin, boundsMax, BoundsMin, BoundsMax))
		{
			return;
		}
		for (int i = 0; i < Objects.Count; i++)
		{
			if (BoundsIntersects(boundsMin, boundsMax, Objects[i].BoundsMin, Objects[i].BoundsMax))
			{
				items.Add(Objects[i].Value);
			}
		}
		if (Children != null)
		{
			for (int j = 0; j < 8; j++)
			{
				Children[j].GetObjects(boundsMin, boundsMax, items);
			}
		}
	}

	public void GetObjects2(Vector3 boundsMin, Vector3 boundsMax, List<T> items)
	{
		if (!BoundsIntersects(boundsMin, boundsMax, BoundsMin, BoundsMax))
		{
			return;
		}
		if (BoundsContains(boundsMin, boundsMax, BoundsMin, BoundsMax))
		{
			addAllObjectsToList(items);
			return;
		}
		for (int i = 0; i < Objects.Count; i++)
		{
			if (BoundsIntersects(Objects[i].BoundsMin, Objects[i].BoundsMax, boundsMin, boundsMax))
			{
				items.Add(Objects[i].Value);
			}
		}
		if (Children != null)
		{
			for (int j = 0; j < 8; j++)
			{
				Children[j].GetObjects2(boundsMin, boundsMax, items);
			}
		}
	}

	public void GetObjects2BoundsPlane(ref Plane[] planes, Vector3 boundsMin, Vector3 boundsMax, List<T> items)
	{
		if (!BoundsIntersects(boundsMin, boundsMax, BoundsMin, BoundsMax))
		{
			return;
		}
		if (BoundsContains(boundsMin, boundsMax, BoundsMin, BoundsMax) && BoundsInPlanes(boundsMin, boundsMax, ref planes) == 2)
		{
			addAllObjectsToList(items);
			return;
		}
		for (int i = 0; i < Objects.Count; i++)
		{
			if (BoundsIntersects(Objects[i].BoundsMin, Objects[i].BoundsMax, boundsMin, boundsMax) && BoundsInPlanes(Objects[i].BoundsMin, Objects[i].BoundsMax, ref planes) >= 1)
			{
				items.Add(Objects[i].Value);
			}
		}
		if (Children != null)
		{
			for (int j = 0; j < 8; j++)
			{
				Children[j].GetObjects2BoundsPlane(ref planes, boundsMin, boundsMax, items);
			}
		}
	}

	private void DrawBounds(Vector3 min, Vector3 max)
	{
		Debug.DrawLine(min, new Vector3(min.x, min.y, max.z), Color.red);
		Debug.DrawLine(min, new Vector3(min.x, max.y, min.z), Color.red);
		Debug.DrawLine(min, new Vector3(max.x, min.y, min.z), Color.red);
	}

	private int BoundsInPlanes(Vector3 min, Vector3 max, ref Plane[] planes)
	{
		int result = 2;
		for (int i = 0; i < planes.Length; i++)
		{
			if (planes[i].GetDistanceToPoint(GetVertexP(min, max, planes[i].normal)) < 0f)
			{
				return 0;
			}
			if (planes[i].GetDistanceToPoint(GetVertexN(min, max, planes[i].normal)) < 0f)
			{
				result = 1;
			}
		}
		return result;
	}

	private bool ObjectInPlanes(Vector3 min, Vector3 max, ref Plane[] planes)
	{
		for (int i = 0; i < planes.Length; i++)
		{
			if (!planes[i].GetSide(GetVertexP(min, max, planes[i].normal)))
			{
				return false;
			}
		}
		return true;
	}

	private Vector3 GetVertexP(Vector3 min, Vector3 max, Vector3 normal)
	{
		if (normal.x > 0f)
		{
			min.x = max.x;
		}
		if (normal.y > 0f)
		{
			min.y = max.y;
		}
		if (normal.z > 0f)
		{
			min.z = max.z;
		}
		return min;
	}

	private Vector3 GetVertexN(Vector3 min, Vector3 max, Vector3 normal)
	{
		if (normal.x > 0f)
		{
			max.x = min.x;
		}
		if (normal.y > 0f)
		{
			max.y = min.y;
		}
		if (normal.z > 0f)
		{
			max.z = min.z;
		}
		return max;
	}

	private void addAllObjectsToList(List<T> items)
	{
		for (int i = 0; i < Objects.Count; i++)
		{
			items.Add(Objects[i].Value);
		}
		if (Children != null)
		{
			for (int j = 0; j < 8; j++)
			{
				Children[j].addAllObjectsToList(items);
			}
		}
	}

	private void addAllObjectsToList(List<T> items, ref Vector3 min, ref Vector3 max)
	{
		for (int i = 0; i < Objects.Count; i++)
		{
			items.Add(Objects[i].Value);
			min = new Vector3(Mathf.Min(min.x, Objects[i].BoundsMin.x), Mathf.Min(min.y, Objects[i].BoundsMin.y), Mathf.Min(min.z, Objects[i].BoundsMin.z));
			max = new Vector3(Mathf.Max(max.x, Objects[i].BoundsMax.x), Mathf.Max(max.y, Objects[i].BoundsMax.y), Mathf.Max(max.z, Objects[i].BoundsMax.z));
		}
		if (Children != null)
		{
			for (int j = 0; j < 8; j++)
			{
				Children[j].addAllObjectsToList(items, ref min, ref max);
			}
		}
	}

	public HxOctreeNode<T> TryShrink(float minSize)
	{
		if (Size < 2f * minSize)
		{
			return this;
		}
		if (Objects.Count == 0 && (Children == null || Children.Length == 0))
		{
			return this;
		}
		int num = -1;
		for (int i = 0; i < Objects.Count; i++)
		{
			NodeObject nodeObject = Objects[i];
			int num2 = OctantIndex(nodeObject.Center);
			if (i == 0 || num2 == num)
			{
				if (BoundsContains(ChildrenBoundsMin[num2], ChildrenBoundsMax[num2], nodeObject.BoundsMin, nodeObject.BoundsMax))
				{
					if (num < 0)
					{
						num = num2;
					}
					continue;
				}
				return this;
			}
			return this;
		}
		if (Children != null)
		{
			bool flag = false;
			for (int j = 0; j < Children.Length; j++)
			{
				if (Children[j].HasObjects())
				{
					if (flag)
					{
						return this;
					}
					if (num >= 0 && num != j)
					{
						return this;
					}
					flag = true;
					num = j;
				}
			}
		}
		if (Children == null)
		{
			Init(Size / 2f, Overlap, MinSize, (ChildrenBoundsMin[num] + ChildrenBoundsMax[num]) / 2f, Parent);
			return this;
		}
		for (int k = 0; k < Objects.Count; k++)
		{
			NodeObject node = Objects[k];
			Children[num].Add(node);
		}
		if (num < 0)
		{
			return this;
		}
		Children[num].Parent = Parent;
		return Children[num];
	}

	private Vector3 GetVertexP(Vector3 normal)
	{
		return Vector3.zero;
	}

	private bool HasObjects()
	{
		if (Objects.Count > 0)
		{
			return true;
		}
		if (Children != null)
		{
			for (int i = 0; i < 8; i++)
			{
				if (Children[i].HasObjects())
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool BoundsIntersects(Vector3 aMin, Vector3 aMax, Vector3 bMin, Vector3 bMax)
	{
		return aMax.x >= bMin.x && aMax.y >= bMin.y && aMax.z >= bMin.z && bMax.x >= aMin.x && bMax.y >= aMin.y && bMax.z >= aMin.z;
	}

	public static bool BoundsContains(Vector3 outerMin, Vector3 outerMax, Vector3 innerMin, Vector3 innerMax)
	{
		if (outerMin.x <= innerMin.x && outerMin.y <= innerMin.y && outerMin.z <= innerMin.z)
		{
			return outerMax.x >= innerMax.x && outerMax.y >= innerMax.y && outerMax.z >= innerMax.z;
		}
		return false;
	}

	private int OctantIndex(Vector3 point)
	{
		return ((!(point.x <= Origin.x)) ? 1 : 0) + ((!(point.z <= Origin.z)) ? 2 : 0) + ((!(point.y >= Origin.y)) ? 4 : 0);
	}

	public void Draw(int counter = 0)
	{
		Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.25f);
		for (int i = 0; i < Objects.Count; i++)
		{
			Vector3 center = (Objects[i].BoundsMax + Objects[i].BoundsMin) / 2f;
			Vector3 size = Objects[i].BoundsMax - Objects[i].BoundsMin;
			Gizmos.DrawCube(center, size);
		}
		Gizmos.color = new Color((float)counter / 5f, 1f, (float)counter / 5f);
		Gizmos.DrawWireCube(Origin, Vector3.one * SizeWithOverlap);
		if (Children != null)
		{
			counter++;
			for (int j = 0; j < 8; j++)
			{
				Children[j].Draw(counter);
			}
		}
	}
}
