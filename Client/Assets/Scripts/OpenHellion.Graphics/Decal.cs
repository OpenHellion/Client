using System;
using UnityEngine;

namespace OpenHellion.Graphics
{
	/// <summary>
	/// Replaces DeferredDecal decals, so they emit a warning instead of silent-failing.
	/// </summary>
	[Obsolete]
	public class Decal : MonoBehaviour
	{
		[SerializeField] private Material myMaterial;

		[SerializeField] private int mySortingLayer;

		void Awake()
		{
			Debug.LogWarning("A gameobject in the scene contains a DeferredDecal, which will not render in the game. This may cause missing detail.", this);
		}

		public Material material
		{
			get { return myMaterial; }
			set { myMaterial = value; }
		}

		public int sortingLayer
		{
			get { return mySortingLayer; }
		}
	}
}
