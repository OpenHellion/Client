using System.Collections.Generic;

namespace ZeroGravity.ShipComponents
{
	public static class ComponentsSettings
	{
		private static Dictionary<DistributionLineType, int> numberOfDistributionLines =
			new Dictionary<DistributionLineType, int>
			{
				{
					DistributionLineType.Power,
					3
				},
				{
					DistributionLineType.CPU,
					1
				},
				{
					DistributionLineType.Air,
					1
				},
				{
					DistributionLineType.Helium3,
					1
				}
			};

		public static int GetNumberOfDistributionLines(DistributionLineType type)
		{
			try
			{
				return numberOfDistributionLines[type];
			}
			catch
			{
				return 0;
			}
		}
	}
}
