using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZeroGravity.Network;

namespace OpenHellion.Net
{
	public static class EventSystem
	{
		public delegate void NetworkListenerDelegate(NetworkData data);

		private static readonly ConcurrentDictionary<Type, NetworkListenerDelegate> _networkDataListeners = new();
		private static readonly ConcurrentDictionary<Type, Func<NetworkData, UniTask<NetworkData>>> _syncRequestListeners = new();

		private static readonly ConcurrentQueue<NetworkData> _networkBuffer = new ConcurrentQueue<NetworkData>();

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

		/// <summary>
		/// 	Add listener for custom events.
		/// </summary>
		public static void AddListener(Type group, NetworkListenerDelegate function)
		{
			var result = _networkDataListeners.GetOrAdd(group, function);

			if (result is not null)
			{
				_networkDataListeners[group] += function;
			}
		}

		/// <summary>
		/// 	Add listener for sync events.
		/// </summary>
		/// <remarks>
		/// 	May call functions on non-main thread.
		/// </remarks>
		public static void AddSyncRequestListener(Type group, Func<NetworkData, UniTask<NetworkData>> function)
		{
			var result = _syncRequestListeners.GetOrAdd(group, function);

			if (result is not null)
			{
				_syncRequestListeners[group] += function;
			}
		}

		/// <summary>
		/// 	Remove listener for custom events.
		/// </summary>
		public static void RemoveListener(Type group, NetworkListenerDelegate function)
		{
			try
			{
				_networkDataListeners[group] -= function;
			}
			catch (KeyNotFoundException e)
			{
				Debug.LogFormat("Tried to remove listener but it doesn't exist: {0}", e.StackTrace);
			}
		}

		/// <summary>
		/// 	Remove listener for sync events.
		/// </summary>
		/// <remarks>
		/// 	May call functions on non-main thread.
		/// </remarks>
		public static void RemoveSyncRequestListener(Type group, Func<NetworkData, UniTask<NetworkData>> function)
		{
			try
			{
				_syncRequestListeners[group] -= function;
			}
			catch (KeyNotFoundException e)
			{
				Debug.LogFormat("Tried to remove listener but it doesn't exist: {0}", e.StackTrace);
			}
		}

		/// <summary>
		/// 	Execute corresponding code for request.
		/// </summary>
		internal static void Invoke(NetworkData data)
		{
			if (_networkDataListeners.ContainsKey(data.GetType()) && _networkDataListeners[data.GetType()] != null)
			{
				if (Thread.CurrentThread.ManagedThreadId == World.MainThreadID)
				{
					_networkDataListeners[data.GetType()](data);
				}
				else
				{
					_networkBuffer.Enqueue(data);
				}
			}
			else
			{
				Debug.LogError("Listener is not registered for data:" + data.GetType() + data);
			}
		}

		internal static UniTask<NetworkData> InvokeSyncRequest(NetworkData data)
		{
			return _syncRequestListeners[data.GetType()](data);
		}

		/// <summary>
		/// 	Execute code for requests stored in queue.
		/// </summary>
		internal static void InvokeQueuedData()
		{
			_packetCounter.Clear();
			while (_networkBuffer.Count > 0)
			{
				if (!_networkBuffer.TryDequeue(out NetworkData result))
				{
					continue;
				}

				if (_networkDataListeners.TryGetValue(result.GetType(), out NetworkListenerDelegate value) && value != null)
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
		}
	}
}
