// GSConnection.cs
//
// Copyright (C) 2023, OpenHellion contributors
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using OpenHellion.IO;
using UnityEngine;
using OpenHellion.UI;
using ZeroGravity;
using ZeroGravity.Network;

namespace OpenHellion.Net
{
	/// <summary>
	/// 	Handles connecting to a game server.
	/// </summary>
	internal sealed class GsConnection
	{
		private string _serverId;

		private Telepathy.Client _client;

		private string _userId;

		/// <summary>
		/// 	Establish a connection to a specified game server.<br />
		/// 	When connection is established, a login request is sent.
		/// </summary>
		internal void Connect(string ip, int port, string serverId, string userId)
		{
			Telepathy.Log.Info = Dbg.Info;
			Telepathy.Log.Warning = Dbg.Warning;
			Telepathy.Log.Error = Dbg.Error;

			_serverId = serverId;
			_userId = userId;

			_client = new(100000)
			{
				OnConnected = OnConnected,
				OnData = OnData,
				OnDisconnected = OnDisconnected,
				SendQueueLimit = 1000,
				ReceiveQueueLimit = 1000
			};

			_client.Connect(ip, port);
		}

		internal int Tick()
		{
			return _client.Tick(50);
		}

		/// <summary>
		/// 	Send network data to the server.
		/// </summary>
		internal async void Send(NetworkData data)
		{
			if (!_client.Connected)
			{
				EventSystem.Invoke(new EventSystem.InternalEventData(EventSystem.InternalEventType.OpenMainScreen));
				Dbg.Log("Tried to send data when not connected to any server.");
				return;
			}

			try
			{
				// Package data.
				ArraySegment<byte> binary = new(await ProtoSerialiser.Pack(data));

				// Send data to server.
				if (binary.Count <= _client.MaxMessageSize)
				{
					_client.Send(binary);
				}
				else
				{
					Dbg.Warning("Packet:", data.GetType(), "was too large to send.");
				}

				NetworkController.LogSentNetworkData(data.GetType());
			}
			catch (Exception ex)
			{
				Dbg.Error("Error when sending data", ex.Message, ex.StackTrace);
			}
		}

		/// <summary>
		/// 	Disconnect from server.
		/// </summary>
		internal void Disconnect()
		{
			_client.Disconnect();
		}

		// Executed when we connect to a server.
		private void OnConnected()
		{
			LogInRequest logInRequest = new LogInRequest
			{
				PlayerId = _userId,
				CharacterData = NetworkController.CharacterData,
				ServerID = _serverId,
				ClientHash = Globals.CombinedHash
			};

			Send(logInRequest);

			Dbg.Log("Established connection with server.");
		}


		// Handles a network package.
		private async void OnData(ArraySegment<byte> message)
		{
			try
			{
				Debug.Assert(message.Array != null);
				NetworkData networkData = await ProtoSerialiser.Unpack(new MemoryStream(message.Array));
				if (networkData != null)
				{
					EventSystem.Instance.Invoke(networkData);
					NetworkController.LogReceivedNetworkData(networkData.GetType());
				}
			}
			catch (Exception ex)
			{
				GlobalGUI.ShowErrorMessage(Localization.ConnectionError, Localization.TryAgainLater, null);
				Dbg.Error(ex.Message, ex.StackTrace);
			}
		}

		// Executed when we disconnect from a server.
		private void OnDisconnected()
		{
			/*if (_world.LogoutRequestSent)
			{
				EventSystem.Invoke(new EventSystem.InternalEventData(EventSystem.InternalEventType.OpenMainScreen));
			}*/

			Dbg.Log("Client disconnected from server.");
		}
	}
}
