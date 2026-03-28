using Riptide;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Geecku.DefaultNetworking.Attachables
{
    public class NDictionary<Key, Value> : Dictionary<Key, Value>
    {
        public new Value this[Key index]
        {
            get
            {
                if (TryGetValue(index, out Value value))
                    return value;
                return default(Value);
            }
            set
            {
                base[index] = value;
            }
        }
    }
    public struct HashObject<T> where T : Enum
    {
        public string Name;
        public ushort HashID;
        public ushort ExtraValue;
        public T Type;

        public HashObject(ushort id, T type, ushort extra_value, string name)
        {
            Name = name;
            HashID = id;
            Type = type;
            ExtraValue = extra_value;
        }
        public HashObject(ushort id, T type, ushort extra_value)
        {
            Name = "Empty";
            HashID = id;
            Type = type;
            ExtraValue = extra_value;
        }

        public override string ToString()
        {
            return HashID + " " + Type + (ExtraValue != 0 ? " " + ExtraValue : string.Empty);
        }
    }
    public abstract class DataObject<T, SyncObj> : SerializedMonoBehaviour where T : Enum where SyncObj : SyncObject<T>
    {
        //private GameMessage Game => GameMessage.Instance;
        [ShowInInspector] public bool IsClient => !IsServer;
        [ShowInInspector, ReadOnly] public bool IsServer { get; private set; }

        private Dictionary<T, NDictionary<ushort, SyncObj>> ClientDic;
        private Dictionary<T, NDictionary<ushort, SyncObj>> ServerDic;

        [HideInInspector] public Dictionary<T, NDictionary<ushort, SyncObj>> Dictionary => IsServer ? ServerDic : ClientDic;

        public virtual void Init(bool is_server)
        {
            IsServer = is_server;
            //IsClient = !IsServer;

            //- Sets
            if (IsClient)
                ClientDic = new();
            if (IsServer)
                ServerDic = new();
        }

        public SyncObj AddSyncObject(SyncObj sync_object)
        {
            if (sync_object == null)
            {
                Debug.LogError("Given sync_object was null. Maybe you missed to add proper handling of creating it in your 'DataObject.CreateSyncObjectFromHash' Methode");
                return null;
            }
            if (!Dictionary.ContainsKey(sync_object.NetworkType))
                Dictionary.Add(sync_object.NetworkType, new NDictionary<ushort, SyncObj>());
            if (Dictionary[sync_object.NetworkType].ContainsKey(sync_object.HashID))
            {
                Debug.LogError("The HashID: " + sync_object.HashID + " for " + sync_object.NetworkType 
                    + " already exists. Returning the already existing object");
                //Destroy(sync_object.gameObject);
                return Dictionary[sync_object.NetworkType][sync_object.HashID];
            }
            Dictionary[sync_object.NetworkType].Add(sync_object.HashID, sync_object);
            sync_object.transform.SetParent(transform);
            return sync_object;
        }
        public SyncObj AddSyncObject(HashObject<T> hash)
        {
            var sync_object = GetSyncObject(hash, this);
            if (sync_object != null)
            {
                Debug.LogError("Sync object is already contained. Transmissions can not be used to update sync objects.");
                return null;
            }
            return AddSyncObject(CreateSyncObjectFromHash(hash));
        }
        public SyncObj AddSyncObject(T type, ushort hash_id)
        {
            return AddSyncObject(new HashObject<T>(hash_id, type, 0));
        }
        public SyncObj AddSyncObject(T type)
        {
            var local = AddSyncObject(new HashObject<T>(0, type, 0));
            local.Initialize();
            return local;
        }
        protected abstract SyncObj CreateSyncObjectFromHash(HashObject<T> hash);

        public bool RemoveSyncObject(HashObject<T> hash, bool destroy_object = false)
        {
            return RemoveSyncObject(GetSyncObject(hash, this), destroy_object);
        }
        public bool RemoveSyncObject(SyncObj sync_object, bool destroy_object = false)
        {
            if (!Dictionary.ContainsKey(sync_object.NetworkType))
            {
                Debug.LogError("Missing DataObject entry for type " + sync_object.NetworkType.ToString() + ". Was the NetworkType registered at any point at all?");
                return false;
            }
            if (!Dictionary[sync_object.NetworkType].ContainsKey(sync_object.HashID))
            {
                Debug.LogError("Missing DataObject entry for hashid " + sync_object.HashID + ". Was the object already removed successfully?");
                return false;
            }

            DeleteSyncObject(sync_object);
            Dictionary[sync_object.NetworkType].Remove(sync_object.HashID);
            SyncObject<T>.RemoveFromGlobalIDList(sync_object.HashID);
            if (destroy_object)
                Destroy(sync_object.gameObject);
            return true;
        }
        protected virtual void DeleteSyncObject(SyncObj sync_object) { }

        public static SyncObj GetSyncObject(HashObject<T> hash, DataObject<T, SyncObj> data_object)
        {
            if (!data_object.Dictionary.ContainsKey(hash.Type))
            {
                //Debug.LogWarning("Missing DataObject entry for type " + hash.Type.ToString() + ". Returning null");
                return null;
            }

            var dic = data_object.Dictionary[hash.Type];
            if (dic.ContainsKey(hash.HashID))
                return dic[hash.HashID];
            return null;
        }
        public SyncObj GetSyncObject(HashObject<T> hash)
        {
            return GetSyncObject(hash, this);
        }
        public SyncObj GetSyncObject(T type, ushort hash_id)
        {
            return GetSyncObject(new HashObject<T>(hash_id, type, 0));
        }
        public bool HasSyncObject(T type, ushort hash_id)
        {
            return Dictionary.ContainsKey(type) && Dictionary[type].ContainsKey(hash_id);
        }

        #region SyncObjectPrefabs
        protected K Instantiate<K>(HashObject<T> hash) where K : SyncObject<T> 
        {
            var local = NetworkManager.Instantiate<K>(transform);
            local.HashID = hash.HashID; //-   HashID is no longer handled locally in SyncObject.ToLinkedMessage()
            local.ExtraValue = hash.ExtraValue;
            local.name = hash.Name;
            return local;
        }
        #endregion

        #region Map
        //public Map AddMap(ushort hash_id)
        //{
        //    if (MapDic.ContainsKey(hash_id))
        //    {
        //        Debug.LogError("Double key for map " + hash_id);
        //    }
        //    var prefab = Game.MapPrefab;
        //    var map = Instantiate(prefab, transform).Create(hash_id);
        //    MapDic.Add(hash_id, map);
        //    return map;
        //}
        #endregion
        #region Character
        //public Character AddCharacter(Characters char_id, ushort hash_id)
        //{
        //    if (CharDic.ContainsKey(hash_id))
        //    {
        //        Debug.LogError("Double key for character " + hash_id);
        //    }
        //    Character prefab;
        //    ObjCharacter obj_char = ResourceManager.GetCharacter(char_id);
        //    if (obj_char != null && obj_char.Character != null)
        //        prefab = obj_char.Character;
        //    else
        //    {
        //        Debug.LogWarning("No Character-Data was found for " + char_id + ". Using default one.");
        //        if (char_id >= ObjCharacter.StartOfEnemyIndex)
        //            prefab = Game.EnemyPrefab;
        //        else
        //            prefab = Game.FigurePrefab;
        //    }
        //    var character = Instantiate(prefab, transform).Create(char_id, hash_id);
        //    CharDic.Add(hash_id, character);
        //    return character;
        //}
        #endregion
        #region Battle
        //public Battle AddBattle(ushort hash_id)
        //{
        //    if (BattleDic.ContainsKey(hash_id))
        //    {
        //        Debug.LogError("Double key for map " + hash_id);
        //    }
        //    var prefab = Game.BattlePrefab;
        //    var map = Instantiate(prefab, transform).Create(hash_id);
        //    BattleDic.Add(hash_id, map);
        //    return map;
        //}
        #endregion
    }
}
