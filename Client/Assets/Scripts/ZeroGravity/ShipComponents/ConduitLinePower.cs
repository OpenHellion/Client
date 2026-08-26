using System;

namespace ZeroGravity.ShipComponents
{
	[Serializable]
	public class ConduitLinePower : IConduitLine
	{
		public DistributionLineType Type
		{
			get { return DistributionLineType.Power; }
		}

		public bool IsConnected { get; private set; }

		public int LineNumber { get; private set; }

		public ConduitLinePower(int lineNumber)
		{
			LineNumber = lineNumber;
		}
	}
}
