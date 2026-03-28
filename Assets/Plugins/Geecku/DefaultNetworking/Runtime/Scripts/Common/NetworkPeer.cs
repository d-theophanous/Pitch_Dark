using Geecku.DefaultEngine;
using Geecku.DefaultNetworking.Common.MessageHandlers;
using Geecku.GlobalMangers;
using Riptide;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Geecku.DefaultNetworking.Common
{
    /// <summary>
    /// Network Peer represents either client or server connection / peer.
    /// <br/>
    /// </summary>
    public abstract class NetworkPeer : SerializedMonoBehaviour
    {
        [SerializeField, FoldoutGroup("Links", expanded: false), ReadOnly] protected NetworkManager Network;
        [SerializeField, FoldoutGroup("Links"), ReadOnly] protected MessageHandler Messages;
        [SerializeField, FoldoutGroup("Links"), ReadOnly] protected Transform BaseTransform;
        public Peer TCPPeer { get; protected set; }
        public Peer UDPPeer { get; protected set; }
        public abstract bool IsReadyForPeer { get; }
        public abstract bool IsInConnection { get; }

        #region Clients
        /// <summary>
        /// Clients dictionary holds all connected (server-side) or known (client-side) clients 
        /// </summary>
        [HideInInspector] protected readonly Dictionary<ushort, ClientInfo> Clients = new();
        public bool Contains(ushort client_id) => Clients.ContainsKey(client_id);
        public ClientInfo GetClient(ushort client_id)
        {
            if (Contains(client_id))
            {
                return Clients[client_id];
            }
            return null;
        }
        public ClientInfo[] GetClientInfos() => Clients.ToValueArray();
        public ClientInfo AddClientInfo(Connection connection) => AddClientInfo(connection.Id, connection);
        public ClientInfo AddClientInfo(ushort client_id, Connection connection = null)
        {
            if (Clients.ContainsKey(client_id))
            {
                Debug.LogError("Client ID [" + client_id + "] is already registered");
                return Clients[client_id];
            }
            ClientInfo info;

            MessageHandler msg_handler = null;
            if (Messages is ClientMessageHandler cmh)
                msg_handler = cmh;
            else 
                msg_handler = (Messages as ServerMessageHandler);

            Clients.Add(client_id, info = new ClientInfo(client_id) { TCP = connection, MessageIndexOnConnect = msg_handler.MsgIdx });
            return info;
        }
        public ClientInfo AddClientInfo(ClientInfo client_info)
        {
            if (Clients.ContainsKey(client_info.ID))
            {
                Debug.LogError("Client ID [" + client_info.ID + "] is already registered");
                return Clients[client_info.ID];
            }

            Clients.Add(client_info.ID, client_info);
            return client_info;
        }
        public void ClearClientDic()
        {
            foreach (var pair in Clients)
            {
                pair.Value.Script.ClearClient();
                pair.Value.Script = null;
            }
            Clients.Clear();
        }
        #endregion

        protected virtual void Awake()
        {
            Network = GetComponentInParent<NetworkManager>();
            Messages = GetComponent<MessageHandler>();
            if (Messages == null)
            {
                if (this is ClientManager)
                    Messages = gameObject.AddComponent<ClientMessageHandler>();
                else if (this is ServerManager)
                    Messages = gameObject.AddComponent<ServerMessageHandler>();
                else
                    Messages = gameObject.AddComponent<MessageHandler>();
            }
            Messages.Peer = this;
            BaseTransform = Network.GetEmptyTransform(transform);

            Init();
        }

        /// <summary>
        /// Will be called in <see cref="Awake"/>.
        /// </summary>
        public abstract void Init();
        /// <summary>
        /// Starts the TCP/UDP connection. 
        /// <br/>
        ///  - Client: connects to the server.. <br/>
        ///  - Server: starts the server.. <br/>
        /// ..as defined by <see cref="NetworkManager"/> parameters (IPAddress, TCP_Port, ...)
        /// </summary>
        public abstract void StartPeer();
        /// <summary>
        /// Stops all TCP/UDP connections and resets peer values (Server/Client).
        /// </summary>
        public abstract void StopPeer();

        /// <summary>
        /// Executed directly before StartPeer executes its main peer<br/>
        /// Used to initiate base data (loading from files, dbs, etc..) for the server or client
        /// </summary>
        public virtual void OnBeforeStartPeer() { }

        /// <summary>
        /// Changes the name of the <see cref="BaseTransform"/>. 
        /// </summary>
        protected abstract void UpdateName();
        /// <summary>
        /// Updates all known <see cref="ClientVisualizationScript"/> associated to this peer (i.e. Ping).
        /// </summary>
        protected void UpdateClientVis()
        {
            foreach (var item in Clients)
            {
                item.Value.Script?.UpdateName();
            }            
        }

        #region Client Visualization
        /// <summary>
        /// Creates client transform needed for visualization based on client_id
        /// </summary>
        /// <param name="client_id">The client ID to create</param>
        public void AddOtherClientVis(ushort client_id) => AddClientVis(client_id);
        protected void AddClientVis(ushort client_id)
        {
            if (!Clients.ContainsKey(client_id))
            {
                Engine.LogServer("Client (" + client_id + ") is not registered yet");
                return;
            }
            var client = Clients[client_id];
            var script = Network.GetEmptyTransform(transform).gameObject.AddComponent<ClientVisualizationScript>();
            script.InitClient(client);
            client.Script = script;
        }
        protected void RemoveClientVis(ushort client_id)
        {
            if (!Clients.ContainsKey(client_id))
            {
                Engine.LogServer("Client (" + client_id + ") is not registered yet and can not be removed");
                return;
            }
            var client = Clients[client_id];
            client.Script.ClearClient();
            client.Script = null;
        }
        /// <summary>
        /// ClientVisualizationScript holds <see cref="ClientInfo"/>, (if Server) the TCP-<see cref="Connection"/> and <see cref="Ping"/>.
        /// </summary>
        public class ClientVisualizationScript : SerializedMonoBehaviour
        {
            private ClientInfo ClientInfo;
            private Connection Client => ClientInfo.TCP;
            [ShowInInspector, ReadOnly] private string AuthKey => ClientInfo.AuthKey;
            private short Ping = -1;

            public void InitClient(ClientInfo info)
            {
                ClientInfo = info;
                ClientInfo.Script = this;
                UpdateName();
            }
            public void ClearClient()
            {
                ClientInfo = null;
                Destroy(gameObject);
            }
            public void UpdateName()
            {
                name = "Client " + ClientInfo.ID + " [" + Ping + "ms]";
                if (Client == null)
                    return;
                SetPing(Client.SmoothRTT, false);
            }
            public void SetPing(short ping, bool update_self = true)
            {
                Ping = ping;
                if (update_self)
                    UpdateName();
            }
            public KeyValuePair<short, short> GetPingPair()
            {
                return new KeyValuePair<short, short>((short)Client.Id, Client.SmoothRTT);
            }
        }
        #endregion

        /// <summary>
        /// ClientInfo holds all necessary information about a client (ID, AuthKey, TCP/UDP connection, ...).
        /// </summary>
        [Serializable, InlineProperty]
        public class ClientInfo
        {
            [ReadOnly] public ushort ID;
            /// <summary>
            /// Is a unique string of characters generated by the server to identify a client.
            /// <br/>
            /// Can be used to reconnect.
            /// </summary>
            [ReadOnly] public string AuthKey;
            [HideInInspector] public Connection TCP;
            [HideInInspector] public Connection UDP;
            [HideInInspector] public int MessageIndexOnConnect;

            [HideInInspector] public ClientVisualizationScript Script;

            /// <summary>
            /// Creates empty client info object.
            /// </summary>
            /// <param name="client_id">Unique client ID to use</param>
            public ClientInfo(ushort client_id)
            {
                MessageIndexOnConnect = -1;
                ID = client_id;
            }
            /// <summary>
            /// Retrives ClientInfo from a <see cref="Message"/>.
            /// </summary>
            /// <param name="msg">Message to receive</param>
            public ClientInfo(Message msg)
            {
                MessageIndexOnConnect = -1;
                ApplyMessage(msg);
            }

            public Message ToMessage(Message msg)
            {
                msg.AddUShort(ID);
                msg.AddString(AuthKey);
                return msg;
            }
            public void ApplyMessage(Message msg)
            {
                ID = msg.GetUShort();
                AuthKey = msg.GetString();
            }
            public void Update(ClientInfo info)
            {
                if (info.ID != ID)
                {
                    Debug.LogError("ClientInfo IDs do not match");
                    return;
                }
                AuthKey = info.AuthKey;
            }
            public void UpdatePing(short ping)
            {
                Script.SetPing(ping);
            }
        }
    }
}
