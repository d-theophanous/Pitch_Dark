using Geecku.DefaultNetworking;
using Geecku.DefaultNetworking.Common;
using Riptide;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

public class TemplateServerManager : ServerManager
{
    [Button]
    public void PrintTest()
    {
        PrintData print_data = new PrintData();
        //- Print thing
        print_data.MessageHandlerID = 600;
        print_data.Print = "Hallo ich funktioniere";
        MessageHandler.SendDelegateDataMessage(print_data);
    }
    //[Button]
    //public void Test3()
    //{
    //    Message msg = Message.Create(MessageSendMode.Reliable, 602);
    //    msg.AddString("Hallo");
    //    msg.AddInt(5);
    //}
    [Button]
    public void Test2()
    {
        AddDamage damage_data = new AddDamage();
        damage_data.MessageHandlerID = 601;
        damage_data.Damage = 50;
        damage_data.Health = 100;
        MessageHandler.SendDelegateDataMessage(damage_data);
    }
    public class PrintData : DataContainer
    {
        public string Print;
        protected override void RetrieveData(Message msg)
        {
            Print = msg.GetString();
        }
        protected override void AddDataToMsg(Message msg)
        {
            msg.AddString(Print);
        }
        public override void Invoke()
        {
            Delegate?.DynamicInvoke(Print);
        }
    }
    public class AddDamage : DataContainer
    {
        public ushort Health;
        public ushort Damage;
        protected override void RetrieveData(Message msg)
        {
            Health = msg.GetUShort();
            Damage = msg.GetUShort();
        }

        public override void Invoke()
        {
            Delegate?.DynamicInvoke(Health, Damage);
        }

        protected override void AddDataToMsg(Message msg)
        {
            msg.AddUShort(Health);
            msg.AddUShort(Damage);
        }
    }
}
