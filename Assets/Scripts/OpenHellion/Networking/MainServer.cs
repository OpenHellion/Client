using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using OpenHellion.Networking.MainServerMessage;
using UnityEngine;
using UnityEngine.Networking;
using ZeroGravity;
using ZeroGravity.Network;

namespace OpenHellion.Networking
{
	// TODO: Use HTTPS.
	public class MainServer : MonoBehaviour
	{
		public const string IpAddress = "localhost";
		public const ushort Port = 6000;

		public static string Address {
			get {
				return IpAddress + ":" + Port;
			}
		}

		public delegate void SendCallback(DataContainer data);

		/// <summary>
		/// 	Send a message to the main server.
		/// </summary>
		public static void SendMessage(NetworkMessage data, SendCallback callback)
		{
			UnityWebRequestAsyncOperation operation = UnityWebRequest.Post(
				Address + "/api/" + data.GetDestination(),
				data.ToString()).SendWebRequest();

            operation.completed += (asyncOperation) =>
			{
				switch (operation.webRequest.result)
				{
					case UnityWebRequest.Result.ConnectionError:
					case UnityWebRequest.Result.DataProcessingError:
						Dbg.Error("MainServer: Error: " + operation.webRequest.error);
						break;
					case UnityWebRequest.Result.ProtocolError:
						Dbg.Error("MainServer: HTTP Error: " + operation.webRequest.error);
						break;
					case UnityWebRequest.Result.InProgress:
						Dbg.Error("MainServer: HTTP Error: Tried to fetch data before connection has finished.");
						break;
					case UnityWebRequest.Result.Success:
						callback(Json.Deserialize<DataContainer>(operation.webRequest.downloadHandler.text));
						break;
				}
			};
		}
	}
}
