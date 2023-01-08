using System.Security.Cryptography.X509Certificates;
using OpenHellion.Networking.Message.MainServer;
using OpenHellion.Networking.Message;
using UnityEngine.Networking;
using ZeroGravity;

namespace OpenHellion.Networking
{
	// TODO: Use HTTPS.
	public class MainServer
	{
		public const string IpAddress = "localhost";
		public const ushort Port = 6000;

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
						break;
					case UnityWebRequest.Result.ProtocolError:
						Dbg.Error("MainServer: HTTP Error: " + operation.webRequest.error);
						break;
					case UnityWebRequest.Result.InProgress:
						Dbg.Error("MainServer: HTTP Error: Tried to fetch data before connection has finished.");
						break;
					case UnityWebRequest.Result.Success:
						Dbg.Log(operation.webRequest.downloadHandler.text);
						callback(Json.Deserialize<T>(operation.webRequest.downloadHandler.text));
						break;
				}
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
						break;
					case UnityWebRequest.Result.ProtocolError:
						Dbg.Error("MainServer: HTTP Error: " + operation.webRequest.error);
						break;
					case UnityWebRequest.Result.InProgress:
						Dbg.Error("MainServer: HTTP Error: Tried to fetch data before connection has finished.");
						break;
					case UnityWebRequest.Result.Success:
						Dbg.Log("Result: " + operation.webRequest.downloadHandler.text);
						callback(Json.Deserialize<T>(operation.webRequest.downloadHandler.text));
						break;
				}

				request.Dispose();
			};
		}
	}
}
