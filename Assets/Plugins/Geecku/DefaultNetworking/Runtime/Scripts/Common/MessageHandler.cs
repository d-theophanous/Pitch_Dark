using Geecku.DefaultNetworking.Attachables;
using Geecku.DefaultNetworking.Template;
using Geecku.GlobalMangers;
using Riptide;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Geecku.DefaultNetworking.Common.NetworkPeer;

namespace Geecku.DefaultNetworking.Common
{
    public class MessageHandler : SerializedMonoBehaviour
    {
        [ReadOnly] public NetworkPeer Peer;

        #region Msg Variables
        public virtual int MsgIdx => -1;
        #endregion


        #region Message Handler Class Local
        public virtual void ResetHandler()
        {

        }
        #endregion

        #region Server Hello
        public static void Send_ServerHello(ServerManager manager, ushort client_id)
        {
            Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.HelloServer_MsgID);

            //- AuthKey
            msg.AddString(manager.GetClient(client_id).AuthKey);
            //- Other clients
            msg.AddByte((byte)(manager.GetClientInfos().Length - 1));
            foreach (var client in manager.GetClientInfos())
            {
                if (client.ID == client_id)
                    continue;
                client.ToMessage(msg);
            }

            manager.SendTo(msg, client_id);
            Send_ClientInfo(manager, client_id);
        }
        [MessageHandler(NetworkManager.HelloServer_MsgID)]
        private static void Receive_ServerHello(Message msg)
        {
            var manager = NetworkManager.Client;
            var auth_key = msg.GetString();
            manager.LocalClient.AuthKey = auth_key;
            var length = msg.GetByte();
            for (int i = 0; i < length; i++)
            {
                var client_info = new ClientInfo(msg);
                manager.AddClientInfo(client_info);
                manager.AddOtherClientVis(client_info.ID);
            }
            manager.MsgHandler.ReceivedServerHello = true;
            manager.OnReceiveServerHelloWrapper();
            //Engine.LogClient(auth_key);
            Send_ClientHello(manager);
        }
        #endregion

        #region Client Hello
        public static void Send_ClientHello(ClientManager manager)
        {
            Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.HelloClient_MsgID);

            //- Nothing yet

            manager.Send(msg);
        }
        [MessageHandler(NetworkManager.HelloClient_MsgID)]
        private static void Receive_ClientHello(ushort client_id, Message msg)
        {
            var manager = NetworkManager.Server;
            List<int> download_id_list = new();
            var has_inital_download = manager.HasInitialDownloadFor(client_id);
            if (has_inital_download)
            {
                download_id_list = manager.InitializeAndStartDownload(client_id);
            }
            
            Send_ClientReady(manager, client_id, download_id_list);
        }
        #endregion

        #region Server Sends Client Rdy
        public static void Send_ClientReady(ServerManager manager, ushort client_id, List<int> download_id_list)
        {
            var msg_index_on_connect = manager.GetClient(client_id).MessageIndexOnConnect;

            Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.ClientReady_MsgID);

            msg.AddInt(msg_index_on_connect);
            msg.AddInt(download_id_list.Count);
            foreach (var id in download_id_list)
            {
                msg.AddInt(id);
            }

            manager.SendTo(msg, client_id);
        }
        [MessageHandler(NetworkManager.ClientReady_MsgID)]
        private static void Receive_ClientReady(Message msg)
        {
            var manager = NetworkManager.Client;
            var msg_index = msg.GetInt();
            var download_length = msg.GetInt();
            List<int> download_id_list = new();
            for (int i = 0; i < download_length; i++)
            {
                download_id_list.Add(msg.GetInt());
            }

            manager.OnReceiveClientReadyWrapper(download_length != 0);
            if (download_length != 0)
            {
                Engine.LogClient("Initial Download detected. Waiting for completion to finish client-ready.");
                //Debug.Log("Download count: " + download_length);
                ClientManager.FinalInitialDownloadIDList = download_id_list;
                ClientManager.MessageIndexOnConnect = msg_index;
                ClientManager.CheckReceivedInitialDownloads();
                return;
            }
            manager.MsgHandler.SetClientReady(msg_index);
            manager.OnReceiveInitialDownloadConfirmedWrapper(false);
        }
        #endregion

        #region Client Info 
        public static void Send_ClientInfo(ServerManager manager, ushort client_id)
        {
            Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.ClientInfo_MsgID);

            //- Message
            manager.GetClient(client_id).ToMessage(msg);

            manager.SendToAllExcept(msg, client_id);
        }
        [MessageHandler(NetworkManager.ClientInfo_MsgID)]
        private static void Receive_ServerClientInfo(Message msg)
        {
            var manager = NetworkManager.Client;

            var client_info = new ClientInfo(msg);
            manager.GetClient(client_info.ID).Update(client_info);
        }
        #endregion

        #region Client Pings 
        private static Dictionary<short, short> PingDic = new Dictionary<short, short>();
        public static void Send_ClientPings(ServerManager manager)
        {
            Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.ClientPings_MsgID);

            //- Message
            var list = manager.GetClientInfos();
            List<KeyValuePair<short, short>> valid_pair_list = new();
            foreach (var info in list)
            {
                if (info.Script != null)
                {
                    var pair = info.Script.GetPingPair();
                    if (!PingDic.ContainsKey(pair.Key))
                        PingDic.Add(pair.Key, pair.Value);
                    if (PingDic[pair.Key] == 0 || PingDic[pair.Key] != pair.Value)
                    {
                        valid_pair_list.Add(pair);
                        PingDic[pair.Key] = pair.Value;
                        continue;
                    }
                }
            }
            if (valid_pair_list.Count == 0)
                return;
            //string txt = "List: " + valid_pair_list.Count;
            //foreach (var item in valid_pair_list)
            //{
            //    txt += "\n" + item.Key + ": " + item.Value;
            //}
            //Engine.LogServer(txt);
            msg.AddByte((byte)valid_pair_list.Count);
            foreach (var pair in valid_pair_list)
            {
                msg.AddKeyValuePair(pair);
            }

            manager.SendToAll(msg);
            //- ToDo: Clients nach dem Disconnecten aus dem Dictionary entfernen
        }
        [MessageHandler(NetworkManager.ClientPings_MsgID)]
        private static void Receive_ClientPings(Message msg)
        {
            var manager = NetworkManager.Client;

            var length = msg.GetByte();
            for (int i = 0; i < length; i++)
            {
                var pair = msg.GetKeyValuePair();
                if (pair.Key == manager.LocalClient.ID)
                    continue;
                if (pair.Key != -1)
                {
                    var info = manager.GetClient((ushort)pair.Key);
                    if (info != null)
                        info.UpdatePing(pair.Value);
                }
            }
        }
        #endregion

        #region Daniels Aufgabenbereich
        public static ulong ServerMsgID;
        public static ulong ClientMsgID;
        public struct MsgInfo : IComparable<MsgInfo>
        {
            public ulong ID;
            public Action Action;

            public int CompareTo(MsgInfo other)
            {
                return ID.CompareTo(other.ID);
            }
        }

        public static List<MsgInfo> PendingMsgList = new();
        private static void ExecutePendingMessages()
        {
            while (PendingMsgList.Count > 0 && PendingMsgList[0].ID == ClientMsgID + 1)
            {
                PendingMsgList[0].Action.Invoke();
                ClientMsgID++;
                PendingMsgList.RemoveAt(0);
            }
        }
        public static void SendDelegateDataMessage(DataContainer data)
        {
            ServerMsgID++;
            Message msg = Message.Create(MessageSendMode.Reliable, data.MessageHandlerID);

            data.ToMessage(msg, (ushort)ServerMsgID);

            //- momentan nur für server
            NetworkManager.Server.SendToAll(msg);
        }
        public static void ReceiveDelegateDataMessage(Action action, ushort msg_id)
        {
            MsgInfo info = new MsgInfo() { ID = msg_id, Action = action };
            HandleDelegateMsg(info);
        }
        private static void HandleDelegateMsg(MsgInfo msg_info)
        {
            if (msg_info.ID == ClientMsgID + 1)
            {
                msg_info.Action.Invoke();
                ClientMsgID++;
                ExecutePendingMessages();
                return;
            }
            PendingMsgList.Add(msg_info);
            PendingMsgList.Sort();
            Debug.LogWarning("Msg out of order, " + msg_info.ID);
        }
        #endregion
    }
}
