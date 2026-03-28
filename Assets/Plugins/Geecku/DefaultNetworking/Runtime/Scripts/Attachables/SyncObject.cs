using Geecku.GlobalMangers;
using Riptide;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Geecku.DefaultNetworking.Attachables
{
    public abstract class DataObjectChild<T, SyncObj> : MonoBehaviour where T : Enum where SyncObj : SyncObject<T>
    {
        protected bool OwnedByClient => GameData != null && GameData.IsClient;
        protected bool OwnedByServer => GameData != null && GameData.IsServer;
        public DataObject<T, SyncObj> GameData => GetComponentInParent<DataObject<T, SyncObj>>();
    }
    public abstract class SyncObject<T> : DataObjectChild<T, SyncObject<T>> where T : Enum
    {
        #region Random Hash
        public static List<ushort> IDList = new();
        private static System.Random Random => Engine.Random;
        public static ushort GenerateHashID()
        {
            ushort local = 0;
            int i = 0;
            const int max_itter = 10000;
            while (i < max_itter && IDList.Contains(local = (ushort)Random.Next(1, ushort.MaxValue))) { i++; }
            if (i >= max_itter)
            {
                Debug.LogError("Max itter reached in sync_object hash generator.");
                return 0;
            }
            return local;
        }
        public static void RemoveFromGlobalIDList(ushort hash_id)
        {
            IDList.Remove(hash_id);
        }
        public static void AddToGlobalIDList(ushort hash)
        {
            if (IDList.Contains(hash))
            {
                Debug.LogWarning("Trying to add a doubled hash manually");
                return;
            }
            IDList.Add(hash);
        }
        public static void RemoveFromGlobalIDList(List<ushort> hashs)
        {
            foreach (var item in hashs)
                RemoveFromGlobalIDList(item);
        }
        #endregion

        #region Network Variables
        [BoxGroup("Network Variables"), ShowInInspector, ReadOnly] public abstract ushort HashID { get; set; }
        [BoxGroup("Network Variables"), ShowInInspector, ReadOnly] public virtual ushort ExtraValue { get; set; }
        [BoxGroup("Network Variables"), ShowInInspector] public abstract T NetworkType { get; }
        #endregion

        protected virtual void Awake()
        {
            LoadMethodes();
        }
        #region Create
        public virtual void Initialize() { }
        #endregion

        #region LinkedMessages
        /// <summary>
        /// Is used to 'pack' data into a package which will be send across the network to be disassambled.
        ///  -> base.ToLinkedMessage can be ignored
        /// </summary>
        public virtual void ToLinkedMessage(LinkedMessage msg) { }
        /// <summary>
        /// Is used to 'unpack' data from a package.
        ///  -> base.ApplyLinkedMessage can be ignored
        /// </summary>
        public virtual void ApplyLinkedMessage(LinkedMessage msg) { }
        #endregion

        #region Sendable Methodes
        public List<MethodInfo> List;

        protected void LoadMethodes()
        {
            List = this.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttribute<SendableAttribute>() != null)
                .ToList();
        }
        protected MethodePacket SafeMethode(string methode_name, params object[] args)
        {
            //MethodInfo method = GetType().GetMethod(methode_name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            MethodePacket packet = new();
            //MethodInfo method = this.GetType().GetMethod(nameof(methode));
            packet.HashID = this.HashID;
            bool found = false;
            for (int i = 0; i < List.Count; i++)
            {
                if (List[i].Name == methode_name && List[i].GetParameters().Length == args.Length)
                {
                    found = true;
                    packet.Index = (ushort)i;
                    break;
                }
                //if (List[i].Equals(method))
                //{
                //    found = true;
                //    packet.Index = (ushort)i;
                //    break;
                //}
            }
            if (!found)
            {
                foreach (var method in List)
                {
                    Debug.Log($"Methode gefunden: {method.Name}, Attribute: {method.GetCustomAttribute<SendableAttribute>()}");
                }
                Debug.LogError("Method was not found in list.");
                return null;
            }
            packet.Arguments = args;

            return packet;
        }
        protected void ExecuteMethode(int index, params object[] args)
        {
            if (index < List.Count)
            {
                List[index].Invoke(this, args);
            }
            else
            {
                foreach (var method in List)
                {
                    Debug.Log($"Methode gefunden: {method.Name}, Attribute: {method.GetCustomAttribute<SendableAttribute>()}");
                }
                Debug.LogError("Methode not found in list at index " + index);
            }
        }
        #endregion
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class SendableAttribute : Attribute
    {

    }
    public class MethodePacket
    {
        public ushort HashID;
        public ushort Index;
        public object[] Arguments;

        public void AddToMessage(Message msg)
        {
            msg.AddUShort(HashID);
            msg.AddUShort(Index);
            if (Arguments == null)
                msg.AddByte(0);
            else
            {
                msg.AddByte((byte)Arguments.Length);
                foreach (var arg in Arguments)
                {
                    if (arg is int i)
                    {
                        msg.AddByte(1);
                        msg.AddInt(i);
                    }
                    else if (arg is uint ui)
                    {
                        msg.AddByte(2);
                        msg.AddUInt(ui);
                    }
                    else if (arg is ushort us)
                    {
                        msg.AddByte(3);
                        msg.AddUShort(us);
                    }
                    else if (arg is short s)
                    {
                        msg.AddByte(4);
                        msg.AddShort(s);
                    }
                    else if (arg is long l)
                    {
                        msg.AddByte(5);
                        msg.AddLong(l);
                    }
                    else if (arg is ulong ul)
                    {
                        msg.AddByte(6);
                        msg.AddULong(ul);
                    }
                    else if (arg is bool b)
                    {
                        msg.AddByte(7);
                        msg.AddBool(b);
                    }
                    else if (arg is float f)
                    {
                        msg.AddByte(8);
                        msg.AddFloat(f);
                    }
                    else if (arg is double d)
                    {
                        msg.AddByte(9);
                        msg.AddDouble(d);
                    }
                    else if (arg is byte by)
                    {
                        msg.AddByte(10);
                        msg.AddByte(by);
                    }
                    else if (arg is string txt)
                    {
                        msg.AddByte(11);
                        msg.AddString(txt);
                    }
                    else if (arg is bool[] b_array)
                    {
                        msg.AddByte(12);
                        msg.AddInt(b_array.Length);
                        for (int k = 0; k < b_array.Length; k++)
                            msg.AddBool(b_array[k]);
                    }
                    else if (arg is int[] i_array)
                    {
                        msg.AddByte(13);
                        msg.AddInt(i_array.Length);
                        for (int k = 0; k < i_array.Length; k++)
                            msg.AddInt(i_array[k]);
                    }
                    else if (arg is float[] f_array)
                    {
                        msg.AddByte(14);
                        msg.AddInt(f_array.Length);
                        for (int k = 0; k < f_array.Length; k++)
                            msg.AddFloat(f_array[k]);
                    }
                    else
                    {
                        msg.AddByte(byte.MaxValue);
                        Debug.LogWarning("I can not encode type " + arg.GetType().ToString());
                    }
                }
            }
        }
        public static MethodePacket ApplyMessage(Message msg)
        {
            MethodePacket local = new MethodePacket();
            local.HashID = msg.GetUShort();
            local.Index = msg.GetUShort();
            byte length = msg.GetByte();
            if (length > 0)
            {
                local.Arguments = new object[length];
                for (int i = 0; i < local.Arguments.Length; i++)
                {
                    byte identifier = msg.GetByte();
                    switch (identifier)
                    {
                        case 1:
                            local.Arguments[i] = msg.GetInt();
                            break;
                        case 2:
                            local.Arguments[i] = msg.GetUInt();
                            break;
                        case 3:
                            local.Arguments[i] = msg.GetUShort();
                            break;
                        case 4:
                            local.Arguments[i] = msg.GetShort();
                            break;
                        case 5:
                            local.Arguments[i] = msg.GetLong();
                            break;
                        case 6:
                            local.Arguments[i] = msg.GetULong();
                            break;
                        case 7:
                            local.Arguments[i] = msg.GetBool();
                            break;
                        case 8:
                            local.Arguments[i] = msg.GetFloat();
                            break;
                        case 9:
                            local.Arguments[i] = msg.GetDouble();
                            break;
                        case 10:
                            local.Arguments[i] = msg.GetByte();
                            break;
                        case 11:
                            local.Arguments[i] = msg.GetString();
                            break;
                        case 12:
                            bool[] b_array = new bool[msg.GetInt()];
                            for (int k = 0; k < b_array.Length; k++)
                                b_array[k] = msg.GetBool();
                            local.Arguments[i] = b_array;
                            break;
                        case 13:
                            int[] i_array = new int[msg.GetInt()];
                            for (int k = 0; k < i_array.Length; k++)
                                i_array[k] = msg.GetInt();
                            local.Arguments[i] = i_array;
                            break;
                        case 14:
                            float[] f_array = new float[msg.GetInt()];
                            for (int k = 0; k < f_array.Length; k++)
                                f_array[k] = msg.GetFloat();
                            local.Arguments[i] = f_array;
                            break;
                        default:
                            Debug.LogWarning("Unknown type read. Ignoring it.");
                            break;
                    }
                }
            }
            return local;
        }
    }
}
