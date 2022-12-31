using System;

namespace OpenHellion.ProviderSystem
{
	// Class to generate an id for internal use in the game.
	internal class IdGenerator
	{
		private long _generatedId;
		public long GeneratedId
		{
			get
			{
				return _generatedId;
			}
		}

		private Random _random;

		public IdGenerator()
		{
			// Initialise seeding.
			_random = new Random((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds());

			RegenerateId();
		}

		public void RegenerateId()
		{
			_generatedId = NextInt64();
		}

		private Int64 NextInt64()
		{
		    byte[] buffer = new byte[sizeof(Int64)];
		    _random.NextBytes(buffer);
		    return BitConverter.ToInt64(buffer, 0);
		}
	}
}
