namespace ZeroGravity.ShipComponents
{
	public interface IConduitLine
	{
		DistributionLineType Type { get; }

		bool IsConnected { get; }

		int LineNumber { get; }
	}
}
