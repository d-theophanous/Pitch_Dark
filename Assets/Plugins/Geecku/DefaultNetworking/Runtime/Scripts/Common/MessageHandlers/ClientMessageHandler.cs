using Geecku.GlobalMangers;
using Riptide;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geecku.DefaultNetworking.Common.MessageHandlers
{
    public class ClientMessageHandler : MessageHandler
    {
        #region Client Variables
        [BoxGroup("Variables"), ReadOnly, ShowInInspector] public bool ClientReady { get; private set; }
        [BoxGroup("Variables"), ReadOnly, ShowInInspector] public bool ReceivedServerHello { get; set; }

        #region Listening
        [BoxGroup("Variables"), ReadOnly, ShowInInspector] private bool ListensToMessages => IsListeningToMessages && DontListenValue == 0;
        private int _GlobalListenValue = 0;
        private bool CalledRoutine;
        [HideInInspector] public int DontListenValue
        {
            get => _GlobalListenValue;
            set
            {
                _GlobalListenValue = value;
                if (_GlobalListenValue > 0)
                {
                    StopAllCoroutines();
                    CalledRoutine = false;
                }
                if (_GlobalListenValue < 0)
                    _GlobalListenValue = 0;

                if (!CalledRoutine && ListensToMessages)
                {
                    CalledRoutine = true;
                    StartCoroutine(ListeningRoutineCheck());
                }
            }
        }
        private IEnumerator ListeningRoutineCheck()
        {
            yield return new WaitForEndOfFrame();
            if (ListensToMessages)
                ExecutePendingMessagesIfValide();
            CalledRoutine = false;
        }
        private bool _IsListeningToMessages;
        [HideInInspector] public bool IsListeningToMessages
        {
            get => _IsListeningToMessages;
            set
            {
                _IsListeningToMessages = value;
                if (!_IsListeningToMessages)
                {
                    StopAllCoroutines();
                    CalledRoutine = false;
                }
                else if (_IsListeningToMessages && !CalledRoutine && ListensToMessages)
                {
                    CalledRoutine = true;
                    StartCoroutine(ListeningRoutineCheck());
                }
            }
        }
        #endregion

        [BoxGroup("Variables"), ReadOnly, ShowInInspector, Title("")] public bool IsReadyForMessages => ClientReady && ReceivedServerHello && ListensToMessages;
        [HideInInspector] public int DownloadID { get; set; }
        #endregion

        #region Messages
        [BoxGroup("Messages"), ReadOnly, ShowInInspector] public int MessageIndex { get; private set; } = -1;
        public override int MsgIdx => MessageIndex;
        [BoxGroup("Messages"), ReadOnly, ShowInInspector] public int PendingMessages => PendingMessageStructList.Count;
        #endregion

        public void SetClientReady(int server_message_index)
        {
            ClientReady = true;
            MessageIndex = server_message_index;
            Engine.LogClient("Client ready with msg-index '" + MessageIndex + "'.");
        }
        public override void ResetHandler()
        {
            base.ResetHandler();

            _GlobalListenValue = 0;
            _IsListeningToMessages = true;
            DownloadID = -1;
            MessageIndex = -1;
            PendingMessageStructList = new();
            ClientReady = false;
            ReceivedServerHello = false;
        }

        private struct MsgHeader
        {
            public int MsgIdx;

            public MsgHeader(Message msg, out ClientManager manager) { MsgIdx = msg.GetInt(); manager = NetworkManager.Client; }
            public MsgHeader(LinkedMessage msg, out ClientManager manager) { MsgIdx = msg.GetInt(); manager = NetworkManager.Client; }
        }
        private struct MsgStruct : IComparable<MsgStruct>
        {
            public long MsgIndex;
            public Action Action;

            public int CompareTo(MsgStruct other)
            {
                return MsgIndex.CompareTo(other.MsgIndex);
            }
        }
        private List<MsgStruct> PendingMessageStructList = new();
        private void HandleAction(int msg_index, Action action)
        {
            bool safe_and_execute_later = !IsReadyForMessages;

            if (!safe_and_execute_later && msg_index == this.MessageIndex + 1) //- if true, then its a message in order
            {
                action.Invoke();
                MessageIndex++;
                ExecutePendingMessagesIfValide();
                return;
            }
            SafeMsgStruct(msg_index, action);
        }
        private void SafeMsgStruct(long msg_index, Action action)
        {
            Debug.LogWarning("Client received msg out of order '" + msg_index + "'.");
            MsgStruct value = new MsgStruct() { MsgIndex = msg_index, Action = action };
            PendingMessageStructList.Add(value);
            PendingMessageStructList.Sort();
        }
        public void ExecutePendingMessagesIfValide()
        {
            if (!IsReadyForMessages)
                return;
            while (IsReadyForMessages && PendingMessageStructList.Count > 0 && PendingMessageStructList[0].MsgIndex == this.MessageIndex + 1)
            {
                var first = PendingMessageStructList[0];
                PendingMessageStructList.RemoveAt(0);

                first.Action.Invoke();
                MessageIndex++;
            }
        }

        //- Wrapper
        public static void Handle(Message msg, Action action)
        {
            var msg_header = new MsgHeader(msg, out ClientManager manager);

            //var i = msg.GetInt();
            //Action action = () =>
            //{
            //    Debug.Log("Print " + i);
            //};
            manager.MsgHandler.HandleAction(msg_header.MsgIdx, action);
        }
        //public static void HandleIndex(Message msg, out ClientManager manager)
        //{
        //    var msg_header = new MsgHeader(msg, out ClientManager m);
        //    manager = m;
        //}
        //public static void HandleAction(int msg_index, Action action, ClientManager manager)
        //{
        //    manager.MsgHandler.HandleAction(msg_index, action);
        //}

        #region Test in order Mesages
        [MessageHandler(567)]
        public static void ClientGets_TestInOrder1(Message msg)
        {
            var msg_header = new MsgHeader(msg, out ClientManager manager);

            var i = msg.GetInt();
            Action action = () =>
            {
                Debug.Log("Print " + i);
            };
            manager.MsgHandler.HandleAction(msg_header.MsgIdx, action);
        }
        #endregion

        #region Server->Client SyncObjectAdd
        [MessageHandler(NetworkManager.SyncObjectAdd_MsgID)]
        private static void Receive_ServerSyncObjectAdd(Message msg)
        {
            var manager = NetworkManager.Client;
            if (NetworkManager.Instance.GameMessager == null)
            {
                Debug.LogError("Can not deal with SyncObject when 'NetworkManager.GameMessageClassType' was never defined properly with an IGameMessage.");
                return;
            }

            LinkedMessage.HandleIncomingMessage(msg, (LinkedMessage msg) =>
            {
                var msg_header = new MsgHeader(msg, out ClientManager manager);
                int length = msg.GetInt();
                Action action = () =>
                {
                    for (int j = 0; j < length; j++)
                    {
                        NetworkManager.HandleAddSyncObjectTransferAction(msg);
                    }
                };
                manager.MsgHandler.HandleAction(msg_header.MsgIdx, action);
            });
        }
        #endregion

        #region Server->Client SyncObjectRemove

        [MessageHandler(NetworkManager.SyncObjectRemove_MsgID)]
        private static void Receive_ServerSyncObjectRemove(Message msg)
        {
            var manager = NetworkManager.Client;
            if (NetworkManager.Instance.GameMessager == null)
            {
                Debug.LogError("Can not deal with SyncObject when 'NetworkManager.GameMessageClassType' was never defined properly with an IGameMessage.");
                return;
            }

            //- ToDo: Make this hole send and receive thing related to Client MSG Index, so that every client has a unqiue msg_index at the server and not one global for all clients
            LinkedMessage.HandleIncomingMessage(msg, (LinkedMessage msg) =>
            {
                var msg_header = new MsgHeader(msg, out ClientManager manager);
                int length = msg.GetInt();
                Action action = () =>
                {
                    for (int j = 0; j < length; j++)
                    {
                        NetworkManager.HandleRemoveSyncObjectTransferAction(msg);
                    }
                };
                manager.MsgHandler.HandleAction(msg_header.MsgIdx, action);
            });
        }
        #endregion

        #region Server->Client SyncObjectUpdate

        [MessageHandler(NetworkManager.SyncObjectUpdate_MsgID)]
        private static void Receive_ServerSyncObjectUpdate(Message msg)
        {
            var manager = NetworkManager.Client;
            if (NetworkManager.Instance.GameMessager == null)
            {
                Debug.LogError("Can not deal with SyncObject when 'NetworkManager.GameMessageClassType' was never defined properly with an IGameMessage.");
                return;
            }

            //- ToDo: Make this hole send and receive thing related to Client MSG Index, so that every client has a unqiue msg_index at the server and not one global for all clients
            LinkedMessage.HandleIncomingMessage(msg, (LinkedMessage msg) =>
            {
                var msg_header = new MsgHeader(msg, out ClientManager manager);
                int length = msg.GetInt();
                Action action = () =>
                {
                    for (int j = 0; j < length; j++)
                    {
                        NetworkManager.HandleUpdateSyncObjectTransferAction(msg);
                    }
                };
                manager.MsgHandler.HandleAction(msg_header.MsgIdx, action);
            });
        }
        #endregion
    }
}
