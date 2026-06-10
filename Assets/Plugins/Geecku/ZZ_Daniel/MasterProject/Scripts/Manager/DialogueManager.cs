using System;
using System.Collections.Generic;
using UnityEngine;

namespace Daniel.Master
{
    public class DialogueManager : Geecku.GlobalMangers.Singleton<DialogueManager>
    {
        [SerializeField] private GameObject SecondPlayerCam;
        public float TextSpeed;
        public ReadableDialogue Dialogue;
        private Camera CurNPCCamera;
        private NPCScript CurNPC;
        //- ugly ToDo opt
        private bool ImproviseAfter;

        public bool ContinueWithDialogue;
        public bool TutorialAfter;

        protected override void Awake()
        {
            base.Awake();
            Dialogue = GlobalUIManager.Instance.GetReadableDialogue();
        }
        public void StartNPCDialogue(DialogueContainer data, Dictionary<int, Action> action_dic, bool improvise = false)
        {
            CurNPCCamera.gameObject.SetActive(true);
            ImproviseAfter = improvise;
            SetUpDialogue(data, action_dic);
        }
        public void StartTutorialDialogue(DialogueContainer data)
        {
            GlobalUIManager.Instance.PlayerViewCamera.gameObject.SetActive(true);
            TutorialAfter = true;
            SetUpDialogue(data, null);
        }
        private void SetUpDialogue(DialogueContainer data, Dictionary<int, Action> action_dic)
        {
            Dialogue.SetUp(data, TextSpeed, action_dic);
            GlobalUIManager.Instance.ToggleUI(UI_Group.DIALOGUE);
            GameManager.Instance.SetGameState(GameState.DIALOGUE);

            Dialogue.StartDialogue();
        }
        public void EndDialogue()
        {
            Debug.Log("in end dialogue");
            if (ImproviseAfter)
            {
                GameManager.Instance.SetGameState(GameState.IMPROVISING);
                GlobalUIManager.Instance.ToggleUI(UI_Group.IMPROVISATION, false);
                InputManager.Instance.PlayerInput.SwitchCurrentActionMap("Improvisation");
                AudioManager.Instance.StartImprovisation();
                PuzzleManager.Instance.IsSolving = false;
            }
            else if (TutorialAfter)
            {
                TutorialManager.Instance.ToggleStatus(false);
                GlobalUIManager.Instance.PlayerViewCamera.gameObject.SetActive(false);
            }
            else
            {
                GlobalUIManager.Instance.ToggleUI(UI_Group.DIALOGUE);
                CleanUpDialogue();
            }
        }
        public void CleanUpDialogue()
        {
            CurNPCCamera.gameObject.SetActive(false);
            InputManager.Instance.PlayerInput.SwitchCurrentActionMap("Player");
            GameManager.Instance.SetGameState(GameState.PLAYING);
        }
        public void UpdateDialogue()
        {
            Dialogue.UpdateReadableDialogue();
        }
        public void SetCurrentNPC(NPCScript npc) 
        {
            CurNPCCamera = npc.GetCamera();
            CurNPC = npc;
        }
    }
}
