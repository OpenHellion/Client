using System;
using System.Collections.Concurrent;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZeroGravity.Network;

namespace OpenHellion.Net
{
	/// <summary>
	/// 	Registry of listeners and dispatcher for received network data.
	/// </summary>
	public static class EventSystem
	{
		private static readonly ConcurrentDictionary<Type, Action<NetworkData>> _networkDataListeners = new();
		private static readonly ConcurrentDictionary<Type, Func<NetworkData, UniTask<NetworkData>>> _syncRequestListeners = new();

		/// <summary>
		/// 	Add listener for custom events.
		/// </summary>
		public static void AddListener(Type group, Action<NetworkData> function)
		{
			if (_networkDataListeners.ContainsKey(group))
			{
				_networkDataListeners[group] += function;
			}
			else
			{
				_networkDataListeners[group] = function;
			}
		}

		/// <summary>
		/// 	Add listener for custom events.
		/// </summary>
		public static void AddListener<T>(Action<NetworkData> function)
		{
			if (_networkDataListeners.ContainsKey(typeof(T)))
			{
				_networkDataListeners[typeof(T)] += function;
			}
			else
			{
				_networkDataListeners[typeof(T)] = function;
			}
		}

		/// <summary>
		/// 	Add listener for sync events.
		/// </summary>
		public static void AddSyncRequestListener(Type group, Func<NetworkData, UniTask<NetworkData>> function)
		{
			if (_syncRequestListeners.ContainsKey(group))
			{
				_syncRequestListeners[group] += function;
			}
			else
			{
				_syncRequestListeners[group] = function;
			}
		}

		/// <summary>
		/// 	Remove listener for custom events.
		/// </summary>
		public static void RemoveListener(Type group, Action<NetworkData> function)
		{
			if (!_networkDataListeners.TryGetValue(group, out Action<NetworkData> listeners))
			{
				return;
			}

			listeners -= function;
			if (listeners == null)
			{
				_networkDataListeners.TryRemove(group, out _);
			}
			else
			{
				_networkDataListeners[group] = listeners;
			}
		}
		
		public static void RemoveListener<T>(Action<NetworkData> function)
		{
			RemoveListener(typeof(T), function);
		}

		/// <summary>
		/// 	Remove listener for sync events.
		/// </summary>
		public static void RemoveSyncRequestListener(Type group, Func<NetworkData, UniTask<NetworkData>> function)
		{
			if (!_syncRequestListeners.TryGetValue(group, out Func<NetworkData, UniTask<NetworkData>> handlers))
			{
				return;
			}

			handlers -= function;
			if (handlers == null)
			{
				_syncRequestListeners.TryRemove(group, out _);
			}
			else
			{
				_syncRequestListeners[group] = handlers;
			}
		}

		/// <summary>
		/// 	Invoke the listener registered for a received message. Must be called on the main thread.
		/// </summary>
		internal static void Dispatch(NetworkData data)
		{
			if (_networkDataListeners.TryGetValue(data.GetType(), out Action<NetworkData> listener))
			{
				listener(data);
			}
			else
			{
				Debug.LogError("Listener is not registered for data:" + data.GetType() + data);
			}
		}

		/// <summary>
		/// 	Invoke the handler for a server-initiated sync request and produce its response.
		/// </summary>
		internal static UniTask<NetworkData> InvokeSyncRequest(NetworkData data)
		{
			if (_syncRequestListeners.TryGetValue(data.GetType(), out Func<NetworkData, UniTask<NetworkData>> handler))
			{
				return handler(data);
			}

			Debug.LogError("Sync request listener was not registered for data:" + data.GetType() + data);
			return UniTask.FromResult<NetworkData>(null);
		}
	}
}
