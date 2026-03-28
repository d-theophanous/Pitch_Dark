using Geecku;
using Geecku.DefaultNetworking;
using Geecku.GlobalMangers;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Daniel.Master
{
    public class ConnectScript : MonoBehaviour
    {
        public const string LOCALHOST = "127.0.0.1";
        #region UI
        [SerializeField] private TMP_InputField IPInput;
        [SerializeField] private Button HostButton;
        [SerializeField] private Button ConnectButton;
        [SerializeField] private TMP_Text ConnectInfo;
        //[SerializeField] private UIScript HostDisconUI;

        public static bool HostDisconnected;

        public void Host()
        {
            NetworkManager.Instance.IP_Address = LOCALHOST;
            NetworkManager.StartServer();
            NetworkManager.StartClient();

            Setup();
        }
        //- ToDo connect failed fall einbauen
        //- Drei Spielerproblem lösen (z.B. events deabonnieren)
        //- Einbauen für Basis: Relay von Client Nachricht an alle CLients automatisieren
        public void Connect()
        {
            var ip_address = GetIPAddress();
            if (ip_address == null)
                return;
            NetworkManager.Instance.IP_Address = ip_address;
            NetworkManager.StartClient();

            Setup();
        }
        private int PlayerCount => LocalClientCount;
        public void UpdatePlayers()
        {
            ConnectInfo.text = PlayerCount + " Player(s)";
        }
        private void Setup()
        {
            HostButton.interactable = false;
            ConnectButton.interactable = false;
            IPInput.interactable = false;
            ConnectInfo.gameObject.SetActive(true);
            UpdatePlayers();
        }
        private string GetIPAddress()
        {
            return IPInput.text;
        }

        #endregion

        private void Awake()
        {
            ConnectInfo.gameObject.SetActive(false);
            IPInput.text = LOCALHOST;
        }
        private void Start()
        {
            if (HostDisconnected)
            {
                //HostDisconUI.Show();
                HostDisconnected = false;
            }            

            NetworkManager.Client.OnClientConnected += Client_OnClientConnected;
            NetworkManager.Client.OnClientDisconnected += Client_OnClientDisconnected;

            NetworkManager.Client.OnReceiveClientReady += Client_OnReceiveClientReady;
        }
        private void Client_OnReceiveClientReady(object sender, bool e)
        {
            LocalClientCount = NetworkManager.Client.GetClientInfos().Length + 1;
            StopAllCoroutines();
            //TikTakToeManager.LoadTTT(this);
            Engine.SwitchScene("TikTakToeScene");
        }

        private void OnDestroy()
        {
            if (NetworkManager.Instance == null)
                return;
            NetworkManager.Client.OnClientConnected -= Client_OnClientConnected;
            NetworkManager.Client.OnClientDisconnected -= Client_OnClientDisconnected;

            NetworkManager.Client.OnReceiveClientReady -= Client_OnReceiveClientReady;
        }


        //private IEnumerator CheckInitialConnection()
        //{
        //    yield return new WaitForSeconds(
        //    NetworkManager.Client.LocalClient.TCP.TimeoutTime);
        //    if (!NetworkManager.Client.LocalClient.TCP.IsConnected)
        //    {
        //        NetworkManager.Client.StopPeer();
        //        Debug.Log("Server may not be active yet. Try again later");
        //        HostButton.interactable = true;
        //        ConnectButton.interactable = true;
        //        IPInput.interactable = true;
        //        ConnectInfo.gameObject.SetActive(false);
        //    }
        //}

        /*
         * ClientManager: bei ClientHello / ServerHello Basisinformationen transferieren 
         * z.B. wie viele Spieler verbunden sind...
         * Nächsten Zeilen werden beibehalten (OnClientConnected)
         */
        private int LocalClientCount;
        private void Client_OnClientConnected(object sender, Riptide.ClientConnectedEventArgs e)
        {
            LocalClientCount++;
        }
        private void Client_OnClientDisconnected(object sender, Riptide.ClientDisconnectedEventArgs e)
        {
            LocalClientCount--;
        }


        private void Update()
        {
            UpdatePlayers();
        }
    }
}
