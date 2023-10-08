using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using ZeroGravity.LevelDesign;

namespace ZeroGravity.Effects
{
	public class RenderToCubeMap : MonoBehaviour
	{
		[Serializable]
		public class CubemapSettings
		{
			public int Size = 512;

			public int Depth = 16;

			public float UpdateTimeSeconds = 1f;
		}

		private RenderTexture cubeMapTextureDocking;

		private RenderTexture cubeMapTextureReflection;

		public Camera CubemapCamera;

		public List<ReflectionProbe> CubemapProbes = new List<ReflectionProbe>();

		public List<SceneDockingPort> DockingPorts = new List<SceneDockingPort>();

		public CubemapSettings DockingPortSettings = new CubemapSettings();

		public CubemapSettings ReflectionProbeSettings = new CubemapSettings();

		private float updateTimerDockingPort;

		private float updateTimerReflectionProbe;

		private void Awake()
		{
			if (CubemapCamera == null)
			{
				CubemapCamera = GetComponent<Camera>();
			}

			cubeMapTextureDocking = new RenderTexture(DockingPortSettings.Size, DockingPortSettings.Size,
				DockingPortSettings.Depth);
			cubeMapTextureDocking.dimension = TextureDimension.Cube;
			cubeMapTextureDocking.hideFlags = HideFlags.HideAndDontSave;
			cubeMapTextureDocking.autoGenerateMips = true;
			CubemapCamera.RenderToCubemap(cubeMapTextureDocking);
			cubeMapTextureReflection = new RenderTexture(ReflectionProbeSettings.Size, ReflectionProbeSettings.Size,
				ReflectionProbeSettings.Depth);
			cubeMapTextureReflection.dimension = TextureDimension.Cube;
			cubeMapTextureReflection.hideFlags = HideFlags.HideAndDontSave;
			cubeMapTextureReflection.autoGenerateMips = true;
			CubemapCamera.RenderToCubemap(cubeMapTextureReflection);
			foreach (ReflectionProbe cubemapProbe in CubemapProbes)
			{
				SetProbeData(cubemapProbe);
			}
		}

		private void Update()
		{
			if (CubemapProbes.Count > 0)
			{
				updateTimerReflectionProbe += Time.deltaTime;
				if (updateTimerReflectionProbe >= ReflectionProbeSettings.UpdateTimeSeconds)
				{
					RenderCubemapForReflectionProbe();
					updateTimerReflectionProbe = 0f;
				}
			}

			if (DockingPorts.Count > 0)
			{
				updateTimerDockingPort += Time.deltaTime;
				if (updateTimerDockingPort >= DockingPortSettings.UpdateTimeSeconds)
				{
					RenderCubemapForDockingPort();
					updateTimerDockingPort = 0f;
				}
			}
		}

		private void SetProbeData(ReflectionProbe probe)
		{
			probe.mode = ReflectionProbeMode.Custom;
			probe.resolution = ReflectionProbeSettings.Size;
			probe.customBakedTexture = cubeMapTextureReflection;
		}

		public void RenderCubemapForReflectionProbe()
		{
			CubemapCamera.RenderToCubemap(cubeMapTextureReflection);
		}

		public void RenderCubemapForDockingPort()
		{
			CubemapCamera.RenderToCubemap(cubeMapTextureDocking);
		}

		public void AddReflectionProbe(ReflectionProbe probe)
		{
			CubemapProbes.Add(probe);
			SetProbeData(probe);
		}

		public void RemoveReflectionProbe(ReflectionProbe probe)
		{
			CubemapProbes.Remove(probe);
			SetProbeData(probe);
		}

		public void AddDockingPort(SceneDockingPort dockingPort)
		{
			DockingPorts.Add(dockingPort);
		}

		public void RemoveDockingPort(SceneDockingPort dockingPort)
		{
			DockingPorts.Remove(dockingPort);
		}
	}
}
