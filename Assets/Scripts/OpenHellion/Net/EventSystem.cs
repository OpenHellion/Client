using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using ZeroGravity.Network;

namespace OpenHellion.Net
{
	public static class EventSystem
	{
		public class InternalEventData
		{
			public readonly InternalEventType Type;

			public readonly object[] Objects;

			public InternalEventData(InternalEventType type, params object[] objects)
			{
				Type = type;
				Objects = objects;
			}
		}

		public delegate void NetworkDataDelegate(NetworkData data);

		public delegate void InternalDataDelegate(InternalEventData data);

		public enum InternalEventType
		{
			OpenMainScreen = 3,
			OcExteriorStatus = 4,
			EquipAnimationEnd = 5,
			ReconnectAuto = 6,
			ConnectionFailed = 8,
		}

		private static readonly ConcurrentDictionary<Type, NetworkDataDelegate> Listeners =
			new ConcurrentDictionary<Type, NetworkDataDelegate>();

		private static readonly ConcurrentDictionary<InternalEventType, InternalDataDelegate> InternalDataGroups =
			new ConcurrentDictionary<InternalEventType, InternalDataDelegate>();

		private static readonly ConcurrentQueue<InternalEventData> InternalBuffer = new ConcurrentQueue<InternalEventData>();

		private static readonly ConcurrentQueue<NetworkData> NetworkBuffer = new ConcurrentQueue<NetworkData>();

		private static readonly Dictionary<Type, int> AvgPacketCounter = new Dictionary<Type, int>();
		private static readonly Dictionary<Type, int> PacketCounter = new Dictionary<Type, int>();

		private static bool _dbgPacketCount = false;

		public static bool DebugPacketCount
		{
			get => _dbgPacketCount;
			set
			{
				_dbgPacketCount = value;
				if (!value)
				{
					PacketCounter.Clear();
				}
			}
		}

		/// <summary>
		/// 	Add listener for custom events.
		/// </summary>
		public static void AddListener(Type group, NetworkDataDelegate function)
		{
			var result = Listeners.GetOrAdd(group, function);

			if (result is not null)
			{
				Listeners[group] += function;
			}
		}

		/// <summary>
		/// 	Add listener for base Unity events.
		/// </summary>
		public static void AddListener(InternalEventType group, InternalDataDelegate function)
		{
			var result = InternalDataGroups.GetOrAdd(group, function);

			if (result is not null)
			{
				InternalDataGroups[group] += function;
			}
		}

		/// <summary>
		/// 	Remove listener for custom events.
		/// </summary>
		public static void RemoveListener(Type group, NetworkDataDelegate function)
		{
			Listeners[group] -= function;
		}

		/// <summary>
		/// 	Remove listener for base Unity events.
		/// </summary>
		public static void RemoveListener(InternalEventType group, InternalDataDelegate function)
		{
			InternalDataGroups[group] -= function;
		}

		/// <summary>
		/// 	Execute corresponding code for request.
		/// </summary>
		internal static void Invoke(NetworkData data)
		{
			if (Listeners.ContainsKey(data.GetType()) && Listeners[data.GetType()] != null)
			{
				if (Thread.CurrentThread.ManagedThreadId == World.MainThreadID)
				{
					Listeners[data.GetType()](data);
				}
				else
				{
					NetworkBuffer.Enqueue(data);
				}
			}
			else
			{
				Debug.LogError("Listener is not registered for data:" + data.GetType() + data);
			}
		}

		/// <summary>
		/// 	Execute corresponding code for request.
		/// </summary>
		public static void Invoke(InternalEventData data)
		{
			if (InternalDataGroups.ContainsKey(data.Type) && InternalDataGroups[data.Type] != null)
			{
				if (Thread.CurrentThread.ManagedThreadId == World.MainThreadID)
				{
					InternalDataGroups[data.Type](data);
				}
				else
				{
					InternalBuffer.Enqueue(data);
				}
			}
			else
			{
				Debug.Log("Cannot invoke " + data.Type + data);
			}
		}

		/// <summary>
		/// 	Execute code for requests stored in queue.
		/// </summary>
		internal static void InvokeQueuedData()
		{
			PacketCounter.Clear();
			while (NetworkBuffer.Count > 0)
			{
				if (!NetworkBuffer.TryDequeue(out NetworkData result))
				{
					continue;
				}

				if (Listeners.TryGetValue(result.GetType(), out NetworkDataDelegate value) && value != null)
				{
					value(result);
				}

				if (DebugPacketCount)
				{
					if (result is DynamicObjectStatsMessage)
					{
						if (AvgPacketCounter.ContainsKey(result.GetType()))
						{
							AvgPacketCounter[result.GetType()]++;
						}
						else
						{
							AvgPacketCounter.Add(result.GetType(), 1);
						}
					}
				}

				if (PacketCounter.ContainsKey(result.GetType()))
				{
					PacketCounter[result.GetType()]++;
				}
				else
				{
					PacketCounter.Add(result.GetType(), 1);
				}
			}

			while (InternalBuffer.Count > 0)
			{
				if (InternalBuffer.TryDequeue(out InternalEventData result) &&
				    InternalDataGroups.TryGetValue(result.Type, out InternalDataDelegate value) && value != null)
				{
					value(result);
				}
			}
		}
	}
}
