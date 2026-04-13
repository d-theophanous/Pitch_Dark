using Geecku.DefaultNetworking;
using Geecku.DefaultNetworking.Common.MessageHandlers;
using Geecku.GlobalMangers;
using NUnit.Framework;
using Riptide;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Daniel.Master
{
    public class GameManager : PersistantDSingleton<GameManager>
    {
        [SerializeField] List<string> SceneList;
        public PlayerScript Player;
        private GameState State;

        public int PlayerNumber => PlayerIdx + 1;
        public int PlayerIdx = 0;

        protected override void Awake()
        {
            base.Awake();
            if (WillBeDestroyed) return;

            //- Setup languages and everything for debug to be able to skip things
            State = GameState.WAITING;
        }
        protected override void Start()
        {
            base.Start();
            GlobalUIManager.Instance.ToggleNetworking();
        }

        protected override void Update()
        {
            switch (State)
            {
                case GameState.PLAYING:
                    Player.UpdatePlayer();
                    break;
                case GameState.CONNECT:
                    break;
                case GameState.WAITING:
                    break;
                default:
                    break;
            }
        }

        private void SwitchSceneToFirstInList()
        {
            if (SceneList.Count == 0 || SceneList[0] == "") return;
            StartCoroutine(SwitchScene(SceneList[0]));
        }
        private IEnumerator SwitchScene(string scene_name)
        {
            SceneManager.LoadScene(scene_name, LoadSceneMode.Additive);
            yield return new WaitForEndOfFrame();
            GlobalUIManager.Instance.ChangeMainCamera();
        }

        #region Game Logic
        public void StartGame()
        {
            StartCoroutine(AsyncStartGame());
            Content.transform.parent.gameObject.SetActive(false);
            Debug.Log("Game started :))");
        }
        private IEnumerator AsyncStartGame()
        {
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);
            State = GameState.PLAYING;
            GlobalUIManager.Instance.ToggleNetworking();

        }
        public bool OtherPlayerIsGateReady { get; private set; }
        private void OtherPlayerGateReady()
        {
            OtherPlayerIsGateReady = true;
            Debug.Log("other player ready");
        }
        public bool PlayerIsGateReady { get; private set; }
        private void PlayerGateReady()
        {
            PlayerIsGateReady = true;
            //- ToDo UI
            Debug.Log("player gate ready");
        }
        #endregion

        #region Networking

        [SerializeField] private int MaxPlayerCount;
        [SerializeField] private TMP_Text Content;
        private int PlayerCountWaiting;

        #region Connect
        public void SetUpNetworking()
        {
            PlayerCountWaiting = NetworkManager.Client.GetClientInfos().Length + 1;
            if (PlayerCountWaiting > MaxPlayerCount)
            {
                Debug.Log("Max Player Count already reached. Disconnected");
                QuitInGame();
                return;
            }
            PlayerIdx = PlayerCountWaiting - 1;

            NetworkManager.Client.OnClientConnected += Client_OnClientConnected;
            NetworkManager.Client.OnClientDisconnected += Client_OnClientDisconnected;

            WaitingForPlayer();
        }

        public void WaitingForPlayer()
        {
            State = GameState.CONNECT;
            Content.transform.parent.gameObject.SetActive(true);
            UpdatePlayerWaitingUI(PlayerCountWaiting);
        }
        private void UpdatePlayerWaitingUI(int player_count)
        {
            PlayerCountWaiting = player_count;
            if (PlayerCountWaiting == MaxPlayerCount)
            {
                //- ToDo
                StartGame();
                return;
            }
            Content.text = "Waiting for player: " + PlayerCountWaiting + "/" + MaxPlayerCount + "...";
        }
        #endregion

        #region Messages
        //- Player ready at door
        public void ClientSend_PlayerAtGate()
        {
            Message msg = Message.Create(MessageSendMode.Reliable, 1000);
            NetworkManager.Client.Send(msg);
        }
        [MessageHandler(1000)]
        private static void ServerReceive_PlayerAtGate(ushort client_id, Message msg)
        {
            Message new_msg = Message.Create(MessageSendMode.Reliable, 1001);
            NetworkManager.Server.MsgHandler.MessageIndex++;
            new_msg.AddInt(NetworkManager.Server.MsgHandler.MessageIndex);
            NetworkManager.Server.SendToAll(new_msg);
        }
        [MessageHandler(1001)]
        private static void ClientReceive_PlayerAtGate(ushort client_id, Message msg)
        {

            Action action;
            if (client_id == NetworkManager.Client.LocalClient.ID)
            {
                action = () =>
                {
                    Instance.PlayerGateReady();
                };
            }
            else
            {
                action = () =>
                {
                    Instance.OtherPlayerGateReady();
                };
            }
            ClientMessageHandler.Handle(msg, action);
        }
        #endregion

        #region Events

        private void UnsubscribeEvents()
        {
            NetworkManager.Client.OnClientConnected -= Client_OnClientConnected;
            NetworkManager.Client.OnClientDisconnected -= Client_OnClientDisconnected;
        }

        protected override void OnDestroy()
        {
            if (NetworkManager.Instance == null)
                return;
            UnsubscribeEvents();
            base.OnDestroy();
        }
        private void Client_OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            UpdatePlayerWaitingUI(PlayerCountWaiting + 1);
        }
        //- if player disconnects
        private void Client_OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            if (e.Id != 1)
                ClientDisconnected();
            else
                HostDisconnected();
        }
        #endregion

        #region Quit
        //ToDo
        public void Quit()
        {
            //- ToDo
            //Engine.SwitchScene("Connect");
        }
        public void QuitInGame()
        {
            Quit();
            NetworkManager.Client.StopPeer();
            ResetPlayer();
        }
        private void ResetPlayer()
        {
            PlayerCountWaiting = 0;
        }
        private void OtherClientDisconnected()
        {
            PlayerCountWaiting--;
        }
        //- ToDo
        private void ClientDisconnected()
        {
            OtherClientDisconnected();
            switch (State)
            {
                case GameState.PLAYING:
                    WaitingForPlayer();
                    break;
                case GameState.CONNECT:
                    UpdatePlayerWaitingUI(PlayerCountWaiting);
                    break;
                default:
                    break;
            }
        }
        private void HostDisconnected()
        {
            QuitInGame();
            //- ToDo
            //Daniel.Connect.ConnectScript.HostDisconnected = true;
        }
        #endregion

        #endregion

    }
    public enum GameState
    {
        PLAYING, CONNECT, WAITING
    }
}
