using Geecku.DefaultNetworking;
using Geecku.GlobalMangers;
using NUnit.Framework;
using Riptide;
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
        [SerializeField] private PlayerScript Player;

        public int PlayerNumber => PlayerIdx + 1;
        public int PlayerIdx = 0;

        protected override void Awake()
        {
            base.Awake();
            if (WillBeDestroyed) return;

            //- Setup languages and everything for debug to be able to skip things
        }
        protected override void Start()
        {
            base.Start();
            GlobalUIManager.Instance.ToggleNetworking();
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
        private void StartGame()
        {
            SceneManager.LoadScene("Game", LoadSceneMode.Additive);
            Content.transform.parent.gameObject.SetActive(false);
            GlobalUIManager.Instance.ToggleNetworking();
            Debug.Log("Game started :))");
        }
        #endregion

        #region Networking

        [SerializeField] private int MaxPlayerCount;
        [SerializeField] private TMP_Text Content;
        private int PlayerCountWaiting;
        private GameState State;
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
        PLAYING, CONNECT
    }
}
