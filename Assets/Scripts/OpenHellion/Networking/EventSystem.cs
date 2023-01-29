using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using ZeroGravity.Network;
using ZeroGravity;

namespace OpenHellion.Networking
{
	public class EventSystem
	{
		public class InternalEventData
		{
			public InternalEventType Type;

			public object[] Objects;

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

		private ConcurrentDictionary<Type, NetworkDataDelegate> _listeners = new ConcurrentDictionary<Type, NetworkDataDelegate>();

		private ConcurrentDictionary<InternalEventType, InternalDataDelegate> _internalDataGroups = new ConcurrentDictionary<InternalEventType, InternalDataDelegate>();

		private ConcurrentQueue<InternalEventData> _internalBuffer = new ConcurrentQueue<InternalEventData>();

		private ConcurrentQueue<NetworkData> _networkBuffer = new ConcurrentQueue<NetworkData>();

		private static Dictionary<Type, int> s_avgPacketCounter = new Dictionary<Type, int>();
		private static Dictionary<Type, int> s_packetCounter = new Dictionary<Type, int>();
		private static float s_frameCounter = 0;
		private static bool s_startCounting = false;
		private static float s_startTime = 0;

		public static bool PrintPackageTime
		{
			set
			{
				s_startCounting = value;
				if (!value)
				{
					foreach (KeyValuePair<Type, int> item in s_avgPacketCounter)
					{
					}
					s_avgPacketCounter = new Dictionary<Type, int>();
				}
				else
				{
					s_startTime = Time.realtimeSinceStartup;
				}
			}
		}


		private static bool s_dbgPacketCount = false;
		public static bool DebugPacketCount
		{
			get
			{
				return s_dbgPacketCount;
			}
			set
			{
				s_dbgPacketCount = value;
				if (!value)
				{
					s_packetCounter = new Dictionary<Type, int>();
				}
			}
		}


		private static EventSystem s_instance;
		internal static EventSystem Instance
		{
			get
			{
				if (s_instance == null)
				{
					s_instance = new EventSystem();
				}

				return s_instance;
			}
		}

		/// <summary>
		/// 	Add listener for custom events.
		/// </summary>
		public static void AddListener(Type group, NetworkDataDelegate function)
		{
			if (Instance._listeners.ContainsKey(group))
			{
				ConcurrentDictionary<Type, NetworkDataDelegate> concurrentDictionary;
				Type key;
				(concurrentDictionary = Instance._listeners)[key = group] = (NetworkDataDelegate)Delegate.Combine(concurrentDictionary[key], function);
			}
			else
			{
				Instance._listeners[group] = function;
			}
		}

		/// <summary>
		/// 	Add listener for base Unity events.
		/// </summary>
		public static void AddListener(InternalEventType group, InternalDataDelegate function)
		{
			if (Instance._internalDataGroups.ContainsKey(group))
			{
				ConcurrentDictionary<InternalEventType, InternalDataDelegate> concurrentDictionary;
				InternalEventType key;
				(concurrentDictionary = Instance._internalDataGroups)[key = group] = (InternalDataDelegate)Delegate.Combine(concurrentDictionary[key], function);
			}
			else
			{
				Instance._internalDataGroups[group] = function;
			}
		}

		/// <summary>
		/// 	Remove listener for custom events.
		/// </summary>
		public static void RemoveListener(Type group, NetworkDataDelegate function)
		{
			if (Instance._listeners.ContainsKey(group))
			{
				ConcurrentDictionary<Type, NetworkDataDelegate> concurrentDictionary;
				Type key;
				(concurrentDictionary = Instance._listeners)[key = group] = (NetworkDataDelegate)Delegate.Remove(concurrentDictionary[key], function);
			}
		}

		/// <summary>
		/// 	Remove listener for base Unity events.
		/// </summary>
		public static void RemoveListener(InternalEventType group, InternalDataDelegate function)
		{
			if (Instance._internalDataGroups.ContainsKey(group))
			{
				ConcurrentDictionary<InternalEventType, InternalDataDelegate> concurrentDictionary;
				InternalEventType key;
				(concurrentDictionary = Instance._internalDataGroups)[key = group] = (InternalDataDelegate)Delegate.Remove(concurrentDictionary[key], function);
			}
		}

		/// <summary>
		/// 	Execute corresponding code for request.
		/// </summary>
		internal void Invoke(NetworkData data)
		{
			if (_listeners.ContainsKey(data.GetType()) && _listeners[data.GetType()] != null)
			{
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
				Dbg.Error("Cannot invoke ", data.GetType(), data);
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
			s_packetCounter.Clear();
			while (_networkBuffer.Count > 0)
			{
				NetworkData result;
				if (!_networkBuffer.TryDequeue(out result))
				{
					continue;
				}
				NetworkDataDelegate value;
				if (_listeners.TryGetValue(result.GetType(), out value) && value != null)
				{
					value(result);
				}
				if (s_startCounting)
				{
					Type type = result.GetType();
					if (result is DynamicObjectStatsMessage)
					{
						if (s_avgPacketCounter.ContainsKey(result.GetType()))
						{
							s_avgPacketCounter[result.GetType()]++;
						}
						else
						{
							s_avgPacketCounter.Add(result.GetType(), 1);
						}
					}
					s_frameCounter += 1f;
				}
				if (s_packetCounter.ContainsKey(result.GetType()))
				{
					s_packetCounter[result.GetType()]++;
				}
				else
				{
					s_packetCounter.Add(result.GetType(), 1);
				}
			}
			if (DebugPacketCount)
			{
				foreach (KeyValuePair<Type, int> item in s_packetCounter)
				{
				}
				DebugPacketCount = false;
			}
			while (_internalBuffer.Count > 0)
			{
				InternalEventData result2;
				InternalDataDelegate value2;
				if (_internalBuffer.TryDequeue(out result2) && _internalDataGroups.TryGetValue(result2.Type, out value2) && value2 != null)
				{
					value2(result2);
				}
			}
		}
	}
}
