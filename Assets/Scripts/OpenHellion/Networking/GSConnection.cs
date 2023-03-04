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
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using OpenHellion.ProviderSystem;
using OpenHellion.IO;
using ZeroGravity;
using ZeroGravity.Network;

namespace OpenHellion.Networking
{
	/// <summary>
	/// 	Handles connecting to a game server.
	/// </summary>
	internal sealed class GSConnection
	{
		private string m_serverIp;

		private int m_serverPort;

		private string m_serverId;

		private string m_serverPassword;

		private Telepathy.Client m_client;

		/// <summary>
		/// 	Establish a connection to a specified game server.<br />
		/// 	When connection is established, a login request is sent.
		/// </summary>
		internal void Connect(string ip, int port, string serverId, string password)
		{
			Telepathy.Log.Info = Dbg.Info;
			Telepathy.Log.Warning = Dbg.Warning;
			Telepathy.Log.Error = Dbg.Error;

			m_serverIp = ip;
			m_serverPort = port;
			m_serverId = serverId;
			m_serverPassword = password;

			m_client = new(80000)
			{
				OnConnected = OnConnected,
				OnData = OnData,
				OnDisconnected = OnDisconnected,
				SendQueueLimit = 1000,
				ReceiveQueueLimit = 1000
			};

			m_client.Connect(ip, port);
		}

		internal int Tick()
		{
			return m_client.Tick(50);
		}

		/// <summary>
		/// 	Send network data to the server.
		/// </summary>
		internal void Send(NetworkData data)
		{
			if (!m_client.Connected)
			{
				EventSystem.Invoke(new EventSystem.InternalEventData(EventSystem.InternalEventType.OpenMainScreen));
				Dbg.Log("Tried to send data when not connected to any server.");
				return;
			}

			try
			{
				// Package data.
				ArraySegment<byte> binary = new(ProtoSerialiser.Package(data));

				// Send data to server.
				m_client.Send(binary);
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
			m_client.Disconnect();
		}

		// Executed when we connect to a server.
		private void OnConnected()
		{
			LogInRequest logInRequest = new LogInRequest
			{
				PlayerId = NetworkController.PlayerId,
				NativeId = ProviderManager.NativeId,
				CharacterData = NetworkController.CharacterData,
				ServerID = m_serverId,
				Password = m_serverPassword,
				ClientHash = Client.CombinedHash
			};

			Send(logInRequest);

			Dbg.Log("Established connection to server.");
		}


		// Handles a network package.
		private void OnData(ArraySegment<byte> message)
		{
			try
			{
				NetworkData networkData = ProtoSerialiser.Unpackage(new MemoryStream(message.Array));
				if (networkData != null)
				{
					EventSystem.Instance.Invoke(networkData);
					NetworkController.LogReceivedNetworkData(networkData.GetType());
				}
			}
			catch (Exception ex)
			{
				if (!Client.Instance.LogInResponseReceived)
				{
					EventSystem.Invoke(new EventSystem.InternalEventData(EventSystem.InternalEventType.ShowMessageBox, Localization.ConnectionError, Localization.TryAgainLater));
					Dbg.Error(ex.Message, ex.StackTrace);
					Disconnect();
				}
				else if (Client.Instance.LogoutRequestSent)
				{
					Dbg.Log("Tried to listen to data, but logout was requested.");
				}
			}
		}

		// Executed when we disconnect from a server.
		private void OnDisconnected()
		{
			if (Client.Instance.LogoutRequestSent)
			{
				EventSystem.Invoke(new EventSystem.InternalEventData(EventSystem.InternalEventType.OpenMainScreen));
			}

			Dbg.Log("Client disconnected from server.");
		}
	}
}
