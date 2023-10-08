using System;

namespace ZeroGravity.ShipComponents
{
	[Serializable]
	public class ConduitLineDeuterium : IConduitLine
	{
		public DistributionLineType Type
		{
			get { return DistributionLineType.Helium3; }
		}

		public bool IsConnected { get; private set; }

		public int LineNumber { get; private set; }

		public ConduitLineDeuterium(int lineNumber)
		{
			LineNumber = lineNumber;
		}
	}
}
