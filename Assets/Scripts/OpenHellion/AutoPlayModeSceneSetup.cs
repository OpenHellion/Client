// AutoPlayModeSceneSetup.cs
//
// Copyright (C) 2023, OpenHellion contributors
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

using UnityEditor;
using UnityEditor.SceneManagement;

namespace OpenHellion
{
#if UNITY_EDITOR
	/// Got from thread: https://forum.unity.com/threads/executing-first-scene-in-build-settings-when-pressing-play-button-in-editor.157502/#post-4152451
	/// <summary>
	///		This script ensures the first scene defined in build settings (if any) is always loaded when entering play mode.
	/// </summary>
	[InitializeOnLoad]
	public class AutoPlayModeSceneSetup
	{
		static AutoPlayModeSceneSetup()
		{
			// Execute once on Unity editor startup.
			SetStartScene();

			// Also execute whenever build settings change.
			EditorBuildSettings.sceneListChanged += SetStartScene;
		}

		static void SetStartScene()
		{
			// At least one scene in build settings?
			if (EditorBuildSettings.scenes.Length > 0)
			{
				// Set start scene to first scene in build settings.
				EditorSceneManager.playModeStartScene =
					AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorBuildSettings.scenes[0].path);
			}
			else
			{
				// Reset start scene.
				EditorSceneManager.playModeStartScene = null;
			}
		}
	}
#endif
}
