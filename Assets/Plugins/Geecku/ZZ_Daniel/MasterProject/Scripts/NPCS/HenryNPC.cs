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
            SetUpTutorialSegments();
        }
        public override void ActivatePrompt()
        {
            GameManager.Instance.Player.ResetMovement();
            DialogueManager.Instance.SetCurrentNPC(this);
            LookAtPlayer();
            //- nur zum Testen?
            DialogueManager.Instance.StartNPCDialogue(
                DialogueListFinal[0], ActionDicList[0]);
            tag = "Untagged";
        }
        private void SetUpTutorialSegments()
        {
            //- Repeat last line
            ButtonTutorialSegment seg_3 = new();
            seg_3.RequiredButtonDic.Add(TutorialButton.Square,
                new TutorialButtonInfo(""));
            seg_3.StartAction = () => {
                AudioManager.Instance.PlayDialogue("de_0_0", null); //- ToDo
            };
            seg_3.EndAction = () => //- ToDO jump to next segment
            {
                //- vlt noch so was wie "now back to the dialgoue" spielen
                TutorialManager.Instance.EndSegment();
                DialogueManager.Instance.ContinueWithDialogue = true;
            };

            //- Navigate through text
            ButtonTutorialSegment seg_2 = new();
            seg_2.RequiredButtonDic.Add(TutorialButton.Up_Arrow,
                new TutorialButtonInfo(""));
            seg_2.RequiredButtonDic.Add(TutorialButton.Down_Arrow,
                new TutorialButtonInfo(""));
            seg_2.StartAction = () => {
                AudioManager.Instance.PlayDialogue("de_0_0", null); //- ToDo
            };
            seg_2.EndAction = () =>
            {
                TutorialManager.Instance.SwitchSegment(seg_3);
            };

            //- Advance Dialogue
            ButtonTutorialSegment seg_1 = new();
            seg_1.RequiredButtonDic.Add(TutorialButton.X,
                new TutorialButtonInfo(""));
            seg_1.StartAction = () => {
                AudioManager.Instance.PlayDialogue("de_0_0", null); //- ToDo
            };
            seg_1.EndAction = () =>
            {
                TutorialManager.Instance.SwitchSegment(seg_2);
            };
            Dictionary<int, Action> dialogue_1_dic = new();
            dialogue_1_dic.Add(0, () =>
            {
                TutorialManager.Instance.StartSegment(seg_1);
            });

            //- Movement stuff
            ButtonTutorialSegment seg_4 = new();
            seg_4.RequiredButtonDic.Add(TutorialButton.Left_Joystick,
                new TutorialButtonInfo("", 2f));
            seg_4.RequiredButtonDic.Add(TutorialButton.Right_Joystick,
                new TutorialButtonInfo(""));
            seg_4.StartAction = () => {
                AudioManager.Instance.PlayDialogue("de_0_0", null); //- ToDo
                InputManager.Instance.PlayerInput.actions.FindActionMap("Player").Enable();
            };
            seg_4.EndAction = () =>
            {
                TutorialManager.Instance.ToggleStatus(true);
                InputManager.Instance.PlayerInput.actions.FindActionMap("Player").Disable();
                GameManager.Instance.Player.ResetMovement();
            };
            dialogue_1_dic.Add(DialogueListFinal[0].DialogueList[0].Lines.Count - 1, () =>
            {
                DialogueManager.Instance.EndDialogue();
                TutorialManager.Instance.StartSegment(seg_4);
            });


            ActionDicList.Add(dialogue_1_dic);
        }
    }
}
