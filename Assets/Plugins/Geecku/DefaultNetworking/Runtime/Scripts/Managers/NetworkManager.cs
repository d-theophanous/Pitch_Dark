using Geecku.DefaultNetworking.Attachables;
using Geecku.GlobalMangers;
using Riptide;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Geecku.DefaultNetworking
{
    /// <summary>
    /// Defines the network protocol:
    /// <br/><br/>
    ///  - Single: single TCP connection (via UDP) <br/>
    ///  - Double: double TCP and UDP connection (TCP is main) (TODO)
    /// </summary>
    public enum ProtocolTypes { Single, Double }
    /// <summary>
    /// The NetworkManager class uses Riptide networking, organizes and holds information for the network structure.
    /// <br/><br/>
    /// Contains network constants, Message ID's and references for the <see cref="ServerManager"/> and <see cref="ClientManager"/>. 
    /// </summary>
    public class NetworkManager : Singleton<NetworkManager>
    {
        #region GameMessage Link
        [HideInInspector] public IGameMessage GameMessager;
        public static Type GameMessageClassType;
        public static Action<LinkedMessage> HandleAddSyncObjectTransferAction;
        public static Action<LinkedMessage> HandleRemoveSyncObjectTransferAction;
        public static Action<LinkedMessage> HandleUpdateSyncObjectTransferAction;
        public static List<Action<LinkedMessage>> HandleInitialDownloadAction;
        public static void AddDownload<T, E>(Dictionary<ushort, T> dic) where T : SyncObject<E> where E : Enum
        {
            Action<LinkedMessage> init_download = (LinkedMessage msg) =>
            {
                //- First Content of message is download_id (int)
                msg.AddInt(dic.Count);
                foreach (var pair in dic)
                {
                    var local = pair.Value;
                    msg.AddHash(local);
                    local.ToLinkedMessage(msg);
                }
            };
            HandleInitialDownloadAction.Add(init_download);
        }

        private void InitGameMessageClass()
        {
            if (GameMessageClassType == null)
                return;
            var gm = gameObject.AddComponent(GameMessageClassType);
            if (gm is IGameMessage msg)
            {
                GameMessager = msg;
                msg.SetTransforms(ServerTransform, ClientTransform);
            }
        }
        #endregion

        #region Links
        [FoldoutGroup("Links", GroupName = "Linked Objects", Expanded = false), Space(2)]
        [SerializeField] protected Transform Setup;
        [FoldoutGroup("Links"), PropertySpace(SpaceBefore = 2, SpaceAfter = 1)]
        [SerializeField] protected Transform ServerTransform;
        [FoldoutGroup("Links"), PropertySpace(SpaceBefore = 1, SpaceAfter = 1)]
        [SerializeField] protected Transform ClientTransform;
        [FoldoutGroup("Links"), PropertySpace(SpaceBefore = 1, SpaceAfter = 1)]
        [SerializeField] protected Canvas Canvas;
        [FoldoutGroup("Links"), PropertySpace(SpaceBefore = 1, SpaceAfter = 1)]
        [SerializeField] protected Transform EmptyTransform;
        /// <summary>
        /// Reference to (optional) colored hierarchy; Boroder/RainbowHierarchy
        /// </summary>
        //[FoldoutGroup("Links"), PropertySpace(SpaceBefore = 1, SpaceAfter = 6)]        
        //[SerializeField] private HierarchyRulesetV2 Ruleset;

        /// <summary>
        /// Creates a new empty transform object.
        /// </summary>
        /// <param name="parent">The parent where to instantiate.</param>
        /// <returns>The fully generated empty game object / transform.</returns>
        public Transform GetEmptyTransform(Transform parent)
        {
            var local = Instantiate(EmptyTransform, parent);
            local.name = "Empty";
            return local;
        }
        #endregion

        #region Client / Server
        [BoxGroup("Managers", GroupName = "Managers"), Space(2)]
        public ServerManager _Server;
        public static ServerManager Server => Instance != null ? Instance._Server : null;
        [BoxGroup("Managers"), Space(2)]
        public ClientManager _Client;
        public static ClientManager Client => Instance != null ? Instance._Client : null;
        #endregion

        #region Inspector
        [ShowInInspector, BoxGroup("Connection Info")]
        public ProtocolTypes ProtocolType = ProtocolTypes.Single;
        /// <summary>
        /// The default IP-address to connect when <see cref="AutoConnectClient"/> is enabled.
        /// </summary>
        //[ShowInInspector, BoxGroup("Connection Info")]
        public string IP_Address = "127.0.0.1";
        /// <summary>
        /// The default TCP port to use.
        /// <br/><br/>
        /// Common free ports: 49152 – 65535
        /// </summary>
        //50112
        private ushort _TCP_Port = 64000;
        public ushort TCP_Port => _TCP_Port;
        /// <summary>
        /// The default UDP port to use when <see cref="ProtocolTypes.Double"/> is selected.
        /// <br/><br/>
        /// Common free ports: 49152 – 65535
        /// </summary>
        //50113
        private ushort _UDP_Port = 64001;
        public ushort UDP_Port => _UDP_Port;
        [ShowInInspector, ReadOnly, BoxGroup("Connection Info")]
        public ushort MaxPlayers => 16;
        #endregion

        #region Network Values
        /// <summary>
        /// Maximum number of (usable) bits that can be stored in a single <see cref="Message"/>.
        /// </summary>
        [ShowInInspector, BoxGroup("Network Values"), SuffixLabel("1 << 13")] public const int MaxPacketSize = 1 << 13;
        /// <summary>
        /// Message header vary in size based on the MsgID (size). 
        /// <br/><br/>
        /// MsgID &lt;  <see cref="MaxMsgIDLimitForTwoBytesInHeader"/> = 1 Byte <br/>
        /// MsgID &gt;= <see cref="MaxMsgIDLimitForTwoBytesInHeader"/> = 2 Byte <br/>
        /// MsgID &gt;= <see cref="MaxMsgIDLimitForThreeBytesInHeader"/> = 3 Byte <br/>
        /// </summary>
        [ShowInInspector, BoxGroup("Network Values"), SuffixLabel("1 << 7")] public const int MaxMsgIDLimitForTwoBytesInHeader = 1 << 7;
        /// <summary>
        /// Message header vary in size based on the MsgID (size). 
        /// <br/><br/>
        /// MsgID &lt;  <see cref="MaxMsgIDLimitForTwoBytesInHeader"/> = 1 Byte <br/>
        /// MsgID &gt;= <see cref="MaxMsgIDLimitForTwoBytesInHeader"/> = 2 Byte <br/>
        /// MsgID &gt;= <see cref="MaxMsgIDLimitForThreeBytesInHeader"/> = 3 Byte <br/>
        /// </summary>
        [ShowInInspector, BoxGroup("Network Values"), SuffixLabel("1 << 14")] public const int MaxMsgIDLimitForThreeBytesInHeader = 1 << 14;
        #endregion

        #region Message Constants
        [ShowInInspector, BoxGroup("Message Constants")] public const ushort HelloServer_MsgID = 5;
        [ShowInInspector, BoxGroup("Message Constants")] public const ushort HelloClient_MsgID = 6;
        [ShowInInspector, BoxGroup("Message Constants")] public const ushort ClientInfo_MsgID = 7;
        [ShowInInspector, BoxGroup("Message Constants")] public const ushort ClientPings_MsgID = 8;
        [ShowInInspector, BoxGroup("Message Constants")] public const ushort ClientReady_MsgID = 9;
        [ShowInInspector, BoxGroup("Message Constants")] public const ushort SyncObjectAdd_MsgID = 10;
        [ShowInInspector, BoxGroup("Message Constants")] public const ushort SyncObjectUpdate_MsgID = 11;
        [ShowInInspector, BoxGroup("Message Constants")] public const ushort SyncObjectRemove_MsgID = 12;
        [ShowInInspector, BoxGroup("Message Constants")] public const ushort DownloadTransfer_MsgID = 13;
        [ShowInInspector, BoxGroup("Message Constants")] public const ushort InitialDownloadTransfer_MsgID = 14;
        [ShowInInspector, BoxGroup("Message Constants")] public const ushort EmptyOrderedMessage_MsgID = 15;

        #endregion

        #region Auto-Values
        [BoxGroup("Auto Values"), SerializeField]
        private bool AutoStartServer;
        [BoxGroup("Auto Values"), SerializeField]
        private bool AutoConnectClient;

        /// <summary>
        /// Will be executed after server start.
        /// Should be used for initial download set-up (server side).
        /// Hint: Server set-up (added sync object) must be done before invoke.
        /// </summary>
        public event EventHandler OnServerStart;
        #endregion

        protected override void Awake()
        {
            base.Awake();
            if (WillBeDestroyed)
                return;

            _Server = GetComponentInChildren<ServerManager>();
            _Client = GetComponentInChildren<ClientManager>();

            Setup.gameObject.SetActive(!Engine.LoadNetworkScene);
            Engine.LogNetwork("Loaded");
        }
        protected override void Start()
        {
            base.Start();
            if (WillBeDestroyed)
                return;

            InitGameMessageClass();
            Engine.LogNetwork("Started");
            AfterNetworkStart?.Invoke(this, null);
            if (AutoStartServer)
                StartServer();
            if (AutoConnectClient)
                StartClient();
        }

        #region Events for AfterStart
        /// <summary>
        /// Is Executed after any NetworkManager Start-Methode is fired.
        /// Can be used reliably to execute NetworkCode as ServerStart of filling Server Data.
        /// </summary>
        public static event EventHandler AfterNetworkStart;
        #endregion

        //- Wrapper
        public static void StartServer()
        {
            Instance._Server.StartPeer();
            Instance.OnServerStart?.Invoke(Instance, null);
        }
        public static void StopServer()
        {
            Instance._Server.StopPeer();
        }
        public static void StartClient()
        {
            Instance._Client.StartPeer();
        }
        public static void StopClient()
        {
            Instance._Client.StopPeer();
        }

        #region Static Methods
        public static K Instantiate<K>(Transform transform) where K : MonoBehaviour
        {
            return Instance.GetEmptyTransform(transform).gameObject.AddComponent<K>();
        }
        public static void SetPort(ushort tcp, ushort udp)
        {
            Instance._TCP_Port = tcp;
            Instance._UDP_Port = udp;
        }
        #endregion

        #region Connect
        //- Manual Debug Connect / Start
        [HideIf("@Instance != null && this._Server != null && this._Server.IsInConnection")]
        [GUIColor("green"), BoxGroup("Buttons", GroupName = "Actions"), HorizontalGroup("Buttons/Debug Buttons"), Button("Start Server", ButtonHeight = 25)]
        private void Debug_StartServer()
        {
            if (Instance == null || _Server == null)
            {
                Debug.LogWarning("Can not instatiate server while Instance of network is null.");
                return;
            }
            if (!_Server.IsReadyForPeer)
            {
                Debug.LogWarning("Cilent is not ready for peer.");
                return;
            }
            StartServer();
        }
        [GUIColor("red"), ShowIf("@Instance != null && this._Server != null && this._Server.IsInConnection"), BoxGroup("Buttons"), HorizontalGroup("Buttons/Debug Buttons"), Button("Stop Server", ButtonHeight = 25)]
        private void Debug_StopServer()
        {
            if (Instance == null || _Server == null)
            {
                Debug.LogWarning("Can stop server while Instance of network is null.");
                return;
            }
            if (!_Server.IsInConnection)
            {
                Debug.LogWarning("Server is not started.");
                return;
            }
            _Server.StopPeer();
        }
        [HideIf("@Instance != null && this._Client != null && this._Client.IsInConnection")]
        [GUIColor("green"), BoxGroup("Buttons"), HorizontalGroup("Buttons/Debug Buttons"), Button("Connect Client", ButtonHeight = 25)]
        private void Debug_ConnectClient()
        {
            if (Instance == null || _Client == null)
            {
                Debug.LogWarning("Can not connect client while Instance of network is null.");
                return;
            }
            if (!_Client.IsReadyForPeer)
            {
                Debug.LogWarning("Cilent is not ready for peer.");
                return;
            }
            StartClient();
        }
        [GUIColor("red"), ShowIf("@Instance != null && this._Client != null && this._Client.IsInConnection"), BoxGroup("Buttons"), HorizontalGroup("Buttons/Debug Buttons"), Button("Disconnect Client", ButtonHeight = 25)]
        private void Debug_StopClient()
        {
            if (Instance == null || _Client == null)
            {
                Debug.LogWarning("Can not disconnect client while Instance of network is null.");
                return;
            }
            if (!_Client.IsInConnection)
            {
                Debug.LogWarning("Client is not connected.");
                return;
            }
            _Client.StopPeer();
        }
        #endregion

        #region Debug
        //[Button]
        //private void TestMethod()
        //{
        //    Test obj = new Test() { HashID = 45 };
        //    obj.LoadMethodes();
        //    foreach (var item in obj.List)
        //    {
        //        Debug.Log(item.Name + ", " + item.GetParameters().Length);
        //    }

        //    var m1 = obj.SafeMethode(nameof(obj.Test1));
        //    var m2 = obj.SafeMethode(nameof(obj.Test2), new object[] { 5 });
        //    var m3 = obj.SafeMethode(nameof(obj.Test3), new object[] { 8912f });

        //    obj.ExecuteMethode(m1.Index, m1.Arguments);
        //    obj.ExecuteMethode(m2.Index, m2.Arguments);
        //    obj.ExecuteMethode(m3.Index, m3.Arguments);
        //}
        #endregion

    }
    public static class NetworkExtension
    {
        public static void AddHash<T>(this LinkedMessage msg, HashObject<T> _object) where T : Enum
        {
            msg.AddUShort(_object.HashID);
            msg.AddUShort(Convert.ToUInt16(_object.Type));
            msg.AddString(_object.Name);
            msg.AddBool(_object.ExtraValue != 0);
            if (_object.ExtraValue != 0)
            {
                msg.AddUShort(_object.ExtraValue);
            }
        }
        public static void AddHash<T>(this LinkedMessage msg, SyncObject<T> _object) where T : Enum
        {
            msg.AddUShort(_object.HashID);
            msg.AddUShort(Convert.ToUInt16(_object.NetworkType));
            msg.AddString(_object.name);
            msg.AddBool(_object.ExtraValue != 0);
            if (_object.ExtraValue != 0)
            {
                msg.AddUShort(_object.ExtraValue);
            }
        }
        public static HashObject<T> GetHash<T>(this LinkedMessage msg) where T : Enum
        {
            ushort hash = msg.GetUShort();
            T type = (T)Enum.ToObject(typeof(T), msg.GetUShort());
            string name = msg.GetString();
            bool has_extra = msg.GetBool();
            ushort extra_value = 0;
            if (has_extra)
            {
                extra_value = msg.GetUShort();
            }
            return new HashObject<T>(hash, type, extra_value, name);
        }
        public static HashObject<T> GetHash<T>(this Message msg) where T : Enum
        {
            ushort hash = msg.GetUShort();
            T type = (T)Enum.ToObject(typeof(T), msg.GetUShort());
            string name = msg.GetString();
            bool has_extra = msg.GetBool();
            ushort extra_value = 0;
            if (has_extra)
            {
                extra_value = msg.GetUShort();
            }
            return new HashObject<T>(hash, type, extra_value, name);
        }
    }
    public static class TokenGenerator
    {
        private const string Base64Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_";

        public static string GenerateToken(int length)
        {
            if (length <= 0) throw new System.ArgumentOutOfRangeException(nameof(length), "Length must be positive");

            var token = new System.Text.StringBuilder(length);
            byte[] buffer = new byte[1];

            int safety_i = 0;
            const int saftety_limit = ushort.MaxValue;
            for (int i = 0; i < length; i++)
            {
                int index;
                do
                {
                    safety_i++;
                    System.Security.Cryptography.RandomNumberGenerator.Fill(buffer);
                    index = buffer[0] % Base64Chars.Length;
                }
                while (safety_i < saftety_limit && buffer[0] - index > (byte.MaxValue - Base64Chars.Length + 1) % Base64Chars.Length);

                token.Append(Base64Chars[index]);
            }
            if (safety_i >= saftety_limit)
                Debug.LogError("Saftylimit reached.");

            return token.ToString();
        }
        public static ChainedHasher CreateHasher()
        {
            return new ChainedHasher();
        }
        public static string GetHashString(ChainedHasher hasher)
        {
            return hasher.GetHash(Base64Chars);
        }
        public class ChainedHasher
        {
            private byte[] Current;

            public ChainedHasher()
            {
                Current = new byte[32];
            }

            public void AddToHash(short value)
            {
                AddToHashInternal(BitConverter.GetBytes(value), nameof(Int16));
            }
            public void AddToHash(ushort value)
            {
                AddToHashInternal(BitConverter.GetBytes(value), nameof(UInt16));
            }
            public void AddToHash(uint value)
            {
                AddToHashInternal(BitConverter.GetBytes(value), nameof(UInt32));
            }
            public void AddToHash(int value)
            {
                AddToHashInternal(BitConverter.GetBytes(value), nameof(Int32));
            }
            public void AddToHash(ulong value)
            {
                AddToHashInternal(BitConverter.GetBytes(value), nameof(UInt64));
            }
            public void AddToHash(long value)
            {
                AddToHashInternal(BitConverter.GetBytes(value), nameof(Int64));
            }
            public void AddToHash(float value)
            {
                AddToHashInternal(BitConverter.GetBytes(value), nameof(Single));
            }
            public void AddToHash(double value)
            {
                AddToHashInternal(BitConverter.GetBytes(value), nameof(Double));
            }
            public void AddToHash(string value)
            {
                AddToHashInternal(System.Text.Encoding.UTF8.GetBytes(value ?? ""), nameof(String));
            }

            private void AddToHashInternal(byte[] data, string typeName)
            {
                using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    byte[] type_bytes = System.Text.Encoding.UTF8.GetBytes(typeName);
                    byte[] combined = new byte[Current.Length + type_bytes.Length + data.Length];

                    Buffer.BlockCopy(Current, 0, combined, 0, Current.Length);
                    Buffer.BlockCopy(type_bytes, 0, combined, Current.Length, type_bytes.Length);
                    Buffer.BlockCopy(data, 0, combined, Current.Length + type_bytes.Length, data.Length);

                    Current = sha256.ComputeHash(combined);
                }
            }

            public string GetHash()
            {
                return BitConverter.ToString(Current).Replace("-", "").ToLowerInvariant();
            }
            public string GetHash(string alphabet)
            {
                if (string.IsNullOrEmpty(alphabet) || alphabet.Length < 2)
                    throw new ArgumentException("Alphabet must have at least 2 symbols.");

                // SHA256-Hash as BigInteger (unsigned, little-endian reversed)
                byte[] reversedHash = (byte[])Current.Clone();
                Array.Reverse(reversedHash); // BigInteger in little-endian format
                
                System.Numerics.BigInteger number = new System.Numerics.BigInteger(reversedHash, isUnsigned: true, isBigEndian: false);
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                int baseSize = alphabet.Length;

                int safety_i = 0;
                const int saftety_limit = ushort.MaxValue;
                do
                {
                    safety_i++;
                    int index = (int)(number % baseSize);
                    sb.Insert(0, alphabet[index]);
                    number /= baseSize;
                } while (safety_i < saftety_limit && number > 0);
                if (safety_i >= saftety_limit)
                    Debug.LogError("Saftylimit reached.");

                return sb.ToString();
            }
        }
    }

    /// <summary>
    /// Is the interface that will be implemented in a GameMessage class that the NetworkManager needs to asociate your project from the default one
    /// </summary>
    public interface IGameMessage
    {
        /// <summary>
        /// The methode that will be called by NetworkManager with the corsponding GameObject of ServerData and ClientData.
        /// Inside the IGameMessage class, there should be then a methode to handle those GameObjects properly
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        public void SetTransforms(Transform server, Transform client);
    }

    #region Daniels Aufgabenbereich
    public abstract class DataContainer
    {
        public ushort MessageHandlerID;
        public ushort MessageID;
        public Delegate Delegate;
        public void ApplyMessage(Message msg)
        {
            MessageID = msg.GetUShort();
            RetrieveData(msg);
        }
        protected abstract void RetrieveData(Message msg);
        public void ToMessage(Message msg, ushort msg_id)
        {
            msg.AddUShort(msg_id);
            AddDataToMsg(msg);
        }
        protected abstract void AddDataToMsg(Message msg);
        public abstract void Invoke();
        public void ApplyMessageAndInvoke(Message msg)
        {
            ApplyMessage(msg);
            Invoke();
        }
    }
    #endregion
}
