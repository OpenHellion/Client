// SpaceBackground.cs
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
using UnityEngine.Experimental.Rendering;

namespace OpenHellion.Graphics
{
	/// <summary>
	/// 	Renders the camera it is on into a screen-sized HDR texture that the world camera shows behind the
	/// 	world. Put it on SunCamera, the base camera of the stack that draws the sun and the planets.
	/// </summary>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Camera))]
	public class SpaceBackground : MonoBehaviour
	{
		private static readonly int SpaceBackgroundTexture = Shader.PropertyToID("_SpaceBackgroundTex");

		private Camera _camera;

		private RenderTexture _texture;

		private void OnEnable()
		{
			_camera = GetComponent<Camera>();
			UpdateTexture();
		}

		private void LateUpdate()
		{
			UpdateTexture();
		}

		private void OnDisable()
		{
			_camera.targetTexture = null;
			Shader.SetGlobalTexture(SpaceBackgroundTexture, Texture2D.blackTexture);
			DestroyTexture(_texture);
			_texture = null;
		}

		/// <summary>
		/// 	Keeps the texture the size of the screen, which the world camera renders to. HDR, so the sun stays
		/// 	bright enough for the world camera's bloom.
		/// </summary>
		private void UpdateTexture()
		{
			int width = Mathf.Max(1, Screen.width);
			int height = Mathf.Max(1, Screen.height);
			if (_texture != null && _texture.width == width && _texture.height == height)
			{
				return;
			}

			RenderTexture previous = _texture;
			_texture = new RenderTexture(width, height, SystemInfo.GetGraphicsFormat(DefaultFormat.HDR),
				SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil))
			{
				name = "SpaceBackground"
			};
			_texture.Create();
			_camera.targetTexture = _texture;
			Shader.SetGlobalTexture(SpaceBackgroundTexture, _texture);
			DestroyTexture(previous);
		}

		private static void DestroyTexture(RenderTexture texture)
		{
			if (texture != null)
			{
				texture.Release();
				Destroy(texture);
			}
		}
	}
}
