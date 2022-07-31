using System.Collections.Generic;

namespace ZeroGravity.Data
{
	public class PlayerServersData
	{
		public List<long> FavoriteServers = new List<long>();

		public Client.ServerFilters PreviousFilter;
	}
}
