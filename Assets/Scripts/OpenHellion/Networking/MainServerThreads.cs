using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using ZeroGravity;
using ZeroGravity.Helpers;
using ZeroGravity.Network;

namespace OpenHellion.Networking
{
	// TODO: Rewrite this.
	internal sealed class MainServerThreads
	{
		public static string PublicKey;

		public static string PrivateKey;

		private static bool _generatingKeys;

		internal MainServerThreads()
		{
			GenerateNewKeyset();
		}

		private static void GenerateNewKeyset()
		{
			new Thread((ThreadStart)delegate
			{
				if (!_generatingKeys)
				{
					_generatingKeys = true;
					try
					{
						DateTime utcNow = DateTime.UtcNow;
						CryptoHelper.GenerateRSAKeys(out PublicKey, out PrivateKey);
					}
					catch (Exception ex)
					{
						Dbg.Error(ex.Message, ex.StackTrace);
					}
					_generatingKeys = false;
				}
			}).Start();
		}

		/// <summary>
		/// 	Send a request to the main server with data.
		/// </summary>
		public void Send(NetworkData data)
		{
			ThreadPool.QueueUserWorkItem(MServerComm, data);
		}

		// Send a request to the main server with data.
		private void MServerComm(object rawData)
		{
			NetworkData data = rawData as NetworkData;
			int maxAttempts = 5;
			for (int i = 1; i <= maxAttempts; i++)
			{
				// Create a socket.
				Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				try
				{
					// Check connection to socket.
					IAsyncResult asyncResult = socket.BeginConnect(NetworkController.MainServerAddress, NetworkController.MainServerPort, null, null);
					if (!asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5.0), exitContext: false))
					{
						throw new TimeoutException();
					}

					// Create a stream.
					NetworkStream stream = new NetworkStream(socket);
					string remotePublicKey = CryptoHelper.ExchangePublicKeys(stream, PublicKey);

					// Convert the class to binary
					byte[] dataBinary = Serializer.Serialize(data);

					// Send data.
					CryptoHelper.WriteRequest(stream, dataBinary, remotePublicKey);

					// Read response.
					byte[] buffer = CryptoHelper.ReadResponse(stream, PrivateKey);
					stream.Close();

					// Decrypted data.
					NetworkData finalData = Serializer.ReceiveData(new MemoryStream(buffer));
					if (finalData != null)
					{
						EventSystem.Invoke(finalData);
					}
				}
				catch (TimeoutException)
				{
					if (i >= maxAttempts)
					{
						EventSystem.Invoke(new EventSystem.InternalEventData(EventSystem.InternalEventType.ShowMessageBox, Localization.ConnectionError, Localization.UnableToConnectToMainServer));
					}
					continue;
				}
				catch (Exception ex)
				{
					if (i >= maxAttempts)
					{
						Client.Instance.SignInFailed = true;
						Dbg.Error("Error occured while attempting to connect to main server", ex.Message, ex.StackTrace);
					}
					continue;
				}
				finally
				{
					socket.Close();
				}
				break;
			}
			new Thread(GenerateNewKeyset).Start();
		}

		/// <summary>
		/// 	TODO: Checks if the server is valid.
		/// </summary>
		private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}
	}
}
