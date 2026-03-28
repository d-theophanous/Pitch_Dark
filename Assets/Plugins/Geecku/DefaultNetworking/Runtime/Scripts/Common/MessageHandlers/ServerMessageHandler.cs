using Geecku.DefaultNetworking.Attachables;
using Riptide;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Geecku.DefaultNetworking.Common.NetworkPeer;

namespace Geecku.DefaultNetworking.Common.MessageHandlers
{
    public class ServerMessageHandler : MessageHandler
    {
        #region Server Variables
        [BoxGroup("Messages"), ReadOnly, ShowInInspector] public int MessageIndex { get; set; }
        public override int MsgIdx => MessageIndex;
        #endregion

        public override void ResetHandler()
        {
            base.ResetHandler();
            MessageIndex = 0;
        }

        //- Wrapper
        public static void Handle(ServerManager manager, Message msg)
        {
            manager.MsgHandler.MessageIndex++;
            msg.AddInt(manager.MsgHandler.MessageIndex); //-    Msg index
        }

        #region Test in order Mesages
        [Button]
        private void TestOrder1()
        {
            ServerSend_TestInOrder1(NetworkManager.Server, 1, 1);
            ServerSend_TestInOrder1(NetworkManager.Server, 2, 3);
            ServerSend_TestInOrder1(NetworkManager.Server, 3, 5);
            ServerSend_TestInOrder1(NetworkManager.Server, 4, 2);
            ServerSend_TestInOrder1(NetworkManager.Server, 5, 4);
            ServerSend_TestInOrder1(NetworkManager.Server, 6, 6);
        }
        public static void ServerSend_TestInOrder1(ServerManager manager, int content, int msg_index = -1)
        {
            manager.MsgHandler.MessageIndex++;
            Message msg = Message.Create(MessageSendMode.Reliable, 567);

            if (msg_index == -1)
                msg.AddInt(manager.MsgHandler.MessageIndex);
            else
                msg.AddInt(msg_index);
            msg.AddInt(msg_index);

            manager.SendToAll(msg);
        }
        #endregion

        #region Server->Client SyncObjectAdd
        public static void Send_SyncObjectAdd<E, SynObj>(ServerManager manager, List<SynObj> sync_objects, params ushort[] client_ids) where SynObj : SyncObject<E> where E : Enum
        {
            List<int> clients = new();
            if (client_ids == null || client_ids.Length == 0)
                clients.Add(-1);
            else foreach (var item in client_ids)
                    clients.Add(item);

            manager.MsgHandler.MessageIndex++;
            foreach (int client_id in clients)
            {
                LinkedMessage msg = LinkedMessage.Create(NetworkManager.SyncObjectAdd_MsgID);

                msg.AddInt(manager.MsgHandler.MessageIndex); //-    Msg index

                msg.AddInt(sync_objects.Count);
                foreach (var sync_object in sync_objects)
                {
                    msg.AddHash<E>(sync_object);
                    sync_object.ToLinkedMessage(msg);
                }

                if (client_id == -1)
                    manager.SendToAll(msg);
                else
                    manager.SendTo(msg, (ushort)client_id);
            }
            //- ToDo: handle out of order clients who didnt receive the messages
        }

        #endregion

        #region Server->Client SyncObjectRemove
        public static void Send_SyncObjectRemove<E, SynObj>(ServerManager manager, List<SynObj> sync_objects, params ushort[] client_ids) where SynObj : SyncObject<E> where E : Enum
        {
            List<int> clients = new();
            if (client_ids == null || client_ids.Length == 0)
                clients.Add(-1);
            else foreach (var item in client_ids)
                    clients.Add(item);

            manager.MsgHandler.MessageIndex++;
            foreach (int client_id in clients)
            {
                LinkedMessage msg = LinkedMessage.Create(NetworkManager.SyncObjectRemove_MsgID);

                msg.AddInt(manager.MsgHandler.MessageIndex); //-    Msg index

                msg.AddInt(sync_objects.Count);
                foreach (var sync_object in sync_objects)
                {
                    msg.AddHash<E>(sync_object);
                }

                if (client_id == -1)
                    manager.SendToAll(msg);
                else
                    manager.SendTo(msg, (ushort)client_id);
            }
            //- ToDo: handle out of order clients who didnt receive the messages
        }

        #endregion

        #region Server->Client SyncObjectUpdate
        public static void Send_SyncObjectUpdate<E, SynObj>(ServerManager manager, List<SynObj> sync_objects, params ushort[] client_ids) where SynObj : SyncObject<E> where E : Enum
        {
            List<int> clients = new();
            if (client_ids == null || client_ids.Length == 0)
                clients.Add(-1);
            else foreach (var item in client_ids)
                    clients.Add(item);

            manager.MsgHandler.MessageIndex++;
            foreach (int client_id in clients)
            {
                LinkedMessage msg = LinkedMessage.Create(NetworkManager.SyncObjectUpdate_MsgID);

                msg.AddInt(manager.MsgHandler.MessageIndex); //-    Msg index

                msg.AddInt(sync_objects.Count);
                foreach (var sync_object in sync_objects)
                {
                    msg.AddHash<E>(sync_object);
                    sync_object.ToLinkedMessage(msg);
                }

                if (client_id == -1)
                    manager.SendToAll(msg);
                else
                    manager.SendTo(msg, (ushort)client_id);
            }
            //- ToDo: handle out of order clients who didnt receive the messages
        }
        #endregion
    }
}
