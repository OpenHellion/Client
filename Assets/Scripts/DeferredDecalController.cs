using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
public class DeferredDecalController : MonoBehaviour
{
	[SerializeField] [HideInInspector] private Mesh myCubeMesh;

	private CommandBuffer myCommandBuffer;

	[CompilerGenerated] private static Comparison<Decal> _003C_003Ef__am_0024cache0;

	private void Start()
	{
		if (myCubeMesh == null)
		{
			Debug.LogError(
				"The cube mesh used for rendering the decals must be set in the inspector window for this SCRIPT! Disabling this.");
			base.enabled = false;
		}

		if (myCommandBuffer == null)
		{
			myCommandBuffer = new CommandBuffer();
		}

		myCommandBuffer.name = "Deferred Decals";
		base.gameObject.GetComponent<Camera>().RemoveCommandBuffer(CameraEvent.BeforeLighting, myCommandBuffer);
		base.gameObject.GetComponent<Camera>().AddCommandBuffer(CameraEvent.BeforeLighting, myCommandBuffer);
	}

	private void OnEnable()
	{
		if (myCommandBuffer == null)
		{
			myCommandBuffer = new CommandBuffer();
		}

		myCommandBuffer.name = "Deferred Decals";
		base.gameObject.GetComponent<Camera>().RemoveCommandBuffer(CameraEvent.BeforeLighting, myCommandBuffer);
	}

	public void OnDisable()
	{
		base.gameObject.GetComponent<Camera>().RemoveCommandBuffer(CameraEvent.BeforeLighting, myCommandBuffer);
	}

	public static void AddDecal(Decal aDecal)
	{
		DeferredDecalHolder.instance.AddDecal(aDecal);
	}

	public static void RemoveDecal(Decal aDecal)
	{
		DeferredDecalHolder.instance.RemoveDecal(aDecal);
	}

	private void OnPreRender()
	{
		if (!base.gameObject.activeInHierarchy || !base.enabled)
		{
			OnDisable();
			return;
		}

		myCommandBuffer.Clear();
		int num = Shader.PropertyToID("_NormalsCopy");
		myCommandBuffer.GetTemporaryRT(num, -1, -1);
		myCommandBuffer.Blit(BuiltinRenderTextureType.GBuffer2, num);
		RenderTargetIdentifier[] colors = new RenderTargetIdentifier[3]
		{
			BuiltinRenderTextureType.GBuffer1,
			BuiltinRenderTextureType.GBuffer0,
			BuiltinRenderTextureType.GBuffer2
		};
		myCommandBuffer.SetRenderTarget(colors, BuiltinRenderTextureType.CameraTarget);
		List<Decal> list = new List<Decal>(DeferredDecalHolder.instance.myDecals.Count);
		list.AddRange(DeferredDecalHolder.instance.myDecals);
		if (_003C_003Ef__am_0024cache0 == null)
		{
			_003C_003Ef__am_0024cache0 = _003COnPreRender_003Em__0;
		}

		list.Sort(_003C_003Ef__am_0024cache0);
		foreach (Decal item in list)
		{
			if (item.material == null)
			{
				Dbg.Error("Decal doesn't have material set", item);
			}

			myCommandBuffer.DrawMesh(myCubeMesh, item.transform.localToWorldMatrix, item.material, 0, 0);
			if (item.material.HasProperty("_ApplyRoughness") && item.material.GetFloat("_ApplyRoughness") != 0f)
			{
				myCommandBuffer.DrawMesh(myCubeMesh, item.transform.localToWorldMatrix, item.material, 0, 1);
			}
		}

		myCommandBuffer.ReleaseTemporaryRT(num);
	}

	[CompilerGenerated]
	private static int _003COnPreRender_003Em__0(Decal a, Decal b)
	{
		return a.sortingLayer - b.sortingLayer;
	}
}
