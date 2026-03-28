using Geecku.DefaultNetworking.Common;
using Geecku.DefaultNetworking.Common.MessageHandlers;
using Geecku.GlobalMangers;
using NUnit.Framework.Internal;
using Riptide;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Geecku.DefaultNetworking
{
    /// <summary>
    /// Handles and manages all client related traffic.  
    /// </summary>
    public class ClientManager : NetworkPeer
    {
        protected Client Client => TCPClient;
        protected Client TCPClient => TCPPeer as Client;
        protected Client UDPClient => UDPPeer as Client;

        public ClientMessageHandler MsgHandler => Messages as ClientMessageHandler;

        public short Ping => TCPClient != null ? TCPClient.RTT : (short)-1;
        public override bool IsReadyForPeer => TCPClient != null && TCPClient.IsNotConnected;
        public override bool IsInConnection => TCPClient != null && TCPClient.IsConnected;
        public bool IsHost => Network._Server.IsActive;

        #region GetVariables
        public int CurPlayers => -1;
        #endregion

        /// <summary>
        /// Contains last connected valid IP.
        /// </summary>
        private string LastIP;

        public override void Init()
        {
            switch (Network.ProtocolType)
            {
                case ProtocolTypes.Double:
                    TCPPeer = new Client(new Riptide.Transports.Tcp.TcpClient(), "<color=#4C8691>TCP-CLIENT</color>");
                    UDPPeer = new Client(new Riptide.Transports.Udp.UdpClient(), "<color=#4C8691>UDP-CLIENT</color>");
                    break;
                default:    //- ProtocolTypes.Single
                    TCPPeer = new Client("<color=#4C8691>CLIENT</color>");
                    break;
            }

            TCPClient.ClientConnected += TCPClient_ClientConnected;
            TCPClient.ClientDisconnected += TCPClient_ClientDisconnected;
            TCPClient.Connected += TCPClient_Connected;
            TCPClient.Disconnected += TCPClient_Disconnected;
            TCPClient.ConnectionFailed += TCPClient_ConnectionFailed;
            //TCPClient.MessageReceived += TCPClient_MessageReceived;
            gameObject.SetActive(false);
        }

        public override void StartPeer()
        {
            Message msg = null;

            gameObject.SetActive(true);
            Messages.ResetHandler();
            LastIP = Network.IP_Address;
            OnBeforeStartPeer();
            Engine.LogClient("Connecting to " + LastIP + ":" + Network.TCP_Port + "...");
            if (!TCPClient.Connect(LastIP + ":" + Network.TCP_Port, message: msg))
            {
                Debug.LogError("Client could not make a connection to server. (" + LastIP + ":" + Network.TCP_Port + ")");
                gameObject.SetActive(false);
                UpdateName();
                return;
            }
            UpdateName();
        }
        public override void StopPeer()
        {
            gameObject.SetActive(false);
            Messages.ResetHandler();
            switch (Network.ProtocolType)
            {
                case ProtocolTypes.Double:
                    if (TCPClient != null && TCPClient.IsConnected)
                        TCPClient.Disconnect();
                    if (UDPClient != null && UDPClient.IsConnected)
                        UDPClient.Disconnect();
                    break;
                default:    //- ProtocolTypes.Single
                    if (TCPClient != null && TCPClient.IsConnected)
                        TCPClient.Disconnect();
                    break;
            }
            ClearClientDic();
            //Engine.LogClient("Client disconnected from " + LastIP);
            UpdateName();
        }

        protected virtual void FixedUpdate()
        {
            if (TCPClient == null)
                return;
            switch (Network.ProtocolType)
            {
                case ProtocolTypes.Double:
                    TCPClient.Update();
                    UDPClient.Update();
                    break;
                default:    //- ProtocolTypes.Single
                    TCPClient.Update();
                    break;
            }
            UpdateName();
            UpdateClientVis();
        }

        protected override void UpdateName()
        {
            if (TCPClient == null || TCPClient.IsNotConnected)
            {
                BaseTransform.name = "Client [not connected]";
                return;
            }
            BaseTransform.name = "Local Client [" + Ping + "ms]";
        }

        #region Send Messages
        public virtual void Send(Message msg)
        {
            msg.Send(TCPClient);
        }
        #endregion

        #region Client
        [HideInInspector] public ClientInfo LocalClient;
        #endregion

        #region EventHandlers
        //- Foreign connect Events
        public event EventHandler<ClientConnectedEventArgs> OnClientConnected;
        public event EventHandler<ClientDisconnectedEventArgs> OnClientDisconnected;
        public event EventHandler<EventArgs> OnConnected;
        public event EventHandler<DisconnectedEventArgs> OnDisconnected;
        public event EventHandler<ConnectionFailedEventArgs> OnFailedConnection;

        //- Connect Events
        public event EventHandler OnReceiveServerHello;
        public event EventHandler<bool> OnReceiveClientReady;
        public event EventHandler<bool> OnReceiveInitialDownloadConfirmed;

        public void OnReceiveServerHelloWrapper() => OnReceiveServerHello?.Invoke(this, null);
        public void OnReceiveClientReadyWrapper(bool has_inital_download) => OnReceiveClientReady?.Invoke(this, has_inital_download);
        public void OnReceiveInitialDownloadConfirmedWrapper(bool had_initial_download) => OnReceiveInitialDownloadConfirmed?.Invoke(this, had_initial_download);
        
        #endregion

        #region Events
        //- Local Connect/Disconnect
        /// <summary>
        /// Gets called when this client established a successfull connection to the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TCPClient_Connected(object sender, System.EventArgs e)
        {
            LocalClient = new ClientInfo(TCPClient.Id);
            Engine.LogClient("Connected successfully.");
            OnConnected?.Invoke(this, e);
        }
        /// <summary>
        /// Gets called when this client disconnects from the server for any reason.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TCPClient_Disconnected(object sender, DisconnectedEventArgs e)
        {
            LogDisconnectReason(e.Reason);
            OnDisconnected?.Invoke(this, e);
            StopPeer();
        }
        //- Global Connect/Disconnect
        /// <summary>
        /// Gets called when another client connects successfully to the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TCPClient_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            Engine.LogClient("Client [" + e.Id + "] connected.");
            
            AddClientInfo(e.Id);
            AddClientVis(e.Id);
            OnClientConnected?.Invoke(this, e);
        }
        /// <summary>
        /// Gets called when another client disconnects from the server for any reason.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TCPClient_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            Engine.LogClient("Client [" + e.Id + "] disconnected.");

            OnClientDisconnected?.Invoke(this, e);
            RemoveClientVis(e.Id);
            Clients.Remove(e.Id);
        }
        //- Local Failed/Received
        /// <summary>
        /// Gets called when this client fails to establish a connection to the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TCPClient_ConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            Engine.LogClient("Connection Failed: " + e.Reason);
            OnFailedConnection?.Invoke(this, e);
            StopPeer();
        }
        private void TCPClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Engine.LogClient("MsgReceived: " + e.MessageId);
        }
        #endregion

        #region Download
        private static List<int> ReceievedInitialDownloadIDList = new();
        public static List<int> FinalInitialDownloadIDList = new();
        public static int MessageIndexOnConnect;
        [MessageHandler(NetworkManager.InitialDownloadTransfer_MsgID)]
        public static void Receive_InitialDownloadTransfer(Message local)
        {
            LinkedMessage.HandleIncomingMessage(local, (LinkedMessage msg) =>
            {
                var download_id = msg.GetInt();
                var download_length = msg.GetInt();
                for (int i = 0; i < download_length; i++)
                {
                    NetworkManager.HandleAddSyncObjectTransferAction(msg);
                }
                ReceievedInitialDownloadIDList.Add(download_id);
                CheckReceivedInitialDownloads();
            });
        }
        public static void CheckReceivedInitialDownloads()
        {
            if (FinalInitialDownloadIDList == null || FinalInitialDownloadIDList.Count == 0)
                return;

            var manager = NetworkManager.Client;
            bool is_done = false;
            if (ReceievedInitialDownloadIDList.Count == FinalInitialDownloadIDList.Count)                
            {
                is_done = true;
                for (int i = 0; i < ReceievedInitialDownloadIDList.Count; i++)
                    if (!FinalInitialDownloadIDList.Contains(ReceievedInitialDownloadIDList[i]))
                    {
                        is_done = false;
                        break;
                    }
            }

            if (is_done)
            {
                ReceievedInitialDownloadIDList = null;
                FinalInitialDownloadIDList = null;

                manager.MsgHandler.SetClientReady(MessageIndexOnConnect);
                manager.OnReceiveInitialDownloadConfirmedWrapper(true);
            }
        }
        #endregion

        #region Empty Message
        [MessageHandler(NetworkManager.EmptyOrderedMessage_MsgID)]
        public static void Receive_EmptyMessage(Message msg)
        {
            ClientMessageHandler.Handle(msg, () =>
            {

            });
        }
        #endregion

        #region Helpers
        private void LogDisconnectReason(DisconnectReason reason)
        {
            string prefix = "Disconnected from Server";
            switch (reason)
            {
                case DisconnectReason.NeverConnected:
                    Engine.LogClient(prefix + ": No connection was ever established.");
                    break;
                case DisconnectReason.ConnectionRejected:
                    Engine.LogClient("Connection rejected.");
                    break;
                case DisconnectReason.TransportError:
                    Engine.LogClient("Connection failed; transport error.");
                    break;
                case DisconnectReason.TimedOut:
                    Engine.LogClient(prefix + ": Timed out.");
                    break;
                case DisconnectReason.Kicked:
                    Engine.LogClient(prefix + ": Kicked by Server.");
                    break;
                case DisconnectReason.ServerStopped:
                    Engine.LogClient(prefix + ": Server stopped.");
                    break;
                case DisconnectReason.Disconnected:
                    Engine.LogClient(prefix + ". Manual exit.");
                    break;
                case DisconnectReason.PoorConnection:
                    Engine.LogClient(prefix + ": Poor Connection.");
                    break;
                default:
                    Engine.LogClient(prefix + ": " + reason + " (propably server stopped).");
                    break;
            }
        }
        #endregion


        #region Debug
        [MessageHandler(165)]
        private static void Handle_Test1(Message msg)
        {
            LinkedMessage.HandleIncomingMessage(msg, (LinkedMessage msg) =>
            {
                var b1 = msg.GetBool();
                var b2 = msg.GetBool();
                var b3 = msg.GetBool();
                var b4 = msg.GetBool();
                var b5 = msg.GetBool();
                var b6 = msg.GetBool();
                var f1 = msg.GetFloat();
                var d1 = msg.GetDouble();
                var by = msg.GetByte();
                var i = msg.GetInt();
                var sho = msg.GetUShort();
                Debug.Log(b1 + ", " + b2 + ", " + b3 + ", " + b4 + ", " + b5 + ", " + b6 + ", " + f1 + ", " + d1 + ", " + by + ", " + i + ", " + sho);
            });
        }
        [MessageHandler(166)]
        private static void Handle_Test2(Message msg)
        {
            LinkedMessage.HandleIncomingMessage(msg, (LinkedMessage msg) =>
            {
                var num = msg.GetInt();
                StringBuilder txt = new StringBuilder("Values: ");
                for (int k = 0; k < num; k++)
                {
                    var i = msg.GetInt();
                    txt.Append(i + (k != num - 1 ? ", " : ""));
                    //if (k == num - 1)
                    //    Debug.Log(i);
                }
                Debug.Log(txt.ToString());
            });
        }
        [MessageHandler(167)]
        private static void Handle_Test3(Message msg)
        {
            LinkedMessage.HandleIncomingMessage(msg, (LinkedMessage msg) =>
            {
                Debug.Log(msg.GetString());
                Debug.Log(msg.GetString());
            });
        }
        #endregion
    }
}
