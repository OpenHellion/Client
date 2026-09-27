using System;
using UnityEngine;

namespace OpenHellion.Graphics
{
	/// <summary>
	/// Replaces Decalicious decals, so they emit a warning instead of silent-failing.
	/// </summary>
	[ExecuteAlways]
	[Obsolete]
	public class Decalicious : MonoBehaviour
	{
		public enum DecalRenderMode
		{
			Deferred = 0,
			Unlit = 1,
			Invalid = 2
		}

		public DecalRenderMode RenderMode = DecalRenderMode.Invalid;

		[Tooltip("Set a Material with a Decalicious shader.")]
		public Material Material;

		[Tooltip("Should this decal be drawn early (low number) or late (high number)?")]
		public int RenderOrder = 100;

		[Tooltip(
			"To which degree should the Decal be drawn? At 1, the Decal will be drawn with full effect. At 0, the Decal will not be drawn. Experiment with values greater than one.")]
		public float Fade = 1f;

		[Tooltip("Set a GameObject here to only draw this Decal on the MeshRenderer of the GO or any of its children.")]
		public GameObject LimitTo;

		[Tooltip("Enable to draw the Albedo / Emission pass of the Decal.")]
		public bool DrawAlbedo = true;

		[Tooltip(
			"Use an interpolated light probe for this decal for indirect light. This breaks instancing for the decal and thus comes with a performance impact, so use with caution.")]
		public bool UseLightProbes = true;

		[Tooltip("Enable to draw the Normal / SpecGloss pass of the Decal.")]
		public bool DrawNormalAndGloss = true;

		[Tooltip(
			"Enable perfect Normal / SpecGloss blending between decals. Costly and has no effect when decals don't overlap, so use with caution.")]
		public bool HighQualityBlending;

		void Awake()
		{
			Debug.LogWarning("A gameobject in the scene contains a Decalicious decal, which will not render in the game. This may cause missing detail.", this);
		}

		private void OnDrawGizmos()
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = Color.clear;
			Gizmos.DrawCube(Vector3.zero, Vector3.one);
			Gizmos.color = Color.white * 0.2f;
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = Color.white * 0.5f;
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		}
	}
}
