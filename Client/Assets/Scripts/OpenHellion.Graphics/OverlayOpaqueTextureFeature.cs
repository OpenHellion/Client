// OverlayOpaqueTextureFeature.cs
//
// Copyright (C) 2026, OpenHellion contributors
//
// SPDX-License-Identifier: GPL-3.0-or-later
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace OpenHellion.Graphics
{
	/// <summary>
	/// 	Gives overlay cameras the camera opaque texture (_CameraOpaqueTexture), which URP otherwise makes only
	/// 	for base cameras. Shaders that read the screen behind them, such as the warp tunnel distortion that
	/// 	PlanetsCamera draws, need it.
	/// </summary>
	/// <remarks>
	/// 	URP turns off an overlay camera's own Opaque Texture setting, but it still copies the colour after the
	/// 	opaque objects when a render pass asks for the colour input. For an overlay camera that copy holds the
	/// 	whole stack so far (SunCamera's sun and skybox, then PlanetsCamera's opaque planets). Transparent
	/// 	objects drawn before the reading shader aren't in it, unlike a GrabPass.
	///
	/// 	The copy follows the pipeline asset's Opaque Downsampling. Set it to None: at 2x Bilinear the
	/// 	distortion shows a blurred, half-resolution background.
	/// 	TODO: This class might not be neccessary if this distortion is rewritten from scratch using URP features?
	/// </remarks>
	public class OverlayOpaqueTextureFeature : ScriptableRendererFeature
	{
		[Tooltip("Cameras that draw any of these layers get the opaque texture. Others using this renderer (the map " +
			"camera) don't pay for the copy.")]
		public LayerMask CameraLayers = 1 << 9; // Planets

		private RequestColorPass _pass;

		public override void Create()
		{
			_pass = new RequestColorPass();
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			Camera camera = renderingData.cameraData.camera;
			if (camera.cameraType != CameraType.Game || (camera.cullingMask & CameraLayers) == 0)
			{
				return;
			}

			renderer.EnqueuePass(_pass);
		}

		/// <summary>
		/// 	Records nothing. Declaring the colour input is what makes URP schedule its colour copy.
		/// </summary>
		private class RequestColorPass : ScriptableRenderPass
		{
			public RequestColorPass()
			{
				renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
				ConfigureInput(ScriptableRenderPassInput.Color);
			}

			public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
			{
			}
		}
	}
}
