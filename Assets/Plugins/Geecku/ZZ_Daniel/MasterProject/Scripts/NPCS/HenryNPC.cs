using System;
using System.Collections.Generic;
using UnityEngine;

namespace Daniel.Master
{
    public class HenryNPC : NPCScript
    {
        protected override void Start()
        {
            base.Start();
            ButtonTutorialSegment seg_1 = new();
            seg_1.RequiredButtonDic.Add(TutorialButton.X,
                new TutorialButtonInfo(""));
            seg_1.EndAction = () =>
            {
                TutorialManager.Instance.EndSegment();
            };
            Dictionary<int, Action> action_dic = new();
            action_dic.Add(0, () =>
            {
                TutorialManager.Instance.StartSegment(seg_1);
            });
            ActionDicList.Add(action_dic);
        }
        public override void ActivatePrompt()
        {
            GameManager.Instance.Player.ResetMovement();
            DialogueManager.Instance.SetCurrentNPC(this);
            LookAtPlayer();
            Debug.Log("NPC interaction");
            //- nur zum Testen?
            DialogueManager.Instance.StartNPCDialogue(
                DialogueListFinal[0], ActionDicList[0]);
            tag = "Untagged";
        }
    }
}
