using System;
using System.Collections;
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

        public void SetUp(DialogueContainer data, float text_speed)
        {
            Debug.Log("Setup");
            CurDialog = data;
            CurDialogData = CurDialog.DialogueList[(int)GameManager.Language];
            Text.text = String.Empty;
            Lines = CurDialogData.Lines.ToArray();
            TextSpeed = text_speed;
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
            if (ContinuePressed)
            {
                if (Text.text == Lines[index])
                {
                    //- optional ToDo: nicer System
                    AudioManager.Instance.PlayDialogue(CurDialogData.DialogueNumber, index);
                    NextLine();
                }
                else
                {
                    StopAllCoroutines();
                    //Text.text = Lines[index];

                    AudioManager.Instance.PlayDialogue(CurDialogData.DialogueNumber, index);
                    NextLine();
                }
                ContinuePressed = false;
            }
        }
        public void StartDialogue()
        {
            index = 0;
            AudioManager.Instance.PlayDialogue(CurDialogData.DialogueNumber, index);
            StartCoroutine(TypeLine());
        }
        private IEnumerator TypeLine()
        {
            yield return new WaitForEndOfFrame();
            Debug.Log("index: " + index);
            Debug.Log("lines: " + Lines.Length);
            foreach (char c in Lines[index].ToCharArray())
            {
                Text.text += c;
                yield return new WaitForSeconds(TextSpeed);
            }
        }
        private void NextLine()
        {
            if (index < Lines.Length - 1)
            {
                index++;
                Text.text = string.Empty;
                StartCoroutine(TypeLine());
            }
            else
            {
                DialogueManager.Instance.EndDialogue();
            }
        }
    }
}
