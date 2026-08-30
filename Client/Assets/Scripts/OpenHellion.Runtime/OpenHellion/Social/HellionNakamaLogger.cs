// HellionNakamaLogger.cs
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

using UnityEngine;
using ILogger = Nakama.ILogger;

namespace OpenHellion.Social
{
	/// <summary>
	///		Implementation of Nakama's ILogger that uses Hellion's own logger.
	/// </summary>
	public class HellionNakamaLogger : ILogger
	{
		public void DebugFormat(string format, params object[] args)
		{
			Debug.LogFormat(format, args);
		}

		public void ErrorFormat(string format, params object[] args)
		{
			Debug.LogErrorFormat(format, args);
		}

		public void InfoFormat(string format, params object[] args)
		{
			Debug.LogFormat(format, args);
		}

		public void WarnFormat(string format, params object[] args)
		{
			Debug.LogWarningFormat(format, args);
		}
	}
}
