using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using ZeroGravity.Helpers;

namespace ZeroGravity.Network
{
	public class MainServerThreads
	{
		public static string PublicKey;

		public static string PrivateKey;

		private static bool _generatingKeys;

		static MainServerThreads()
		{
			PublicKey = "<RSAKeyValue><Modulus>77pMQaLVNBRiSmMjAM6wo3CnnFJzzVTytNb3W8MbGM7Etneh3JI4fFurNjNsaXvthmv5MKqjmXW1+pRi4y7adTOzKzxf4rbc4wnQNSxWEY+UWfv+xxbmglW0M7Gi2jYjvrCw9Vw0MlFssLMECZgiHXveCWkR8srQ3oIJbhV4U/RAGY3M9ah1pP5prjdWyB549+9wZb9k2JB8Bnrs1lpGAEYFVwC33qNwsqV8zxsBDKAld+62ip5Bl1ZDpIVAC6qVBBgO1Lbu505IcM6VojRl3HF1RJfXG6vTEg9MFUPGE1lg3Htfx146FayeOe+daa3iB7nv2hChpbxptuOzh6B3Nw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
			PrivateKey = "<RSAKeyValue><Modulus>77pMQaLVNBRiSmMjAM6wo3CnnFJzzVTytNb3W8MbGM7Etneh3JI4fFurNjNsaXvthmv5MKqjmXW1+pRi4y7adTOzKzxf4rbc4wnQNSxWEY+UWfv+xxbmglW0M7Gi2jYjvrCw9Vw0MlFssLMECZgiHXveCWkR8srQ3oIJbhV4U/RAGY3M9ah1pP5prjdWyB549+9wZb9k2JB8Bnrs1lpGAEYFVwC33qNwsqV8zxsBDKAld+62ip5Bl1ZDpIVAC6qVBBgO1Lbu505IcM6VojRl3HF1RJfXG6vTEg9MFUPGE1lg3Htfx146FayeOe+daa3iB7nv2hChpbxptuOzh6B3Nw==</Modulus><Exponent>AQAB</Exponent><P>8pU8xvuhnV1OUlaLoIVvnzlLuNrfnsJC/b3q76pMTyFvGmMyYrlSs+pRq0oxei7ZrW2EQgCzCV6FruF+T02s/75lKdx5IAYbVz72iHKVUabN2j/tWj1/vPaH1ddris60htSeXw1nmdXBkPnmeeaPg+j7MygLQMe3Y/Ft8PM1v8M=</P><Q>/PyiA7BKEHTWJ9xzzqEwtSGU3l9Q4b5Rv7tUo2PWe7b28G/Vr1XUTEUm/hIaHdkwZWaJD5AvUzITm1CVqZVJhnHUB5JKXN8lfZJdaZ+wh0J5DxJiOhcqMOlQWuxYxY/Ur+6UDB84fi9HgBGUyZMJ1RoUbhhjJs41fC/hj4aWh30=</Q><DP>6MkBe8l4+3PQdv8KGk4yIU9wNMIEmWq9spWPX9HCTKU+4smSTl6c2H3hRUh7Vk6jIOPZaSUWqcUE8B7/gMZ/XyOsHJGvwmVZ001ecu3SEHrLS8bQ1Cnz7Ld+/lwsnRVi84gROhG4+0y+5YqJ+yyfR+xJMnwY4F9GwyepYvFhANU=</DP><DQ>Jf/8hflQ5q/mCw6+BmeH3W0x40UF7SKdPONyjyhL60FMTkk+G5wUy3AHr1oguiMJdfG03LWXpjN7ZnTwRgy6gotOgeSjbtfF1drKVTc7WNXPQUhNoNQh7j7dZwrhOqukEtkPO9LktW98mewKIU0IyQ2ly/O10PpgZkr64oggzDE=</DQ><InverseQ>hM4NqpxKtMKMfRlFtTUTa/tduTffLhVA9Qs7cqO18CeyfEjmZez98vuIHaLp2hIBNyQrM9a9JMLThdfsshYosvXu68Ug9XoIBdSsWckgoVsqft3H/oW7SO5vPzLfqVvxyOu6JNcC39Q5jIgUCRPfy2O9nYypCcMop0LquP49zw8=</InverseQ><D>FloB6yrCAzmVUoc8x8H9wTdrQU2Ew5PUj1ztsk4WNDFvMO4lltnBT4MCiyqvRISMCHj/wnRejZvmjEcSk0kWkE/yOClIbWyMWtTa52JLYhbHh75RqoFtm6BfDWTWIM9kdM6nqTDgIwLphe4jBbv8DlCyyqDTzhXv9JceRvfUgMs6i/Qa3AIwhW3RgOUdATcwNb6hLAkGLFfqE+QBQoQ9xOGCy5Q+7YIWQXzH+E/zZDyT31jQH+LPf1nWTj/WmSp0qXeBoBRbQGq9qZR3NJ/S34dLhhsLst9v3cc6klY2hNXV3wao29a3eWKucrLhgduATQGOE5AHJts8YkhvnC35DQ==</D></RSAKeyValue>";
			generateNewKeyset();
		}

		private static void generateNewKeyset()
		{
			new Thread((ThreadStart)delegate
			{
				if (!_generatingKeys)
				{
					_generatingKeys = true;
					try
					{
						DateTime utcNow = DateTime.UtcNow;
						CryptoHelper.GenerateRSAKeys(out var text, out var text2);
						PublicKey = text;
						PrivateKey = text2;
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

		/// <summary>
		/// 	Send a request to the main server with data.
		/// </summary>
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
					IAsyncResult asyncResult = socket.BeginConnect(NetworkController.MainServerAddres, NetworkController.MainServerPort, null, null);
					if (!asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5.0), exitContext: false))
					{
						throw new TimeoutException();
					}

					// Create a stream.
					NetworkStream stream = new NetworkStream(socket);
					string remotePublicKey = CryptoHelper.ExchangePublicKeys(stream, PublicKey);

					// Convert the data to binary
					byte[] dataBinary = Serializer.Serialize(data);

					// Request data.
					CryptoHelper.WriteRequest(stream, dataBinary, remotePublicKey);
					byte[] buffer = CryptoHelper.ReadResponse(stream, PrivateKey);
					stream.Close();

					// Decrypted data.
					NetworkData finalData = Serializer.ReceiveData(new MemoryStream(buffer));
					if (finalData != null)
					{
						Client.Instance.NetworkController.EventSystem.Invoke(finalData);
					}
				}
				catch (TimeoutException)
				{
					if (i >= maxAttempts)
					{
						Client.Instance.NetworkController.EventSystem.Invoke(new EventSystem.InternalEventData(EventSystem.InternalEventType.ShowMessageBox, Localization.ConnectionError, Localization.UnableToConnectToMainServer));
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
			new Thread(generateNewKeyset).Start();
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
