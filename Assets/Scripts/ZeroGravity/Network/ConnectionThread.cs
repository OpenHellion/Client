using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;

namespace ZeroGravity.Network
{
	public sealed class ConnectionThread
	{
		private volatile bool gameSocketReady;

		private volatile bool runThread;

		public Thread connectThread;

		public Thread sendingThread;

		public Thread listeningThread;

		private string gameServerAddress;

		private int gameServerPort;

		private long gameServerID;

		private string gameServerPassword;

		private int retryAttempt;

		private Socket socket;

		private ConcurrentQueue<NetworkData> networkDataQueue;

		private EventWaitHandle waitHandle;

		// TODO: Replace this with a custom generated id, and save this in an external provider.
		private string steamId;

		public void Start(string address, int port, long id, string password, string steamId, int retryAttempt = 3)
		{
			gameServerAddress = address;
			gameServerPort = port;
			gameServerID = id;
			gameServerPassword = password;
			this.steamId = steamId;
			this.retryAttempt = retryAttempt;
			runThread = true;
			networkDataQueue = new ConcurrentQueue<NetworkData>();
			waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

			connectThread = new Thread(ConnectThread);
			connectThread.IsBackground = true;
			connectThread.Start();

			sendingThread = new Thread(SendingThread);
			sendingThread.IsBackground = true;
			sendingThread.Start();
		}

		public void Send(NetworkData data)
		{
			if (gameSocketReady)
			{
				networkDataQueue.Enqueue(data);
				waitHandle.Set();
			}
		}

		public void StopImmediate()
		{
			runThread = false;
			if (waitHandle != null)
			{
				waitHandle.Set();
			}
			if (socket != null)
			{
				socket.Close();
			}
			listeningThread.Interrupt();
			listeningThread.Abort();
			sendingThread.Interrupt();
			sendingThread.Abort();
		}

		public void Stop()
		{
			runThread = false;
			gameSocketReady = false;
			if (waitHandle != null)
			{
				waitHandle.Set();
				socket.Close();
				if (sendingThread != null)
				{
					sendingThread.Interrupt();
				}
				if (listeningThread != null)
				{
					listeningThread.Interrupt();
				}
			}
		}

		private void SendingThread()
		{
			while (Client.IsRunning && runThread)
			{
				waitHandle.WaitOne();
				if (!Client.IsRunning || !runThread)
				{
					break;
				}
				NetworkData result = null;
				while (networkDataQueue.Count > 0)
				{
					if (!networkDataQueue.TryDequeue(out result))
					{
						Client.Instance.NetworkController.EventSystem.Invoke(new EventSystem.InternalEventData(EventSystem.InternalEventType.RemoveLoadingCanvas));
						Dbg.Info("Problem occured while dequeueing network data");
						socket.Close();
						gameSocketReady = false;
						Client.Instance.NetworkController.EventSystem.Invoke(new EventSystem.InternalEventData(EventSystem.InternalEventType.OpenMainScreen));
						return;
					}
					try
					{
						socket.Send(Serializer.Serialize(result));
						Client.Instance.LogSentNetworkData(result.GetType());
					}
					catch (ArgumentNullException)
					{
						Client.Instance.NetworkController.EventSystem.Invoke(new EventSystem.InternalEventData(EventSystem.InternalEventType.RemoveLoadingCanvas));
						Dbg.Error("Serialized data buffer is null", result.GetType().ToString(), result);
					}
					catch (Exception ex2)
					{
						Client.Instance.NetworkController.EventSystem.Invoke(new EventSystem.InternalEventData(EventSystem.InternalEventType.RemoveLoadingCanvas));
						if (!(ex2 is SocketException) && !(ex2 is ObjectDisposedException))
						{
							Dbg.Error("SendToGameServer exception", ex2.Message, ex2.StackTrace);
						}
						socket.Close();
						gameSocketReady = false;
						Client.Instance.NetworkController.EventSystem.Invoke(new EventSystem.InternalEventData(EventSystem.InternalEventType.OpenMainScreen));
						return;
					}
				}
			}
		}

		public void DisconnectImmediate()
		{
			if (gameSocketReady)
			{
				gameSocketReady = false;
				if (socket != null)
				{
					socket.Close();
				}
				networkDataQueue = null;
				waitHandle = null;
				if (connectThread != null)
				{
					connectThread.Abort();
				}
				if (sendingThread != null)
				{
					sendingThread.Abort();
				}
			}
			StopImmediate();
		}

		public void Disconnect()
		{
			if (gameSocketReady)
			{
				gameSocketReady = false;
				if (socket != null)
				{
					socket.Close();
				}
				networkDataQueue = null;
				waitHandle = null;
				if (connectThread != null)
				{
					connectThread.Abort();
				}
				if (sendingThread != null)
				{
					sendingThread.Abort();
				}
			}
			Stop();
		}

		private void ConnectThread()
		{
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			while (retryAttempt > 0)
			{
				try
				{
					IAsyncResult asyncResult = socket.BeginConnect(gameServerAddress, gameServerPort, null, null);
					if (!asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5.0), false))
					{
						socket.Close();
						throw new TimeoutException();
					}
					if (socket.Connected)
					{
						gameSocketReady = true;
						FinalizeConnecting();
						return;
					}
					retryAttempt--;
				}
				catch (TimeoutException)
				{
					retryAttempt--;
					gameSocketReady = false;
					if (retryAttempt == 0)
					{
						FinalizeConnecting();
						Client.Instance.NetworkController.EventSystem.Invoke(new EventSystem.InternalEventData(EventSystem.InternalEventType.ConnectionFailed));
					}
					return;
				}
				catch (Exception ex2)
				{
					Dbg.Error("Error occured while attempting to connect to game server", ex2.Message, ex2.StackTrace);
					FinalizeConnecting();
					return;
				}
			}
			FinalizeConnecting();
			Client.Instance.NetworkController.EventSystem.Invoke(new EventSystem.InternalEventData(EventSystem.InternalEventType.ConnectionFailed));
		}

		private void FinalizeConnecting()
		{
			if (gameSocketReady)
			{
				listeningThread = new Thread(Listen);
				listeningThread.IsBackground = true;
				listeningThread.Start();
				LogInRequest logInRequest = new LogInRequest();
				logInRequest.SteamId = steamId;
				logInRequest.CharacterData = NetworkController.CharacterData;
				logInRequest.ServerID = gameServerID;
				logInRequest.Password = gameServerPassword;
				logInRequest.ClientHash = Client.CombinedHash;
				Send(logInRequest);
			}
			else
			{
				Disconnect();
				Client.Instance.NetworkController.EventSystem.Invoke(new EventSystem.InternalEventData(EventSystem.InternalEventType.ConnectionFailed));
			}
		}

		/// <summary>
		/// 	Thread code that handles listening for data.<br/>
		/// 	Calls <c>EventSystem.Invoke</c>, which in turn refers it to a listener function.
		/// </summary>
		private void Listen()
		{
			while (Client.IsRunning && gameSocketReady && runThread)
			{
				try
				{
					NetworkData networkData = Serializer.ReceiveData(socket);
					if (networkData != null && Client.Instance.NetworkController != null)
					{
						Client.Instance.NetworkController.EventSystem.Invoke(networkData);
						Client.Instance.LogReceivedNetworkData(networkData.GetType());
						continue;
					}
					break;
				}
				catch (Exception ex)
				{
					if (ex is SocketException || (ex.InnerException != null && ex.InnerException is SocketException))
					{
						if (!Client.Instance.LogInResponseReceived)
						{
							Client.Instance.NetworkController.EventSystem.Invoke(new EventSystem.InternalEventData(EventSystem.InternalEventType.ShowMessageBox, Localization.ConnectionError, Localization.TryAgainLater));
						}
						else if (Client.Instance.LogoutRequestSent)
						{
							Dbg.Info("Tried to listen to data, but logout was requested.");
						}
						else
						{
							Dbg.Error("***", ex.Message, ex.StackTrace);
						}
					}
					else
					{
						Dbg.Error("Game server listening thread exception", ex.Message, ex.StackTrace);
					}
					break;
				}
			}
		}
	}
}
