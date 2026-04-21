using Geecku.GlobalMangers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Daniel.Master
{
    public class DialogueManager : Singleton<DialogueManager>
    {
        [SerializeField] private GameObject SecondPlayerCam;
        public float TextSpeed;
        private ReadableDialogue Dialogue;

        protected override void Awake()
        {
            base.Awake();
            Dialogue = GlobalUIManager.Instance.GetReadableDialogue();
            GlobalUIManager.Instance.SecondPlayerCam = SecondPlayerCam;
        }
        public void StartDialogue(DialogueContainer data)
        {
            Dialogue.SetUp(data, TextSpeed);
            GlobalUIManager.Instance.ToggleUI(UI_Group.DIALOGUE);
            GameManager.Instance.SetGameState(GameState.DIALOGUE);
        }
        public void UpdateDialogue()
        {
            Dialogue.UpdateReadableDialogue();
        }
    }
}
