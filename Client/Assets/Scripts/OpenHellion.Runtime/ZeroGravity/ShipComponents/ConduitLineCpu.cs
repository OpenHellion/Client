using System;

namespace ZeroGravity.ShipComponents
{
	[Serializable]
	public class ConduitLineCpu : IConduitLine
	{
		public DistributionLineType Type
		{
			get { return DistributionLineType.CPU; }
		}

		public bool IsConnected { get; private set; }

		public int LineNumber { get; private set; }

		public ConduitLineCpu(int lineNumber)
		{
			LineNumber = lineNumber;
		}
	}
}
