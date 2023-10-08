using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.ShipComponents;

namespace ZeroGravity
{
	public class ZeroOccluder : MonoBehaviour
	{
		public enum Type
		{
			Distance = 1,
			PlayerInside = 2,
			PlayerOutside = 3
		}

		[Serializable]
		private class WholeObject
		{
			public GameObject Obj;

			public Transform Parent;
		}

		[SerializeField] private Type occluderType;

		[SerializeField] private float occlusionDistance;

		[SerializeField] private List<GameObject> occlusionObjects;

		[SerializeField] [HideInInspector] private List<MeshRenderer> renderers = new List<MeshRenderer>();

		[SerializeField] [HideInInspector] private List<Light> lights = new List<Light>();

		[SerializeField] [HideInInspector] private List<WholeObject> objects = new List<WholeObject>();

		[SerializeField] [HideInInspector] private bool occlusionObjectsSerialized;

		private bool areObjectsVisible = true;

		public Type OccluderType
		{
			get { return occluderType; }
		}

		public float OcclusionDistance
		{
			get { return occlusionDistance; }
		}

		public float OcclusionDistanceSquared
		{
			get { return occlusionDistance * occlusionDistance; }
		}

		private void Awake()
		{
			if (!occlusionObjectsSerialized)
			{
				SerializeOcclusionObjects();
			}
		}

		public void SerializeOcclusionObjects()
		{
			renderers.Clear();
			lights.Clear();
			objects.Clear();
			foreach (GameObject occlusionObject in occlusionObjects)
			{
				SerializeGameObject(occlusionObject);
			}

			occlusionObjectsSerialized = true;
		}

		private void SerializeGameObject(GameObject go)
		{
			if (go == null || !go.activeInHierarchy)
			{
				return;
			}

			bool flag = false;
			Animator[] componentsInChildren = go.GetComponentsInChildren<Animator>();
			foreach (Animator animator in componentsInChildren)
			{
				if (animator.runtimeAnimatorController != null)
				{
					flag = true;
					break;
				}
			}

			if (go.GetComponentInChildren<VesselComponent>(true) != null)
			{
				flag = true;
			}

			if (flag)
			{
				MeshRenderer[] components = go.GetComponents<MeshRenderer>();
				foreach (MeshRenderer item in components)
				{
					renderers.Add(item);
				}

				Light[] components2 = go.GetComponents<Light>();
				foreach (Light item2 in components2)
				{
					lights.Add(item2);
				}

				IEnumerator enumerator = go.transform.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Transform transform = (Transform)enumerator.Current;
						SerializeGameObject(transform.gameObject);
					}

					return;
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = enumerator as IDisposable) != null)
					{
						disposable.Dispose();
					}
				}
			}

			objects.Add(new WholeObject
			{
				Obj = go,
				Parent = go.transform.parent
			});
		}

		public void ShowOccludedObjects(bool show)
		{
			if (show == areObjectsVisible || (!show && !ZeroOcclusion.UseOcclusion))
			{
				return;
			}

			areObjectsVisible = show;
			if (renderers != null && renderers.Count > 0)
			{
				foreach (MeshRenderer renderer in renderers)
				{
					if (renderer != null)
					{
						renderer.enabled = areObjectsVisible;
					}
				}
			}

			if (lights != null && lights.Count > 0)
			{
				foreach (Light light in lights)
				{
					if (light != null)
					{
						light.enabled = areObjectsVisible;
					}
				}
			}

			if (objects == null || objects.Count <= 0)
			{
				return;
			}

			if (ZeroOcclusion.BlackHole == null)
			{
				ZeroOcclusion.BlackHole = new GameObject("BlackHole");
				ZeroOcclusion.BlackHole.transform.parent = transform.root;
				ZeroOcclusion.BlackHole.SetActive(false);
			}

			foreach (WholeObject @object in objects)
			{
				if (@object != null && @object.Obj != null)
				{
					Vector3 localPosition = @object.Obj.transform.localPosition;
					Quaternion localRotation = @object.Obj.transform.localRotation;
					Vector3 localScale = @object.Obj.transform.localScale;
					if (!areObjectsVisible)
					{
						@object.Obj.transform.SetParent(ZeroOcclusion.BlackHole.transform);
					}
					else
					{
						@object.Obj.transform.SetParent(@object.Parent);
					}

					@object.Obj.transform.localPosition = localPosition;
					@object.Obj.transform.localRotation = localRotation;
					@object.Obj.transform.localScale = localScale;
					@object.Obj.SetActive(areObjectsVisible);
				}
			}
		}

		public void DestroyOcclusionObjects()
		{
			foreach (WholeObject @object in objects)
			{
				if (@object != null && @object.Obj != null)
				{
					UnityEngine.Object.Destroy(@object.Obj);
				}
			}
		}
	}
}
