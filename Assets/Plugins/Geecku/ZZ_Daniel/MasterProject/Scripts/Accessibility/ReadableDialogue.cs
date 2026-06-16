using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Daniel.Master
{
    public class ReadableDialogue : ReadableElement
    {
        private string[] Lines;
        private float TextSpeed;
        private int index;
        private bool ContinuePressed;
        private DialogueContainer CurDialog;
        private DialogueData CurDialogData;
        private Dictionary<int, Action> ActionDic;
        private Action DefaultAction = () => { DialogueManager.Instance.ContinueWithDialogue = true; };
        private Action CurrentAction;
        private float CurrentPitch;

        public void SetUp(DialogueContainer data, float text_speed, Dictionary<int, Action> action_dic, float pitch)
        {
            CurDialog = data;
            CurDialogData = CurDialog.DialogueList[GetChangedLanguageInt((int)GameManager.Language)];
            Text.text = String.Empty;
            Lines = CurDialogData.Lines.ToArray();
            TextSpeed = text_speed;
            CurrentPitch = pitch;

            if (action_dic == null)
                ActionDic = new();
            else
                ActionDic = action_dic;
        }
        //- hilarious (ToDo) opt
        private int GetChangedLanguageInt(int language)
        {
            if (language == 0)
                return 1;
            else if (language == 1)
                return 0;
            else
                return 2;
        }
        public override void Activate()
        {
            if (DialogueManager.Instance.ContinueWithDialogue)
                ContinuePressed = true;
        }
        public override void Repeat()
        {
            AudioManager.Instance.RepeatLastDialogue();
        }
        protected override void OnSelect() { }
        //- repeat current dialogue line instead of going back
        public override void Return()
        {
            //- ToDo(MP) logic dass wieder von vorne angefangen wird. 
        }
        public void UpdateReadableDialogue()
        {
            if (ContinuePressed && DialogueManager.Instance.ContinueWithDialogue)
            {
                DialogueManager.Instance.ContinueWithDialogue = false;
                ContinuePressed = false;
                NextLine();
            }
        }
        public void StartDialogue()
        {
            Action action = () =>
            {
                index = 0;
                NextLine();
                ContinuePressed = false;
                DialogueManager.Instance.ContinueWithDialogue = false;
            };
            AudioManager.Instance.PlayScreenInfo(ScreenInfo.DIALOGUE, action);
        }
        private IEnumerator TypeLine()
        {
            yield return new WaitForEndOfFrame();
            foreach (char c in Lines[index].ToCharArray())
            {
                Text.text += c;
                yield return new WaitForSeconds(TextSpeed);
            }
            index++;
        }
        private bool NextLine()
        {
            if (index <= Lines.Length - 1)
            {
                if (ActionDic.ContainsKey(index))
                    CurrentAction = ActionDic[index];
                else
                    CurrentAction = DefaultAction;
                    Text.text = string.Empty;
                AudioManager.Instance.PlayDialogue(CurDialogData.DialogueNumber, index,
                    CurrentAction, CurDialog.Tones[index], CurrentPitch);

                StartCoroutine(TypeLine());
                return true;
            }
            else
            {
                DialogueManager.Instance.EndDialogue();
                ContinuePressed = false;
                if (ActionDic.ContainsKey(index))
                {
                    ActionDic[index]?.Invoke();
                }
                return false;
            }
        }
    }
}
