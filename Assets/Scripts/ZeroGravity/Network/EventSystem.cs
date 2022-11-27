using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ZeroGravity.Network
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
			GravityChange = 0,
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

		private ConcurrentDictionary<Type, NetworkDataDelegate> networkDataGroups = new ConcurrentDictionary<Type, NetworkDataDelegate>();

		private ConcurrentQueue<NetworkData> networkBuffer = new ConcurrentQueue<NetworkData>();

		private Dictionary<Type, int> packetCounter = new Dictionary<Type, int>();

		private Dictionary<Type, int> avgPacketCounter = new Dictionary<Type, int>();

		private float frameCounter;

		private bool startCounting;

		private float startTime;

		private bool dbgPacketcoung;

		private ConcurrentDictionary<InternalEventType, InternalDataDelegate> internalDataGroups = new ConcurrentDictionary<InternalEventType, InternalDataDelegate>();

		private ConcurrentQueue<InternalEventData> internalBuffer = new ConcurrentQueue<InternalEventData>();

		public bool PrintajAvg
		{
			set
			{
				startCounting = value;
				if (!value)
				{
					foreach (KeyValuePair<Type, int> item in avgPacketCounter)
					{
					}
					avgPacketCounter = new Dictionary<Type, int>();
				}
				else
				{
					startTime = Time.realtimeSinceStartup;
				}
			}
		}

		public bool DebugPacketCount
		{
			get
			{
				return dbgPacketcoung;
			}
			set
			{
				dbgPacketcoung = value;
				if (!value)
				{
					packetCounter = new Dictionary<Type, int>();
				}
			}
		}

		/// <summary>
		/// 	Add listener for custom events.
		/// </summary>
		public void AddListener(Type group, NetworkDataDelegate function)
		{
			if (networkDataGroups.ContainsKey(group))
			{
				ConcurrentDictionary<Type, NetworkDataDelegate> concurrentDictionary;
				Type key;
				(concurrentDictionary = networkDataGroups)[key = group] = (NetworkDataDelegate)Delegate.Combine(concurrentDictionary[key], function);
			}
			else
			{
				networkDataGroups[group] = function;
			}
		}

		/// <summary>
		/// 	Add listener for base Unity events.
		/// </summary>
		public void AddListener(InternalEventType group, InternalDataDelegate function)
		{
			if (internalDataGroups.ContainsKey(group))
			{
				ConcurrentDictionary<InternalEventType, InternalDataDelegate> concurrentDictionary;
				InternalEventType key;
				(concurrentDictionary = internalDataGroups)[key = group] = (InternalDataDelegate)Delegate.Combine(concurrentDictionary[key], function);
			}
			else
			{
				internalDataGroups[group] = function;
			}
		}

		/// <summary>
		/// 	Remove listener for custom events.
		/// </summary>
		public void RemoveListener(Type group, NetworkDataDelegate function)
		{
			if (networkDataGroups.ContainsKey(group))
			{
				ConcurrentDictionary<Type, NetworkDataDelegate> concurrentDictionary;
				Type key;
				(concurrentDictionary = networkDataGroups)[key = group] = (NetworkDataDelegate)Delegate.Remove(concurrentDictionary[key], function);
			}
		}

		/// <summary>
		/// 	Remove listener for base Unity events.
		/// </summary>
		public void RemoveListener(InternalEventType group, InternalDataDelegate function)
		{
			if (internalDataGroups.ContainsKey(group))
			{
				ConcurrentDictionary<InternalEventType, InternalDataDelegate> concurrentDictionary;
				InternalEventType key;
				(concurrentDictionary = internalDataGroups)[key = group] = (InternalDataDelegate)Delegate.Remove(concurrentDictionary[key], function);
			}
		}

		/// <summary>
		/// 	Execute corresponding code for request.
		/// </summary>
		public void Invoke(NetworkData data)
		{
			if (networkDataGroups.ContainsKey(data.GetType()) && networkDataGroups[data.GetType()] != null)
			{
				if (Thread.CurrentThread.ManagedThreadId == Client.MainThreadID)
				{
					networkDataGroups[data.GetType()](data);
				}
				else
				{
					networkBuffer.Enqueue(data);
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
		public void Invoke(InternalEventData data)
		{
			if (internalDataGroups.ContainsKey(data.Type) && internalDataGroups[data.Type] != null)
			{
				if (!Client.IsGameBuild || Thread.CurrentThread.ManagedThreadId == Client.MainThreadID)
				{
					internalDataGroups[data.Type](data);
				}
				else
				{
					internalBuffer.Enqueue(data);
				}
			}
			else
			{
				Dbg.Info("Cannot invoke ", data.Type, data);
			}
		}

		/// <summary>
		/// 	Execute code for requests stored in queue.
		/// </summary>
		public void InvokeQueuedData()
		{
			packetCounter.Clear();
			while (networkBuffer.Count > 0)
			{
				NetworkData result;
				if (!networkBuffer.TryDequeue(out result))
				{
					continue;
				}
				NetworkDataDelegate value;
				if (networkDataGroups.TryGetValue(result.GetType(), out value) && value != null)
				{
					value(result);
				}
				if (startCounting)
				{
					Type type = result.GetType();
					if (result is DynamicObjectStatsMessage)
					{
						if (avgPacketCounter.ContainsKey(result.GetType()))
						{
							avgPacketCounter[result.GetType()]++;
						}
						else
						{
							avgPacketCounter.Add(result.GetType(), 1);
						}
					}
					frameCounter += 1f;
				}
				if (packetCounter.ContainsKey(result.GetType()))
				{
					packetCounter[result.GetType()]++;
				}
				else
				{
					packetCounter.Add(result.GetType(), 1);
				}
			}
			if (DebugPacketCount)
			{
				foreach (KeyValuePair<Type, int> item in packetCounter)
				{
				}
				DebugPacketCount = false;
			}
			while (internalBuffer.Count > 0)
			{
				InternalEventData result2;
				InternalDataDelegate value2;
				if (internalBuffer.TryDequeue(out result2) && internalDataGroups.TryGetValue(result2.Type, out value2) && value2 != null)
				{
					value2(result2);
				}
			}
		}
	}
}
