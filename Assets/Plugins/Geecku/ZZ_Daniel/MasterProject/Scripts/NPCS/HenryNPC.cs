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
            TutorialButtonInfo seg_3_button_info = new TutorialButtonInfo("200_5");
            seg_3.RequiredButtonDic.Add(TutorialButton.Square,
                seg_3_button_info);
            seg_3.StartAction = () => {
                AudioManager.Instance.PlayDialogue(200, 4, () => { seg_3_button_info.SpeechOver = true; }); 
            };
            seg_3.EndAction = () =>
            {
                AudioManager.Instance.PlayDialogue(200, 9, () =>
                {
                    TutorialManager.Instance.EndSegment();
                    DialogueManager.Instance.ContinueWithDialogue = true;
                    DialogueManager.Instance.Dialogue.Activate();
                }, Message_Tone.WAIT);
            };

            //- Navigate through text
            ButtonTutorialSegment seg_2 = new();
            TutorialButtonInfo seg_2_button_info_1 = new TutorialButtonInfo("200_3");
            TutorialButtonInfo seg_2_button_info_2 = new TutorialButtonInfo("");
            seg_2.RequiredButtonDic.Add(TutorialButton.Up_Arrow,
                seg_2_button_info_1);
            seg_2.RequiredButtonDic.Add(TutorialButton.Down_Arrow,
                seg_2_button_info_2);
            seg_2.StartAction = () => {
                AudioManager.Instance.PlayDialogue(200, 2, () => { seg_2_button_info_1.SpeechOver = true; }); 
            };
            seg_2.EndAction = () =>
            {
                TutorialManager.Instance.SwitchSegment(seg_3);
            };

            //- Advance Dialogue
            ButtonTutorialSegment seg_1 = new();
            TutorialButtonInfo seg_1_button_info = new TutorialButtonInfo("200_1");
            seg_1.RequiredButtonDic.Add(TutorialButton.X,
                seg_1_button_info);
            seg_1.StartAction = () => {
                AudioManager.Instance.PlayDialogue(200, 0, () => { seg_1_button_info.SpeechOver = true; }); 
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
            TutorialButtonInfo seg_4_button_info_1 = new TutorialButtonInfo("200_8", 2f);
            TutorialButtonInfo seg_4_button_info_2 = new TutorialButtonInfo("");
            seg_4.RequiredButtonDic.Add(TutorialButton.Left_Joystick,
                seg_4_button_info_1);
            seg_4.RequiredButtonDic.Add(TutorialButton.Right_Joystick,
                seg_4_button_info_2);
            seg_4.StartAction = () => {
                AudioManager.Instance.PlayDialogue(200, 6, () =>
                {
                    AudioManager.Instance.PlayDialogue(200, 7, () => { seg_2_button_info_1.SpeechOver = true; });
                }, Message_Tone.NONE); 
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
