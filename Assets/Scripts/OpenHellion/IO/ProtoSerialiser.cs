using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using ProtoBuf;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace OpenHellion.IO
{
	/// <summary>
	/// 	Class for deserialisation of network messages. Handles safe deserialsation of packets from the server.
	/// </summary>
	public static class ProtoSerialiser
	{
		private class ZeroDataException : Exception
		{
			public ZeroDataException(string message)
				: base(message)
			{
			}
		}

		private class StatisticsHelper
		{
			public long ByteSum;

			public int PacketNumber;

			public long BytesSinceLastCheck;

			public StatisticsHelper(long bytes)
			{
				ByteSum = bytes;
				PacketNumber = 1;
				BytesSinceLastCheck = bytes;
			}
		}

		private static DateTime _statisticUpdateResetTime = DateTime.UtcNow;

		private static DateTime _lastStatisticUpdateTime;

		private static readonly double _statisticsLogUpdateTime = 1.0;

		private static readonly Dictionary<Type, StatisticsHelper> _sentStatistics =
			new Dictionary<Type, StatisticsHelper>();

		private static readonly Dictionary<Type, StatisticsHelper> _receivedStatistics =
			new Dictionary<Type, StatisticsHelper>();

		/// <summary>
		/// 	For deserialisation of data not sent through network.
		/// </summary>
		private static NetworkData Deserialize(MemoryStream ms)
		{
			NetworkData networkData = null;
			ms.Position = 0L;
			try
			{
				networkData = Serializer.Deserialize<NetworkDataTransportWrapper>(ms).data;
			}
			catch (Exception ex)
			{
				Dbg.Error("Failed to deserialize communication data", ex.Message, ex.StackTrace);
			}

			if (_statisticsLogUpdateTime > 0.0)
			{
				try
				{
					ProcessStatistics(networkData, ms, _receivedStatistics);
					return networkData;
				}
				catch
				{
					return networkData;
				}
			}

			return networkData;
		}

		public static async Task<NetworkData> Unpack(Stream str)
		{
			int dataReadSize = 0;
			int size;

			// Get size of buffer
			byte[] bufferSize = new byte[4];
			do
			{
				size = await str.ReadAsync(bufferSize, dataReadSize, bufferSize.Length - dataReadSize);
				if (size == 0)
				{
					throw new ZeroDataException("Received zero data message.");
				}

				dataReadSize += size;
			} while (dataReadSize < bufferSize.Length);

			uint bufferLength = BitConverter.ToUInt32(bufferSize, 0);

			// Read following contents.
			byte[] buffer = new byte[bufferLength];
			dataReadSize = 0;
			do
			{
				size = await str.ReadAsync(buffer, dataReadSize, buffer.Length - dataReadSize);
				if (size == 0)
				{
					throw new ZeroDataException("Received zero data message.");
				}

				dataReadSize += size;
			} while (dataReadSize < buffer.Length);

			// Make the stream into NetworkData.
			MemoryStream ms = new MemoryStream(buffer, 0, buffer.Length);
			return Deserialize(ms);
		}

		public static async Task<byte[]> Pack(NetworkData data)
		{
			await using MemoryStream outMs = new MemoryStream();
			await using MemoryStream ms = new MemoryStream();

			try
			{
				NetworkDataTransportWrapper dataWrapper = new NetworkDataTransportWrapper
				{
					data = data
				};

				await Task.Run(() => Serializer.Serialize(ms, dataWrapper));
			}
			catch (Exception ex)
			{
				Dbg.Error("Failed to serialize communication data", ex.Message, ex.StackTrace);
				return null;
			}

			if (_statisticsLogUpdateTime > 0.0)
			{
				try
				{
					ProcessStatistics(data, ms, _sentStatistics);
				}
				catch
				{
					// Ignored.
				}
			}

			await outMs.WriteAsync(BitConverter.GetBytes((uint)ms.Length), 0, 4);
			await outMs.WriteAsync(ms.ToArray(), 0, (int)ms.Length);
			await outMs.FlushAsync();
			return outMs.ToArray();
		}

		private static void ProcessStatistics(NetworkData data, MemoryStream ms,
			Dictionary<Type, StatisticsHelper> stat)
		{
			Type type = data.GetType();
			if (stat.TryGetValue(type, out var value))
			{
				value.ByteSum += ms.Length;
				value.PacketNumber++;
				value.BytesSinceLastCheck += ms.Length;
			}
			else
			{
				stat[type] = new StatisticsHelper(ms.Length);
			}

			if (!(DateTime.UtcNow.Subtract(_lastStatisticUpdateTime).TotalSeconds >= _statisticsLogUpdateTime))
			{
				return;
			}

			TimeSpan timeSpan = DateTime.UtcNow.Subtract(_statisticUpdateResetTime);
			string text = (stat != _sentStatistics)
				? ("Received packets statistics (" + timeSpan.ToString("h':'mm':'ss") + "): \n")
				: ("Sent packets statistics (" + timeSpan.ToString("h':'mm':'ss") + "): \n");
			long num = 0L;
			string text2;
			foreach (KeyValuePair<Type, StatisticsHelper> item in stat
				         .OrderBy((KeyValuePair<Type, StatisticsHelper> m) => m.Value.ByteSum).Reverse())
			{
				text2 = text;
				text = text2 + item.Key.Name + ": " + item.Value.PacketNumber + " (" +
				       ((float)item.Value.ByteSum / 1000f).ToString("##,0") + " kB), \n";
				item.Value.BytesSinceLastCheck = 0L;
				num += item.Value.ByteSum;
			}

			text2 = text;
			text = text2 + "-----------------------------------------\nTotal: " +
			       ((float)num / 1000f).ToString("##,0") + " kB (avg: " +
			       ((double)num / timeSpan.TotalSeconds / 1000.0).ToString("##,0") + " kB/s)";
			if (MyPlayer.Instance != null)
			{
				if (stat == _sentStatistics)
				{
					MyPlayer.Instance.SentPacketStatistics = text;
				}
				else
				{
					MyPlayer.Instance.ReceivedPacketStatistics = text;
				}
			}

			_lastStatisticUpdateTime = DateTime.UtcNow;
		}

		public static void ResetStatistics()
		{
			_sentStatistics.Clear();
			_receivedStatistics.Clear();
			_statisticUpdateResetTime = DateTime.UtcNow;
			_lastStatisticUpdateTime = DateTime.UtcNow;
		}
	}
}
