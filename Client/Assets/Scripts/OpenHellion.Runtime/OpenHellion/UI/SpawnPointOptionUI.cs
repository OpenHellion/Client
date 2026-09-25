// SpawnPointOptionUI.cs
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

using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZeroGravity;

namespace OpenHellion.UI
{
	public class SpawnPointOptionUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		public TextMeshProUGUI Name;

		public GameObject Selected;

		public void Initialise(string name, string crew, Action onClick)
		{
			Name.text = crew.IsNullOrEmpty() ? name : name + "\n" + crew;

			if (Selected is not null)
			{
				Selected.SetActive(false);
			}

			GetComponent<Button>().onClick.AddListener(delegate { onClick(); });
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (Selected is not null)
			{
				Selected.SetActive(true);
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (Selected is not null)
			{
				Selected.SetActive(false);
			}
		}
	}
}
