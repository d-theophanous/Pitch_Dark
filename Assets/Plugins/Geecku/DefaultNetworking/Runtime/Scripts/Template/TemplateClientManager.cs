using Geecku.DefaultNetworking;
using Geecku.DefaultNetworking.Common;
using Riptide;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using static TemplateServerManager;

public class TemplateClientManager : ClientManager
{

    [MessageHandler(600)]
    public static void Receive_PrintTest(Message msg)
    {
        PrintData data = new();
        data.ApplyMessage(msg);
        Action test_action = () =>
        {
            Debug.Log(data.Print);
        };
        MessageHandler.ReceiveDelegateDataMessage(test_action, data.MessageID);
    }
    //[MessageHandler(602)]
    //public static void Receive_Test3(Message msg)
    //{
    //    var msg_id = msg.GetUShort();
    //    var txt = msg.GetString();
    //    var i = msg.GetInt();
    //    Action action = () =>
    //    {
    //        Debug.Log(txt + i);
    //    };
    //    MessageHandler.ReceiveDelegateDataMessage(action, msg_id);
    //}
    [MessageHandler(601)]
    public static void Receive_Test2(Message msg)
    {
        AddDamage data = new();
        data.ApplyMessage(msg);
        Action test_action_damage = () =>
        {
            Debug.Log("Health: " + (data.Health - data.Damage));
        };
        MessageHandler.ReceiveDelegateDataMessage(test_action_damage, data.MessageID);
    }
}
