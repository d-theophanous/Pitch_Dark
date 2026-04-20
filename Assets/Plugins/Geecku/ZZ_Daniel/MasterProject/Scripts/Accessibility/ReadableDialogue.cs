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

        public void SetUp(DialogueData data, float text_speed)
        {
            Text.text = String.Empty;
            Lines = data.lines.ToArray();
            TextSpeed = text_speed;
        }
        public override void Activate()
        {
            ContinuePressed = true;
        }
        //- repeat current dialogue line instead of going back
        public override void Return()
        {
            //- ToDo logic dass wieder von vorne angefangen wird. 
        }
        public void UpdateReadableDialogue()
        {
            if (ContinuePressed)
            {
                if (Text.text == Lines[index])
                {
                    NextLine();
                }
                else
                {
                    StopAllCoroutines();
                    Text.text = Lines[index];
                }
                ContinuePressed = false;
            }
        }
        protected override void OnSelect()
        {
            StartDialogue();
        }
        public void StartDialogue()
        {
            index = 0;
            StartCoroutine(TypeLine());
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
                GlobalUIManager.Instance.ToggleUI(UI_Group.DIALOGUE);
                InputManager.Instance.PlayerInput.SwitchCurrentActionMap("Player");
                GameManager.Instance.SetGameState(GameState.PLAYING);
            }
        }
    }
}
