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

        public void SetUp(DialogueContainer data, float text_speed, Dictionary<int, Action> action_dic)
        {
            CurDialog = data;
            CurDialogData = CurDialog.DialogueList[GetChangedLanguageInt((int)GameManager.Language)];
            Text.text = String.Empty;
            Lines = CurDialogData.Lines.ToArray();
            TextSpeed = text_speed;

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
            ContinuePressed = true;
        }
        protected override void OnSelect() { }
        //- repeat current dialogue line instead of going back
        public override void Return()
        {
            //- ToDo(MP) logic dass wieder von vorne angefangen wird. 
        }
        public void UpdateReadableDialogue()
        {
            if (ContinuePressed || DialogueManager.Instance.ContinueWithDialogue)
            {
                DialogueManager.Instance.ContinueWithDialogue = false;
                if (Text.text == Lines[index])
                {
                    NextLine();
                    //- this is so ugly, change
                    if (GameManager.Instance.PlayerIdx == 1 && (index == 18 || index == 19 || index == 20))
                        AudioManager.Instance.ChangeLines = true;
                    else
                        AudioManager.Instance.ChangeLines = false;
                        //- optional ToDo: nicer System
                        AudioManager.Instance.PlayDialogue(CurDialogData.DialogueNumber, index,
                            CurDialog.Tones[index]);
                }
                else
                {
                    StopAllCoroutines();
                    //Text.text = Lines[index];

                    if (NextLine())
                    AudioManager.Instance.PlayDialogue(CurDialogData.DialogueNumber, index,
                        CurDialog.Tones[index]);
                }
                ContinuePressed = false;
            }
        }
        public void StartDialogue()
        {
            index = -1;
            AudioManager.Instance.PlayDialogue(CurDialogData.DialogueNumber, index + 1,
                        CurDialog.Tones[index + 1]);
            NextLine();
        }
        private IEnumerator TypeLine()
        {
            yield return new WaitForEndOfFrame();
            foreach (char c in Lines[index].ToCharArray())
            {
                Text.text += c;
                yield return new WaitForSeconds(TextSpeed);
            }
        }
        private bool NextLine()
        {
            if (index < Lines.Length - 1)
            {
                index++;
                if (ActionDic.ContainsKey(index))
                    ActionDic[index]?.Invoke();
                Text.text = string.Empty;
                StartCoroutine(TypeLine());
                return true;
            }
            else
            {
                DialogueManager.Instance.EndDialogue();
                return false;
            }
        }
    }
}
