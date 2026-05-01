using Geecku.GlobalMangers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Daniel.Master
{
    public class DialogueManager : Geecku.GlobalMangers.Singleton<DialogueManager>
    {
        [SerializeField] private GameObject SecondPlayerCam;
        public float TextSpeed;
        private ReadableDialogue Dialogue;
        private Camera CurNPCCamera;
        //- ugly
        private bool ImproviseAfter;

        protected override void Awake()
        {
            base.Awake();
            Dialogue = GlobalUIManager.Instance.GetReadableDialogue();
        }
        public void StartDialogue(DialogueContainer data, bool improvise = false)
        {
            CurNPCCamera.gameObject.SetActive(true);
            ImproviseAfter = improvise;
            Dialogue.SetUp(data, TextSpeed);
            GlobalUIManager.Instance.ToggleUI(UI_Group.DIALOGUE);
            GameManager.Instance.SetGameState(GameState.DIALOGUE);
        }
        public void EndDialogue()
        {
            if (ImproviseAfter)
            {
                InputManager.Instance.PlayerInput.SwitchCurrentActionMap("Improvisation");
                AudioManager.Instance.StartImprovisation();
                GameManager.Instance.SetGameState(GameState.IMPROVISING);
                GlobalUIManager.Instance.ToggleUI(UI_Group.IMPROVISATION, false);
                PuzzleManager.Instance.IsSolving = false;
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
        public void SetCurrentNPCCamera(Camera camera) { CurNPCCamera = camera; }
    }
}
