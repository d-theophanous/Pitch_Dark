using Geecku.DefaultNetworking;
using Geecku.DefaultNetworking.Common.MessageHandlers;
using Geecku.GlobalMangers;
using Riptide;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

namespace Daniel.Master
{
    public class GameManager : PersistantDSingleton<GameManager>
    {
        [SerializeField] List<string> SceneList;
        [SerializeField] ConnectScript ConnectScript;

        //[Header("References")]
        [HideInInspector] public PlayerScript Player;

        private GameState State;
        public static Language Language;

        //- Player 1 and Player 2
        public int PlayerNumber => PlayerIdx + 1;
        public int PlayerIdx = 0;

        //- Debug
        public Interactable DebugInteractable;

        protected override void Awake()
        {
            base.Awake();

            //- Setup languages and everything for debug to be able to skip things
            State = GameState.WAITING;
        }
        protected override void Start()
        {
            base.Start();
            GlobalUIManager.Instance.ToggleUI(UI_Group.PLAYER_SELECTION);

            SetGameLanguage((int)Language.ENGLISH);

            SubscribeEvents();
        }

        public event EventHandler UpdateEvent;
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
                case GameState.DIALOGUE:
                    DialogueManager.Instance.UpdateDialogue();
                    break;
                case GameState.SOLVING_PUZZLE:
                    break;
                case GameState.IMPROVISING:
                    break;
                default:
                    break;
            }
            UpdateEvent?.Invoke(this, null);
            AudioManager.Instance.UpdateAudio();

            //- Testing
        }
        public void SetGameState(GameState state) { State = state; }
        public void SetGameLanguage(int language) 
        { 
            Language = (Language)language;
            switch (Language)
            {
                case Language.ENGLISH:
                    LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[1];
                    break;
                case Language.DUTCH:
                    LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
                    break;
                case Language.GERMAN:
                    LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[2];
                    break;
                default:
                    break;
            }
        }
        public void SetInitialLanguage(int language)
        {
            GlobalUIManager.Instance.ToggleUI(UI_Group.ACCESSIBILITY_SELECTION, false);
            SetGameLanguage(language);
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
            GlobalUIManager.Instance.ToggleUI(UI_Group.NETWORK_CONNECT);
        }
        public void QuitWaitingForPuzzle()
        {
            GlobalUIManager.Instance.ToggleUI(UI_Group.NETWORK_GATE);
            //- Send other player message of cancelation
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
        //- for first puzzle (I know this is bad ToDo)
        public void SolveCurrentPuzzle()
        {
            PuzzleManager.Instance.SolvePuzzle();
            SetGameState(GameState.SOLVING_PUZZLE);
        }
        public void SolveSecondPuzzle(int interval)
        {
            PuzzleManager.Instance.SolveSecondPuzzle(interval);
            SetGameState(GameState.SOLVING_PUZZLE);
        }
        #endregion

        #region Networking

        [SerializeField] private int MaxPlayerCount;
        [SerializeField] private TMP_Text Content;
        private int PlayerCountWaiting;

        #region Gate Logic
        [SerializeField] private TMP_Text WaitingPlayerText;
        #endregion

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

        private void SubscribeEvents()
        {
            NetworkManager.Client.OnClientConnected += Client_OnClientConnected;
            NetworkManager.Client.OnClientDisconnected += Client_OnClientDisconnected;
            NetworkManager.Client.OnFailedConnection += Client_OnFailedConnection;
        }
        private void UnsubscribeEvents()
        {
            NetworkManager.Client.OnClientConnected -= Client_OnClientConnected;
            NetworkManager.Client.OnClientDisconnected -= Client_OnClientDisconnected;
            NetworkManager.Client.OnFailedConnection -= Client_OnFailedConnection;
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
        private void Client_OnFailedConnection(object sender, ConnectionFailedEventArgs e)
        {
            //-ToDo different cases handlen
            Debug.Log("in asdjf");
            Quit();
        }
        #endregion

        #region Quit
        //ToDo
        public void Quit()
        {
            Content.transform.parent.gameObject.SetActive(false);
            ConnectScript.ConnectionFailed();
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

        #region Debug
        public void SetPlayer(int player)
        {
            PlayerIdx = player;
            GlobalUIManager.Instance.ToggleUI(UI_Group.LANGUAGE_SELECTION, false);
        }
        public void DebugCurInteractable()
        {
            DebugInteractable.ActivatePrompt();
        }
        public void DebugSkipGate()
        {
            PuzzleManager.Instance.StartPuzzle();
        }
        #endregion
    }
    public enum GameState
    {
        PLAYING, CONNECT, WAITING, DIALOGUE, SOLVING_PUZZLE, IMPROVISING
    }
    public enum Language
    {
        ENGLISH, GERMAN, DUTCH
    }
}
