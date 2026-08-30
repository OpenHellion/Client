using System;
using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Network;
using ZeroGravity.UI;

namespace ZeroGravity.Objects
{
	[Serializable]
	public class ManeuverTransfer
	{
		public static long NextGUID = 1L;

		public int Index;

		public long GUID;

		public ManeuverType Type;

		public int WarpIndex;

		public List<int> WarpCells = new List<int>();

		public string Name = string.Empty;

		public float StartOrbitAngle;

		public float EndOrbitAngle;

		public double StartSolarSystemTime;

		public double EndSolarSystemTime;

		public float StartEta;

		public float EndEta;

		public float PointOnStableOrbit;

		public Vector3 ThrustVector;

		public float TravelTime;

		public ManeuverUI ManeuverUI;

		public void ToggleWarpCell(int warpCellIndex, bool isSelected)
		{
			if (isSelected && !WarpCells.Contains(warpCellIndex))
			{
				WarpCells.Add(warpCellIndex);
			}
			else if (!isSelected && WarpCells.Contains(warpCellIndex))
			{
				WarpCells.Remove(warpCellIndex);
			}
		}

		public void SetWarpCells(List<int> warpCellIndexes)
		{
			WarpCells.Clear();
			WarpCells = new List<int>(warpCellIndexes);
		}
	}
}
