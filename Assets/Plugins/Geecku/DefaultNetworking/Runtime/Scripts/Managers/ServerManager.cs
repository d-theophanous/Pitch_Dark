using Geecku.DefaultNetworking.Attachables;
using Geecku.DefaultNetworking.Common;
using Geecku.DefaultNetworking.Common.MessageHandlers;
using Geecku.GlobalMangers;
using Riptide;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Geecku.DefaultNetworking
{
    /// <summary>
    /// Handles and manages all server related traffic.  
    /// </summary>
    public class ServerManager : NetworkPeer
    {
        protected Server Server => TCPServer;
        protected Server TCPServer => TCPPeer as Server;
        protected Server UDPServer => UDPPeer as Server;
        public ServerMessageHandler MsgHandler => Messages as ServerMessageHandler;
        public override bool IsReadyForPeer => TCPServer != null && !TCPServer.IsRunning;
        public override bool IsInConnection => TCPServer != null && TCPServer.IsRunning;
        public bool IsActive => Server != null && Server.IsRunning;
        public bool IsRunning => IsActive;

        #region GetVariables
        public int CurPlayers => Server.ClientCount;
        #endregion

        public override void Init()
        {
            Riptide.Utils.RiptideLogger.Initialize(
                Engine.LogNetwork, null, UnityEngine.Debug.LogWarning, UnityEngine.Debug.LogError,
                false);

            switch (Network.ProtocolType)
            {
                case ProtocolTypes.Double:
                    TCPPeer = new Server(new Riptide.Transports.Tcp.TcpServer(), "<color=#904C50>TCP-SERVER</color>");
                    UDPPeer = new Server(new Riptide.Transports.Udp.UdpServer(), "<color=#904C50>UDP-SERVER</color>");
                    break;
                default:    //- ProtocolTypes.Single
                    TCPPeer = new Server("<color=#904C50>SERVER</color>");
                    break;
            }

            TCPServer.ClientConnected += TCPServer_ClientConnected;
            TCPServer.ClientDisconnected += TCPServer_ClientDisconnected;
            TCPServer.ConnectionFailed += TCPServer_ConnectionFailed;
            //TCPServer.MessageReceived += TCPServer_MessageReceived;

            TCPServer.HandleConnection = OnClientConnectionTCP;
            if (Network.ProtocolType != ProtocolTypes.Single)
                UDPServer.HandleConnection = OnClientConnectionUDP;
            gameObject.SetActive(false);
        }

        public override void StartPeer()
        {
            gameObject.SetActive(true);
            Messages.ResetHandler();

            OnBeforeStartPeer();
            switch (Network.ProtocolType)
            {
                case ProtocolTypes.Double:
                    TCPServer.Start(Network.TCP_Port, Network.MaxPlayers);
                    Engine.LogServer("TCP Started on port " + Network.TCP_Port + ".");
                    UDPServer.Start(Network.UDP_Port, Network.MaxPlayers);
                    Engine.LogServer("UDP Started on port " + Network.UDP_Port + ".");
                    break;
                default:    //- ProtocolTypes.Single
                    TCPServer.Start(Network.TCP_Port, Network.MaxPlayers);
                    Engine.LogServer("Started on port " + Network.TCP_Port + ".");
                    break;
            }
            UpdateName();
        }
        public override void StopPeer()
        {
            gameObject.SetActive(false);    //- maybe this will prevent to kick clients correctly
            Messages.ResetHandler();
            if (TCPServer != null && TCPServer.IsRunning)
            {
                foreach (var connection in TCPServer.Clients)
                    TCPServer.DisconnectClient(connection);
                TCPServer.Stop();
                //Engine.LogServer("TCP Stopped.");
            }
            if (UDPServer != null && UDPServer.IsRunning)
            {
                foreach (var connection in UDPServer.Clients)
                    UDPServer.DisconnectClient(connection);
                UDPServer.Stop();
                //Engine.LogServer("UDP Stopped.");
            }
            Engine.LogServer("Server Stopped.");
            Clients.Clear();
            UpdateName();
        }

        #region On Client Connect
        public const byte AuthKeyLength = 16;
        /// <summary>
        /// Is called by Riptide when a client is trying to establish a TCP-connection.
        /// <br/>
        /// Must be either rejected or accepted by the server.
        /// </summary>
        /// <param name="connection">Incoming client connection</param>
        /// <param name="msg">Contains reconnect information from the client</param>
        protected virtual void OnClientConnectionTCP(Connection connection, Message msg)
        {
            SetupBaseQuality(connection);

            var is_recon = msg.GetBool();
            TCPServer.Accept(connection);

            ClientInfo info = AddClientInfo(connection);
            if (!is_recon)
            {
                string key = string.Empty;
                const int max_itter = ushort.MaxValue;
                int i = 0;
                while (i < max_itter && ContainsAuthKey(key = TokenGenerator.GenerateToken(AuthKeyLength))) 
                    i++; 

                info.AuthKey = key;
                return;
            }
            var auth_key = msg.GetString();
            //- ToDo: Reconnect
        }
        /// <summary>
        /// Is called by Riptide when a client is trying to establish a UDP-connection.
        /// <br/>
        /// Must be either rejected or accepted by the server.
        /// </summary>
        /// <param name="connection">Incoming client connection</param>
        /// <param name="msg"></param>
        protected virtual void OnClientConnectionUDP(Connection connection, Message msg)
        {
            ushort client_id = msg.GetUShort();

            SetupBaseQuality(connection);
            if (!Clients.ContainsKey(client_id))
            {
                UDPServer.Reject(connection, null);
                return;
            }
            UDPServer.Accept(connection);

            Clients[client_id].UDP = connection;
        }
        /// <summary>
        /// Sets up base quality information for the client.
        /// <br/>
        /// i.e. <see cref="Connection.CanQualityDisconnect"/>.
        /// </summary>
        /// <param name="connection"></param>
        private void SetupBaseQuality(Connection connection)
        {
            connection.CanQualityDisconnect = true;
        }
        private bool ContainsAuthKey(string key)
        {
            foreach (var client in Clients)
            {
                if (client.Value.AuthKey == key)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        protected virtual void FixedUpdate()
        {
            if (TCPServer == null)
                return;
            switch (Network.ProtocolType)
            {
                case ProtocolTypes.Double:
                    TCPServer?.Update();
                    UDPServer?.Update();
                    break;
                default:    //- ProtocolTypes.Single
                    TCPServer?.Update();
                    break;
            }
            UpdateClientVis();
            MessageHandler.Send_ClientPings(this);
        }
        protected override void UpdateName()
        {
            if (TCPServer == null || !TCPServer.IsRunning)
            {
                BaseTransform.name = "Server [inactive]";
                return;
            }
            BaseTransform.name = "Server [" + TCPServer.ClientCount + "/" + TCPServer.MaxClientCount + "]";
        }

        #region Send Messages
        public virtual void SendToAll(Message msg)
        {
            msg.Send(Server);
        }
        public virtual void SendTo(Message msg, ushort client_id)
        {
            msg.Send(Server, client_id);
        }
        public virtual void SendToAll(LinkedMessage msg)
        {
            msg.Send(Server);
        }
        public virtual void SendTo(LinkedMessage msg, ushort client_id)
        {
            msg.Send(Server, client_id);
        }
        public virtual void SendToAllExcept(Message msg, ushort except_client_id, bool is_ordered = false)
        {
            Server.SendToAll(msg, except_client_id);
            if (is_ordered)
            {
                Message new_msg = Message.Create(MessageSendMode.Reliable, NetworkManager.EmptyOrderedMessage_MsgID);
                new_msg.AddInt(MsgHandler.MessageIndex);
                Server.Send(new_msg, except_client_id);
            }
        }
        #endregion

        #region EventHandlers
        public event EventHandler<ServerConnectedEventArgs> OnClientConnected;
        public event EventHandler<ServerDisconnectedEventArgs> OnClientDisconnected;
        #endregion

        #region Events
        //- Client Connect/Disconnect/Failed
        /// <summary>
        /// Gets called when a client connection has been successfully established and sends back a <see cref="MessageHandler.Send_ServerHello(ServerManager, ushort)"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TCPServer_ClientConnected(object sender, ServerConnectedEventArgs e)
        {
            Engine.LogServer("Client " + e.Client.Id + " (" + e.Client.ToString() + ") connected successfully.");
            UpdateName();
            AddClientVis(e.Client.Id);
            MessageHandler.Send_ServerHello(this, e.Client.Id);
            OnClientConnected?.Invoke(this, e);
        }
        /// <summary>
        /// Gets called when a client disconnects for any reason. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TCPServer_ClientDisconnected(object sender, ServerDisconnectedEventArgs e)
        {
            OnClientDisconnected?.Invoke(this, e);
            LogDisconnectReason(e.Client, e.Reason);
            RemoveClientVis(e.Client.Id);
            Clients.Remove(e.Client.Id);
            UpdateName();
        }
        /// <summary>
        /// Gets called when a client was never able to connect succesfully in the first place.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TCPServer_ConnectionFailed(object sender, ServerConnectionFailedEventArgs e)
        {
            if (Clients.ContainsKey(e.Client.Id))
                Clients.Remove(e.Client.Id);
            Engine.LogServer("Connection Failed: " + e.Client.Id);
            UpdateName();
        }
        //- Msg Received
        private void TCPServer_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Engine.LogClient("MsgReceived: " + e.MessageId);
        }
        #endregion

        #region Client Ready / First Connect
        public bool HasInitialDownloadFor(ushort client_id)
        {
            if (GetClient(client_id) is ClientInfo info)
                return GetConnectClientDownloadID(info);
            return false;
        }
        protected virtual bool GetConnectClientDownloadID(ClientInfo info) => 
            NetworkManager.HandleInitialDownloadAction != null && 
            NetworkManager.HandleInitialDownloadAction.Count > 0;
        #endregion

        #region Download
        
        //private List<uint> ActiveDownloadIDList = new();
        //private Dictionary<ushort, List<DownloadInfo>> ClientDownloadDic = new(); //-   ToDo Richtig clearen (bei Server Start und Ende)
        private const int MinIDValue = 1000;
        private int CreateDownloadID(List<int> list)
        {
            int id, i = 0;
            const int max_itter = 10000;
            while (list.Contains(id = Engine.Random.Next(MinIDValue, int.MaxValue)) && i < max_itter)
                i++;
            if (i >= max_itter)
            {
                Debug.LogError("Max itter reached in ServerManager CreateDownloadID().");
                return 0;
            }
            list.Add(id);
            return id;
        }
        

        public List<int> InitializeAndStartDownload(ushort client_id)
        {
            List<int> local = new();
            foreach (var action in NetworkManager.HandleInitialDownloadAction)
            {
                LinkedMessage msg = LinkedMessage.Create(NetworkManager.InitialDownloadTransfer_MsgID);
                
                CreateDownloadID(local);
                msg.AddInt(local[local.Count - 1]);
                action.Invoke(msg);

                SendTo(msg, client_id);
            }
            return local;
        }

        public void StartDownload<T, K>(List<T> sync_obj_list, ushort client_id) where T : SyncObject<K> where K : Enum
        {
            if (HandleSyncObjDownloadAction == null)
            {
                Debug.LogError("NetworkManager.HandleSyncObjDownloadAction must be defined before starting the download.");
                return;
            }
            //if (!ClientDownloadDic.ContainsKey(client_id))
            //    ClientDownloadDic.Add(client_id, new());

            //var info = new DownloadInfo();
            //ClientDownloadDic[client_id].Add(info);

            LinkedMessage msg = LinkedMessage.Create(NetworkManager.DownloadTransfer_MsgID);
            msg.AddInt(sync_obj_list.Count);
            foreach (var local in sync_obj_list)
            {
                msg.AddHash(local);
                local.ToLinkedMessage(msg);
            }
            SendTo(msg, client_id);
            Engine.LogServer("Starting Download ("+ sync_obj_list.Count +") to Client: " + client_id);
        }
        private static Action<LinkedMessage> HandleSyncObjDownloadAction => NetworkManager.HandleAddSyncObjectTransferAction;
        [MessageHandler(NetworkManager.DownloadTransfer_MsgID)]
        public static void Receive_DownloadTransfer(Message local)
        {
            //Debug.Log("Receive_Download Start");
            LinkedMessage.HandleIncomingMessage(local, (LinkedMessage msg) =>
            {
                //Debug.Log("Receive_Download");
                if (HandleSyncObjDownloadAction == null)
                {
                    Debug.LogError("NetworkManager.HandleSyncObjDownloadAction must be defined and can not be null");
                    return;
                }
                var count = msg.GetInt();
                for (int i = 0; i < count; i++)
                {
                    HandleSyncObjDownloadAction(msg);
                }
                Debug.Log("Finished Download");
            });
        }
        #endregion

        #region Helpers
        private void LogDisconnectReason(Connection client, DisconnectReason reason)
        {
            string prefix = "Client " + client.Id + " (" + client.ToString() + ")";
            switch (reason)
            {
                case DisconnectReason.NeverConnected:
                    Engine.LogServer(prefix + " disconnected. No connection was ever established.");
                    break;
                case DisconnectReason.ConnectionRejected:
                    Engine.LogServer(prefix + " connection rejected.");
                    break;
                case DisconnectReason.TransportError:
                    Engine.LogServer(prefix + " connection failed, transport error.");
                    break;
                case DisconnectReason.TimedOut:
                    Engine.LogServer(prefix + " disconnected: Timed out.");
                    break;
                case DisconnectReason.Kicked:
                    Engine.LogServer(prefix + " disconnected: Kicked by Server.");
                    break;
                case DisconnectReason.ServerStopped:
                    Engine.LogServer(prefix + " disconnected: Server stopped.");
                    break;
                case DisconnectReason.Disconnected:
                    Engine.LogServer(prefix + " disconnected.");
                    break;
                case DisconnectReason.PoorConnection:
                    Engine.LogServer(prefix + " disconnected: Poor Connection.");
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Debug
        [ContextMenu("Debug/Test 1")]
        private void Test1()
        {
            LinkedMessage l_msg = LinkedMessage.Create(165);
            l_msg.AddBool(true);
            l_msg.AddBool(false);
            l_msg.AddBool(false);
            l_msg.AddBool(false);
            l_msg.AddBool(true);
            l_msg.AddBool(true);
            l_msg.AddFloat(314.54f);
            l_msg.AddDouble(5514.54d);
            l_msg.AddByte(100);
            l_msg.AddInt(1324144);
            l_msg.AddUShort(50001);
            //var hash = TokenGenerator.CreateHasher();
            //hash.AddToHash(1324144);
            //hash.AddToHash(50001);
            //Debug.Log(hash.GetHash());
            //Debug.Log(hash.GetHash("0123456789abcdefghijklmnopqrstuvwxyz"));
            //Debug.Log(TokenGenerator.GetHashString(hash));

            l_msg.Send(Server);
        }
        [ContextMenu("Debug/Test 2")]
        private void Test2()
        {
            LinkedMessage l_msg = LinkedMessage.Create(166);
            int num = 90000;
            l_msg.AddInt(num);
            for (int i = 0; i < num; i++)
            {
                l_msg.AddInt(i);
            }

            l_msg.Send(Server);
        }
        [ContextMenu("Debug/Test 3")]
        private void Test3()
        {
            LinkedMessage l_msg = LinkedMessage.Create(167);
            l_msg.AddString("Das ist ein Text");
            l_msg.AddString("Sonderzeichen wie äöü und ß!?");

            l_msg.Send(Server);
        }
        #endregion
    }
}
