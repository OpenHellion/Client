using System;

namespace ZeroGravity.ShipComponents
{
	[Serializable]
	public class ConduitLineAir : IConduitLine
	{
		public DistributionLineType Type
		{
			get
			{
				return DistributionLineType.Air;
			}
		}

		public bool IsConnected { get; private set; }

		public int LineNumber { get; private set; }

		public ConduitLineAir(int lineNumber)
		{
			LineNumber = lineNumber;
		}
	}
}
