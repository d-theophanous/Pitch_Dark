using Riptide;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Geecku.DefaultNetworking
{
    public static class Messages
    {
        public static void Send(this Message msg, Client sender)
        {
            if (sender.IsNotConnected)
            {
                Debug.LogWarning("Client is not connected");
                return;
            }
            sender.Send(msg);
        }
        public static void Send(this Message msg, Server sender)
        {
            if (!sender.IsRunning)
            {
                Debug.LogWarning("Server is not running");
                return;
            }
            sender.SendToAll(msg);
        }
        public static void SendToAll(this Message msg, Server sender) => Send(msg, sender);
        public static void Send(this Message msg, Server sender, ushort client_id)
        {
            sender.Send(msg, client_id);
        }
        public static void Send(this Message msg, Server sender, params ushort[] client_ids)
        {
            for (int i = 0; i < client_ids.Length; i++)
            {
                var client_id = client_ids[i];
                sender.Send(msg, client_id, false);
            }
            msg.Release();
        }

        #region KeyValue Pair
        public static void AddKeyValuePair(this Message msg, KeyValuePair<short, short> pair)
        {
            msg.AddShort(pair.Key);
            msg.AddShort(pair.Value);
        }
        public static KeyValuePair<short, short> GetKeyValuePair(this Message msg)
        {
            return new KeyValuePair<short, short>(msg.GetShort(), msg.GetShort());
        }
        #endregion
    }

    public class ConfirmedMessage
    {

    }

    public class ChainedMessage
    {

    }

    /// <summary>
    /// Used for automated download messages between client and server.
    /// <br/>
    /// Dynamically splits <see cref="Message"/> according to their content size.
    /// </summary>
    public class LinkedMessage
    {
        private const int RandomSeed = 134214;
        private static System.Random Random;
        internal static readonly List<ushort> InUseList = new();
        private readonly ushort LinkedMsgID;
        private int MaxBitsPerMessage => NetworkManager.MaxPacketSize;
        private readonly List<Message> Messages = new();
        private Message Current;
        private int CurrentPacketNum;
        private int PacketNum
        {
            get => CurrentPacketNum;
            set
            {
                CurrentPacketNum = value;
                TotalPacketNum += value;
            }
        }
        private int TotalPacketNum;
        private int RestExpectedTotalPackets;
        private int OrderIndex;
        private readonly bool ReadOnly;

        private ushort MsgID;
        private LinkedMessage()
        {
            if (Random == null)
                Random = new System.Random(RandomSeed);
            ushort tmp_id = 0;
            const int max_itter = 10000;
            int i = 0;
            while (i < max_itter && (InUseList.Contains(tmp_id = (ushort)Random.Next(ushort.MaxValue)) || tmp_id == 0)) { i++; }
            if (i >= max_itter)
            {
                Debug.LogError("Max Itter (" + i + ") for linked Message reached.");
                throw new System.Exception("Max Itter reached for LinkedMessage");
            }
            LinkedMsgID = tmp_id;
        }
        private LinkedMessage(ushort linked_msg_id)
        {
            if (InUseList.Contains(linked_msg_id))
            {
                Debug.LogWarning("LinkedMsgID is already in use. But its not that bad if its readonly.");
            }
            else
                InUseList.Add(linked_msg_id);
            LinkedMsgID = linked_msg_id;
            ReadOnly = true;
        }
        public static LinkedMessage Create(MessageSendMode send_mode, ushort id)
        {
            return Create(id);
        }
        public static LinkedMessage Create(ushort id)
        {
            LinkedMessage msg = new LinkedMessage();
            msg.MsgID = id;
            msg.Current = Message.Create(MessageSendMode.Reliable, id);
            return msg;
        }

        #region Get-Methodes
        private T PackMessageAndRemoveFromList<T>(Func<T> getter, int bits)
        {
            if (RestExpectedTotalPackets == 0)
            {
                throw new Exception("Expected total packets is already zero, do not try to get any more data, there is nothing left!");
            }
            if (PacketNum == 0 || Current == null)
            {
                if (Messages.Count == 0)
                    throw new Exception("Trying to read further data from a already emptied linkedmessage. ID: " + LinkedMsgID);

                Current = Messages[0];
                Messages.RemoveAt(0);
                var msg_id = Current.GetUShort();
                var packets = Current.GetInt();
                PacketNum = packets;
                Current.GetByte();
                if (msg_id >= NetworkManager.MaxMsgIDLimitForTwoBytesInHeader)
                    Current.GetByte();
                if (msg_id >= NetworkManager.MaxMsgIDLimitForThreeBytesInHeader)
                    Current.GetByte();
            }
            if (Current.UnreadBits >= bits)
            {
                var value = getter();
                PacketNum--;
                RestExpectedTotalPackets--;
                if (PacketNum == 0)
                {
                    Current.Release();
                    Current = null;
                }
                return value;
            }
            else
                throw new Exception("Trying to get bits from a message that is not big enough. ID: " + LinkedMsgID);
        }
        public bool GetBool()
        {
            const int bits = 1;
            return PackMessageAndRemoveFromList(() => Current.GetBool(), bits);
        }
        public byte GetByte()
        {
            const int bits = 8;
            return PackMessageAndRemoveFromList(() => Current.GetByte(), bits);
        }
        public ushort GetUShort()
        {
            const int bits = 16;
            return PackMessageAndRemoveFromList(() => Current.GetUShort(), bits);
        }
        public short GetShort()
        {
            const int bits = 16;
            return PackMessageAndRemoveFromList(() => Current.GetShort(), bits);
        }
        public uint GetUInt()
        {
            const int bits = 32;
            return PackMessageAndRemoveFromList(() => Current.GetUInt(), bits);
        }
        public int GetInt()
        {
            const int bits = 32;
            return PackMessageAndRemoveFromList(() => Current.GetInt(), bits);
        }
        public ulong GetULong()
        {
            const int bits = 64;
            return PackMessageAndRemoveFromList(() => Current.GetULong(), bits);
        }
        public long GetLong()
        {
            const int bits = 64;
            return PackMessageAndRemoveFromList(() => Current.GetLong(), bits);
        }
        public float GetFloat()
        {
            const int bits = 32;
            return PackMessageAndRemoveFromList(() => Current.GetFloat(), bits);
        }
        public double GetDouble()
        {
            const int bits = 64;
            return PackMessageAndRemoveFromList(() => Current.GetDouble(), bits);
        }
        public string GetString()
        {
            int length = GetInt();
            byte[] array = new byte[length];
            for (int i = 0; i < length; i++)
                array[i] = GetByte();
            if (length == 0)
                return "";
            return System.Text.Encoding.UTF8.GetString(array);
        }
        #endregion

        #region Add-Methodes
        private void PackMessageAndAddToList(bool create_new = true)
        {
            OrderIndex++;
            var msg = Message.Create(MessageSendMode.Reliable, MsgID);
            msg.AddUShort(LinkedMsgID);
            msg.AddBool(false); //- Is Header
            msg.AddInt(OrderIndex);

            msg.AddUShort(MsgID);
            msg.AddInt(PacketNum);
            msg.AddMessage(Current);
            Messages.Add(msg);
            Current.Release();

            if (create_new)
            {
                Current = Message.Create(MessageSendMode.Reliable, MsgID);
                PacketNum = 1;
                return;
            }
            PacketNum = 0;
            Current = null;
        }
        public void AddBool(bool value)
        {
            if (ReadOnly)
            {
                Debug.LogWarning("Trying to add a value to a readonly LinkedMessage.");
                return;
            }
            const int bits = 1;
            if (Current.WrittenBits + bits < MaxBitsPerMessage)
                PacketNum++;
            else
                PackMessageAndAddToList();
            Current.AddBool(value);
        }
        public void AddByte(byte value)
        {
            if (ReadOnly)
            {
                Debug.LogWarning("Trying to add a value to a readonly LinkedMessage.");
                return;
            }
            const int bits = 8;
            if (Current.WrittenBits + bits < MaxBitsPerMessage)
                PacketNum++;
            else
                PackMessageAndAddToList();
            Current.Add(value);
        }
        public void AddUShort(ushort value)
        {
            if (ReadOnly)
            {
                Debug.LogWarning("Trying to add a value to a readonly LinkedMessage.");
                return;
            }
            const int bits = 16;
            if (Current.WrittenBits + bits < MaxBitsPerMessage)
                PacketNum++;
            else
                PackMessageAndAddToList();
            Current.Add(value);
        }
        public void AddShort(short value)
        {
            if (ReadOnly)
            {
                Debug.LogWarning("Trying to add a value to a readonly LinkedMessage.");
                return;
            }
            const int bits = 16;
            if (Current.WrittenBits + bits < MaxBitsPerMessage)
                PacketNum++;
            else
                PackMessageAndAddToList();
            Current.Add(value);
        }
        public void AddUInt(uint value)
        {
            if (ReadOnly)
            {
                Debug.LogWarning("Trying to add a value to a readonly LinkedMessage.");
                return;
            }
            const int bits = 32;
            if (Current.WrittenBits + bits < MaxBitsPerMessage)
                PacketNum++;
            else
                PackMessageAndAddToList();
            Current.Add(value);
        }
        public void AddInt(int value)
        {
            if (ReadOnly)
            {
                Debug.LogWarning("Trying to add a value to a readonly LinkedMessage.");
                return;
            }
            const int bits = 32;
            if (Current.WrittenBits + bits < MaxBitsPerMessage)
                PacketNum++;
            else
                PackMessageAndAddToList();
            Current.Add(value);
        }
        public void AddULong(ulong value)
        {
            if (ReadOnly)
            {
                Debug.LogWarning("Trying to add a value to a readonly LinkedMessage.");
                return;
            }
            const int bits = 64;
            if (Current.WrittenBits + bits < MaxBitsPerMessage)
                PacketNum++;
            else
                PackMessageAndAddToList();
            Current.Add(value);
        }
        public void AddLong(long value)
        {
            if (ReadOnly)
            {
                Debug.LogWarning("Trying to add a value to a readonly LinkedMessage.");
                return;
            }
            const int bits = 64;
            if (Current.WrittenBits + bits < MaxBitsPerMessage)
                PacketNum++;
            else
                PackMessageAndAddToList();
            Current.Add(value);
        }
        public void AddFloat(float value)
        {
            if (ReadOnly)
            {
                Debug.LogWarning("Trying to add a value to a readonly LinkedMessage.");
                return;
            }
            const int bits = 32;
            if (Current.WrittenBits + bits < MaxBitsPerMessage)
                PacketNum++;
            else
                PackMessageAndAddToList();
            Current.Add(value);
        }
        public void AddDouble(double value)
        {
            if (ReadOnly)
            {
                Debug.LogWarning("Trying to add a value to a readonly LinkedMessage.");
                return;
            }
            const int bits = 64;
            if (Current.WrittenBits + bits < MaxBitsPerMessage)
                PacketNum++;
            else
                PackMessageAndAddToList();
            Current.Add(value);
        }
        public void AddString(string value)
        {
            if (ReadOnly)
            {
                Debug.LogWarning("Trying to add a value to a readonly LinkedMessage.");
                return;
            }
            byte[] array = System.Text.Encoding.UTF8.GetBytes(value);
            int bits = array.Length * 8 + 64;
            if (bits > MaxBitsPerMessage)
            {
                this.AddInt(0);
                Debug.LogError("Can not add string with bit-length of " + bits + " to a msg of max length " + MaxBitsPerMessage + ". String-Splitting is not nativly supported.");
                return;
            }
            this.AddInt(array.Length);
            for (int i = 0; i < array.Length; i++)
                this.AddByte(array[i]);
        }
        #endregion

        public void Release()
        {
            if (Current != null)
            {
                Debug.LogWarning("Releasing a Packet while current is not null.");
            }
            Messages.Clear();
            Current = null;
            InUseList.Remove(LinkedMsgID);
        }
        public void ReleaseFull()
        {
            if (Current != null)
            {
                Debug.LogWarning("Releasing a Packet while current is not null (in FullRelease).");
                Current.Release();
            }
            if (Messages.Count != 0)
            {
                Debug.LogWarning("Releasing a LinkedMessage with unhandled messages. (" + Messages.Count + ")");
                foreach (var item in Messages)
                    item.Release();
                Messages.Clear();
            }
            InUseList.Remove(LinkedMsgID);
        }

        protected List<Message> GetAllMessages()
        {
            PackMessageAndAddToList(false);
            List<Message> list = new List<Message>();

            Message header = Message.Create(MessageSendMode.Reliable, MsgID);
            header.AddUShort(LinkedMsgID);
            header.AddBool(true); //- Is Header
            header.AddInt(0);   //- Internal Order
            header.AddInt(TotalPacketNum);
            header.AddInt(Messages.Count + 1);
            list.Add(header);
            list.AddRange(Messages);

            return list;
        }
        private static Dictionary<ushort, LinkedPacketInfo> PendingPacketsDic = new();
        public static void HandleIncomingMessage(Message msg, Action<LinkedMessage> action)
        {
            ushort linked_msg_id = msg.GetUShort();
            bool is_header = msg.GetBool();
            int order_index = msg.GetInt();

            if (!PendingPacketsDic.ContainsKey(linked_msg_id))
                PendingPacketsDic.Add(linked_msg_id, new LinkedPacketInfo(linked_msg_id, action));

            LinkedPacketInfo info = PendingPacketsDic[linked_msg_id];
            if (is_header)
            {
                var total_packets = msg.GetInt();
                var msg_count = msg.GetInt();
                if (!info.ReceivedHeader)
                {
                    info.ExpectedMessages = msg_count;
                    info.ExpectedPackets = total_packets;
                    info.AddMessage(msg, order_index);
                }
                else
                    Debug.LogError("Msg-Header for " + linked_msg_id + " already received. This should not happen!");
                if (info.ExpectedMessages == info.ReceivedMessages)
                    ExecuteAndDispose(info);
                return;
            }
            info.AddMessage(msg, order_index);
            if (info.ReceivedHeader && info.ExpectedMessages == info.ReceivedMessages)
                ExecuteAndDispose(info);
        }
        private static void ExecuteAndDispose(LinkedPacketInfo packet)
        {
            PendingPacketsDic.Remove(packet.LinkedMsgID);

            Action<LinkedMessage> action = packet.Action;
            var pairs = packet.GetMessages();
            List<Message> ordered_messages = new();
            while (pairs.Count > 0)
            {
                var item = pairs[0];
                for (int i = 1; i < pairs.Count; i++)
                {
                    if (pairs[i].Key < item.Key)
                    {
                        item = pairs[i];
                    }
                }
                if (item.Key != 0)
                    ordered_messages.Add(item.Value);
                pairs.Remove(item);
            }

            LinkedMessage linked_msg = new LinkedMessage(packet.LinkedMsgID);

            linked_msg.RestExpectedTotalPackets = packet.ExpectedPackets;
            linked_msg.Messages.AddRange(ordered_messages);

            action?.Invoke(linked_msg);
            linked_msg.ReleaseFull();
        }

        public void Send(Client sender)
        {
            var list = GetAllMessages();
            Release();

            foreach (var msg in list)
                sender.Send(msg);
        }
        public void Send(Server sender)
        {
            var list = GetAllMessages();
            Release();

            foreach (var msg in list)
                sender.SendToAll(msg, false);

            foreach (var msg in list)
                msg.Release();
        }
        public void SendToAll(Server sender) => Send(sender);
        public void Send(Server sender, ushort client_id)
        {
            var list = GetAllMessages();
            Release();


            foreach (var msg in list)
                sender.Send(msg, client_id);
        }
        public void Send(Server sender, params ushort[] client_ids)
        {
            var list = GetAllMessages();
            Release();

            for (int i = 0; i < client_ids.Length; i++)
            {
                var client_id = client_ids[i];
                foreach (var msg in list)
                    sender.Send(msg, client_id, false);
            }
            foreach (var msg in list)
                msg.Release();
        }

        private class LinkedPacketInfo
        {
            public Action<LinkedMessage> Action;
            public ushort LinkedMsgID;
            public int ExpectedMessages = -1;
            public int ExpectedPackets = -1;
            public bool ReceivedHeader => ExpectedPackets != -1;
            public int ReceivedMessages => List.Count;
            private readonly List<KeyValuePair<int, Message>> List = new();
            public List<KeyValuePair<int, Message>> GetMessages() => List;

            public void AddMessage(Message msg, int order_index)
            {
                List.Add(new KeyValuePair<int, Message>(order_index, msg));
            }

            public LinkedPacketInfo(ushort id, Action<LinkedMessage> action)
            {
                LinkedMsgID = id;
                Action = action;
            }
        }
    }

}
