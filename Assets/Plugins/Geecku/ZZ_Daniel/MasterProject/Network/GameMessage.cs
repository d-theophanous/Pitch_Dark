using Geecku.DefaultNetworking;
using Geecku.DefaultNetworking.Attachables;
using Geecku.GlobalMangers;
using Riptide;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Daniel.Master
{
    public enum Owner { Server, Client }
    public enum NetworkTypes : ushort { Character, Map, Battle }
    public class GameMessage : Singleton<GameMessage>, IGameMessage //- ToDO: eine Singleton GameMessage classe erstellen!
    {
        #region Links
        [SerializeField, ReadOnly, FoldoutGroup("Links", Expanded = false)] private Transform ServerTransform;
        [SerializeField, ReadOnly, FoldoutGroup("Links")] private Transform ClientTransform;
        public void SetTransforms(Transform server_transform, Transform client_transform)
        {
            ServerTransform = server_transform;
            ClientTransform = client_transform;

            //- Init DataObject-Script
            var server = ServerTransform.gameObject.AddComponent<DataObject>();
            (Server = server).Init(true);
            var client = ClientTransform.gameObject.AddComponent<DataObject>();
            (Client = client).Init(false);
        }
        #endregion

        public static DataObject Client { get; private set; }
        public static DataObject Server { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            if (WillBeDestroyed)
                return;
            Engine.LogNetwork("GameMessage loaded.");
        }
        protected override void Start()
        {
            base.Start();
            if (WillBeDestroyed)
                return;
            //NetworkManager.Client.OnClientInitTransission += OnClientTransission;
            //NetworkManager.Server.OnConnectTransmission += OnClientConnectTransmission;
            //NetworkManager.Client.OnClientRdy += OnClientRdy;
            Engine.LogNetwork("GameMessage started.");
        }

        public void CreateSyncObject(LinkedMessage msg)
        {
            throw new NotImplementedException();
        }
    }
    public static class NetworkExtension
    {
        public static void AddHash(this Message msg, HashObject<NetworkTypes> _object)
        {
            msg.AddUShort(_object.HashID);
            msg.AddUShort((ushort)_object.Type);
            msg.AddBool(_object.ExtraValue != 0);
            if (_object.ExtraValue != 0)
            {
                msg.AddUShort(_object.ExtraValue);
            }
        }
        public static void AddHash(this Message msg, SyncObject<NetworkTypes> _object)
        {
            msg.AddUShort(_object.HashID);
            msg.AddUShort((ushort)_object.NetworkType);
            msg.AddBool(_object.ExtraValue != 0);
            if (_object.ExtraValue != 0)
            {
                msg.AddUShort(_object.ExtraValue);
            }
        }        
    }
}
