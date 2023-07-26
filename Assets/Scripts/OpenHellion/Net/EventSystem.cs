using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ZeroGravity.Network;
using ZeroGravity;

namespace OpenHellion.Networking
{
	public class EventSystem
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
			ShowMessageBox = 1,
			SetActiveLoginLoadingCanvas = 2,
			OpenMainScreen = 3,
			OcExteriorStatus = 4,
			EquipAnimationEnd = 5,
			ReconnectAuto = 6,
			RemoveLoadingCanvas = 7,
			ConnectionFailed = 8,
			CloseAllLoadingScreens = 9
		}

		private readonly ConcurrentDictionary<Type, NetworkDataDelegate> _listeners = new ConcurrentDictionary<Type, NetworkDataDelegate>();

		private readonly ConcurrentDictionary<InternalEventType, InternalDataDelegate> _internalDataGroups = new ConcurrentDictionary<InternalEventType, InternalDataDelegate>();

		private readonly ConcurrentQueue<InternalEventData> _internalBuffer = new ConcurrentQueue<InternalEventData>();

		private readonly ConcurrentQueue<NetworkData> _networkBuffer = new ConcurrentQueue<NetworkData>();

		private static readonly Dictionary<Type, int> _avgPacketCounter = new Dictionary<Type, int>();
		private static readonly Dictionary<Type, int> _packetCounter = new Dictionary<Type, int>();

		private static bool _dbgPacketCount = false;
		public static bool DebugPacketCount
		{
			get => _dbgPacketCount;
			set
			{
				_dbgPacketCount = value;
				if (!value)
				{
					_packetCounter.Clear();
				}
			}
		}


		private static EventSystem _instance;
		internal static EventSystem Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new EventSystem();
				}

				return _instance;
			}
		}

		/// <summary>
		/// 	Add listener for custom events.
		/// </summary>
		public static void AddListener(Type group, NetworkDataDelegate function)
		{
			var result = Instance._listeners.GetOrAdd(group, function);

			if (result is not null)
			{
				Instance._listeners[group] += function;
			}
		}

		/// <summary>
		/// 	Add listener for base Unity events.
		/// </summary>
		public static void AddListener(InternalEventType group, InternalDataDelegate function)
		{
			var result = Instance._internalDataGroups.GetOrAdd(group, function);

			if (result is not null)
			{
				Instance._internalDataGroups[group] += function;
			}
		}

		/// <summary>
		/// 	Remove listener for custom events.
		/// </summary>
		public static void RemoveListener(Type group, NetworkDataDelegate function)
		{
			Instance._listeners[group] -= function;
		}

		/// <summary>
		/// 	Remove listener for base Unity events.
		/// </summary>
		public static void RemoveListener(InternalEventType group, InternalDataDelegate function)
		{
			Instance._internalDataGroups[group] -= function;
		}

		/// <summary>
		/// 	Execute corresponding code for request.
		/// </summary>
		internal void Invoke(NetworkData data)
		{
			if (_listeners.ContainsKey(data.GetType()) && _listeners[data.GetType()] != null)
			{
				if (Client.Instance.ExperimentalBuild) Dbg.Log("Executing event of type: " + data.GetType());

				if (Thread.CurrentThread.ManagedThreadId == Client.MainThreadID)
				{
					_listeners[data.GetType()](data);
				}
				else
				{
					_networkBuffer.Enqueue(data);
				}
			}
			else
			{
				Dbg.Error("Listener is not registered for data:", data.GetType(), data);
			}
		}

		/// <summary>
		/// 	Execute corresponding code for request.
		/// </summary>
		// TODO: Make this non-static internal.
		public static void Invoke(InternalEventData data)
		{
			if (Instance._internalDataGroups.ContainsKey(data.Type) && Instance._internalDataGroups[data.Type] != null)
			{
				if (!Client.IsGameBuild || Thread.CurrentThread.ManagedThreadId == Client.MainThreadID)
				{
					Instance._internalDataGroups[data.Type](data);
				}
				else
				{
					Instance._internalBuffer.Enqueue(data);
				}
			}
			else
			{
				Dbg.Log("Cannot invoke ", data.Type, data);
			}
		}

		/// <summary>
		/// 	Execute code for requests stored in queue.
		/// </summary>
		internal void InvokeQueuedData()
		{
			_packetCounter.Clear();
			while (_networkBuffer.Count > 0)
			{
				if (!_networkBuffer.TryDequeue(out NetworkData result))
				{
					continue;
				}
				if (_listeners.TryGetValue(result.GetType(), out NetworkDataDelegate value) && value != null)
				{
					value(result);
				}

				if (DebugPacketCount)
				{
					if (result is DynamicObjectStatsMessage)
					{
						if (_avgPacketCounter.ContainsKey(result.GetType()))
						{
							_avgPacketCounter[result.GetType()]++;
						}
						else
						{
							_avgPacketCounter.Add(result.GetType(), 1);
						}
					}
				}

				if (_packetCounter.ContainsKey(result.GetType()))
				{
					_packetCounter[result.GetType()]++;
				}
				else
				{
					_packetCounter.Add(result.GetType(), 1);
				}
			}

			while (_internalBuffer.Count > 0)
			{
				if (_internalBuffer.TryDequeue(out InternalEventData result) && _internalDataGroups.TryGetValue(result.Type, out InternalDataDelegate value) && value != null)
				{
					value(result);
				}
			}
		}
	}
}
