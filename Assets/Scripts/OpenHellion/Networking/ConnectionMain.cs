// ConnectionMain.cs
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
using System.Security.Cryptography.X509Certificates;
using OpenHellion.Networking.Message.MainServer;
using OpenHellion.IO;
using UnityEngine.Networking;

namespace OpenHellion.Networking
{
	// TODO: Use HTTPS.
	/// <summary>
	/// 	Handles connecting to the main server.
	/// </summary>
	public class ConnectionMain
	{
		public const string IpAddress = "localhost";
		public const ushort Port = 6001;

		public static string Address {
			get {
				return IpAddress + ":" + Port;
			}
		}

		public delegate void SendCallback<T>(T data);

		/// <summary>
		/// 	Send a message to the main server.
		/// </summary>
		public static void Post<T>(MSMessage data, SendCallback<T> callback)
		{
			UnityWebRequest request = new(Address + "/api/" + data.GetDestination(), "POST")
			{
				uploadHandler = new UploadHandlerRaw(new System.Text.UTF8Encoding().GetBytes(data.ToString()))
			};

			request.SetRequestHeader("Content-Type", "application/json");

			UnityWebRequestAsyncOperation operation = request.SendWebRequest();

			operation.completed += (asyncOperation) =>
			{
				switch (operation.webRequest.result)
				{
					case UnityWebRequest.Result.ConnectionError:
					case UnityWebRequest.Result.DataProcessingError:
						Dbg.Error("MainServer: Error: " + operation.webRequest.error);
						throw new Exception();
					case UnityWebRequest.Result.ProtocolError:
						Dbg.Error("MainServer: HTTP Error: " + operation.webRequest.error);
						throw new Exception();
					case UnityWebRequest.Result.InProgress:
						Dbg.Error("MainServer: HTTP Error: Tried to fetch data before connection has finished.");
						throw new Exception();
					case UnityWebRequest.Result.Success:
						Dbg.Log(operation.webRequest.downloadHandler.text);
						callback(JsonSerialiser.Deserialize<T>(operation.webRequest.downloadHandler.text));
						break;
				}

				request.Dispose();
			};
		}

		/// <summary>
		/// 	Send a request to get data from the main server.
		/// </summary>
		public static void Get<T>(MSMessage data, SendCallback<T> callback)
		{
			Dbg.Log(data.GetDestination() + ": " + data.ToString());

			UnityWebRequest request = UnityWebRequest.Get(Address + "/api/" + data.GetDestination());

			request.uploadHandler = new UploadHandlerRaw(new System.Text.UTF8Encoding().GetBytes(data.ToString()));
			request.SetRequestHeader("Content-Type", "application/json");

			UnityWebRequestAsyncOperation operation = request.SendWebRequest();

			operation.completed += (asyncOperation) =>
			{
				switch (operation.webRequest.result)
				{
					case UnityWebRequest.Result.ConnectionError:
					case UnityWebRequest.Result.DataProcessingError:
						Dbg.Error("MainServer: Error: " + operation.webRequest.error);
						throw new Exception();
					case UnityWebRequest.Result.ProtocolError:
						Dbg.Error("MainServer: HTTP Error: " + operation.webRequest.error);
						throw new Exception();
					case UnityWebRequest.Result.InProgress:
						Dbg.Error("MainServer: HTTP Error: Tried to fetch data before connection has finished.");
						throw new Exception();
					case UnityWebRequest.Result.Success:
						Dbg.Log("Result: " + operation.webRequest.downloadHandler.text);
						callback(JsonSerialiser.Deserialize<T>(operation.webRequest.downloadHandler.text));
						break;
				}

				request.Dispose();
			};
		}
	}
}
