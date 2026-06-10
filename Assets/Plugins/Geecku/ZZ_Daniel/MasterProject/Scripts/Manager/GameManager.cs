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

        public bool SkipTutorial;
        [HideInInspector] public bool StartTutorial;

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
            MovementEvents = new();

            //- Setup languages and everything for debug to be able to skip things
            SetGameState(GameState.WAITING);
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

        //- for future games: implement states as classes!!!!
        public void SetGameState(GameState state) 
        {
            switch (State)
            {
                case GameState.PLAYING:
                    UnsubscribeMovementEvents();
                    break;
                case GameState.CONNECT:
                    break;
                case GameState.WAITING:
                    break;
                case GameState.DIALOGUE:
                    break;
                case GameState.SOLVING_PUZZLE:
                    break;
                case GameState.IMPROVISING:
                    break;
                default:
                    break;
            }
            State = state;
            switch (State)
            {
                case GameState.PLAYING:
                    Debug.Log("gamestate switch to playing");
                    SubscribeMovementEvents();
                    break;
                case GameState.CONNECT:
                    break;
                case GameState.WAITING:
                    break;
                case GameState.DIALOGUE:
                    break;
                case GameState.SOLVING_PUZZLE:
                    break;
                case GameState.IMPROVISING:
                    break;
                default:
                    break;
            }
        }
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
            //- ToDo switch back to accessibility option selection
            GlobalUIManager.Instance.ToggleUI(UI_Group.GENRE_SELECTION, false);
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

        #region Update Event Handling
        public List<EventHandler> MovementEvents;

        public void SubscribeMovementEvents()
        {
            foreach (EventHandler handler in MovementEvents)
            {
                UpdateEvent -= handler; //- make sure there are in there only once
                UpdateEvent += handler;
            }
        }
        private void UnsubscribeMovementEvents()
        {
            foreach (EventHandler handler in MovementEvents)
            {
                UpdateEvent -= handler;
            }
        }
        public void AddEventAndSubscribe(List<EventHandler> events, EventHandler handler)
        {
            events.Add(handler);
            UpdateEvent += handler;
        }

        #endregion

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
            GlobalUIManager.Instance.ToggleUI(UI_Group.NETWORK_CONNECT);

            if (SkipTutorial)
            {
                Debug.Log("ja in skip tutorial");
                SetGameState(GameState.PLAYING);
            }
            else
            {

            }
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

        //- Message numbers
        private const ushort PlayerGateIDServer = 1000;
        private const ushort PlayerGateIDClient = 1001;
        private const ushort DirectionCheckIDServer = 1002;
        private const ushort DirectionCheckIDClient = 1003;

        #region Template functions for sending other player a message
        public void ClientSend_()
        {
            Message msg = Message.Create(MessageSendMode.Reliable, 0);
            NetworkManager.Client.Send(msg);
        }
        [MessageHandler(0)]
        private static void ServerReceive_(ushort client_id, Message msg)
        {
            Message new_msg = Message.Create(MessageSendMode.Reliable, 1);
            NetworkManager.Server.MsgHandler.MessageIndex++;
            new_msg.AddInt(NetworkManager.Server.MsgHandler.MessageIndex);
            NetworkManager.Server.SendToAll(new_msg);
        }
        [MessageHandler(1)]
        private static void ClientReceive_(ushort client_id, Message msg)
        {

            Action action;
            if (client_id == NetworkManager.Client.LocalClient.ID)
            {
                action = () =>
                {
                    //- logic for what player who sent the message should do with it
                };
            }
            else
            {
                action = () =>
                {
                    //- logic for other player
                };
            }
            ClientMessageHandler.Handle(msg, action);
        }
        #endregion

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
        //- Player helps other person navigate
        public void ClientSend_DirectionCheck(ushort content)
        {
            Message msg = Message.Create(MessageSendMode.Reliable, DirectionCheckIDServer);
            msg.AddUShort(content);
            NetworkManager.Client.Send(msg);
        }
        [MessageHandler(DirectionCheckIDServer)]
        private static void ServerReceive_DirectionCheck(ushort client_id, Message msg)
        {
            var content = msg.GetUShort();
            Message new_msg = Message.Create(MessageSendMode.Reliable, DirectionCheckIDClient);
            NetworkManager.Server.MsgHandler.MessageIndex++;

            new_msg.AddInt(NetworkManager.Server.MsgHandler.MessageIndex);
            new_msg.AddUShort(client_id);
            new_msg.AddUShort(content);
            NetworkManager.Server.SendToAll(new_msg);
        }
        [MessageHandler(DirectionCheckIDClient)]
        private static void ClientReceive_DirectionCheck(Message msg)
        {
            Action action = () =>
            {
                var client_id = msg.GetUShort();
                var content = msg.GetUShort();
                Debug.Log("content in client receive: " + content);
                if (client_id == NetworkManager.Client.LocalClient.ID)
                {
                    Debug.Log("sent: " + content);
                }
                else
                {
                    if (content == 0)
                    {
                        Debug.Log("not rumbling");
                        Rumbler.Instance.StopRumble();
                    }
                    else if (content == 1)
                    {
                        Rumbler.Instance.StartRumble();
                        Debug.Log("rumbling");
                    }
                }
            };            
            ClientMessageHandler.Handle(msg, action);
        }

        //- Player ready at door
        public void ClientSend_PlayerAtGate()
        {
            Message msg = Message.Create(MessageSendMode.Reliable, PlayerGateIDServer);
            NetworkManager.Client.Send(msg);
        }
        [MessageHandler(PlayerGateIDServer)]
        private static void ServerReceive_PlayerAtGate(ushort client_id, Message msg)
        {
            Message new_msg = Message.Create(MessageSendMode.Reliable, PlayerGateIDClient);
            NetworkManager.Server.MsgHandler.MessageIndex++;
            new_msg.AddInt(NetworkManager.Server.MsgHandler.MessageIndex);
            new_msg.AddUShort(client_id);
            NetworkManager.Server.SendToAll(new_msg);
        }
        [MessageHandler(PlayerGateIDClient)]
        private static void ClientReceive_PlayerAtGate(Message msg)
        {
            var client_id = msg.GetUShort();
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
        NONE, PLAYING, CONNECT, WAITING, DIALOGUE, SOLVING_PUZZLE, IMPROVISING
    }
    public enum Language
    {
        ENGLISH, GERMAN, DUTCH
    }
}
