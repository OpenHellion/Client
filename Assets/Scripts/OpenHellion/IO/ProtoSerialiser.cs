using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using ProtoBuf;
using UnityEngine;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace OpenHellion.IO
{
	/// <summary>
	/// 	Class for deserialisation of network messages.
	/// 	Handles safe deserialisation of packets from the server and processes statistics.
	/// </summary
	public static class ProtoSerialiser
	{
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
		public static NetworkData Deserialise(Stream ms)
		{
			NetworkData networkData = null;
			ms.Position = 0L;
			try
			{
				networkData = Serializer.Deserialize<NetworkData>(ms);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
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

		public static async UniTask<NetworkData> Unpack(Stream stream, int maxMessageSize)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			if (!stream.CanRead) throw new ArgumentException("Cannot read from stream.");
			int dataRead = 0;
			int readSize;

			// Get size of buffer
			byte[] dataLengthBuffer = new byte[4];
			do
			{
				readSize = await stream.ReadAsync(dataLengthBuffer.AsMemory(dataRead, dataLengthBuffer.Length - dataRead));
				if (readSize == 0)
				{
					throw new Exception("Received zero data message.");
				}

				dataRead += readSize;
			} while (dataRead < dataLengthBuffer.Length);

			uint dataLength = BitConverter.ToUInt32(dataLengthBuffer, 0);
			if (dataLength > maxMessageSize)
			{
				throw new ArgumentException($"Message too large. Payload of {dataLength}.");
			}

			// Read following contents.
			byte[] buffer = new byte[dataLength];
			dataRead = 0;
			do
			{
				readSize = await stream.ReadAsync(buffer.AsMemory(dataRead, buffer.Length - dataRead));
				if (readSize == 0)
				{
					throw new Exception("Received zero data message.");
				}

				dataRead += readSize;
			} while (dataRead < buffer.Length);

			// Make the stream into NetworkData.
			using MemoryStream ms = new MemoryStream(buffer, 0, buffer.Length);
			return Deserialise(ms);
		}

		public static async UniTask<byte[]> Pack(NetworkData data)
		{
			await using MemoryStream ms = new MemoryStream();

			try
			{
				Serializer.Serialize(ms, data);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
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

			await using MemoryStream outMs = new MemoryStream();
			await outMs.WriteAsync(BitConverter.GetBytes((uint)ms.Length), 0, 4);
			await outMs.WriteAsync(ms.ToArray(), 0, (int)ms.Length);
			await outMs.FlushAsync();
			return outMs.ToArray();
		}

		private static void ProcessStatistics(NetworkData data, Stream ms,
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
					   (item.Value.ByteSum / 1000f).ToString("##,0") + " kB), \n";
				item.Value.BytesSinceLastCheck = 0L;
				num += item.Value.ByteSum;
			}

			text2 = text;
			text = text2 + "-----------------------------------------\nTotal: " +
				   (num / 1000f).ToString("##,0") + " kB (avg: " +
				   (num / timeSpan.TotalSeconds / 1000.0).ToString("##,0") + " kB/s)";
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
