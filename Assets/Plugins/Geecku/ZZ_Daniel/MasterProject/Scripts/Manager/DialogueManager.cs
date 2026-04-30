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
        [SerializeField] private CinemachineCamera MainCamera;
        [SerializeField] private CinemachineCamera DialogueCamera;
        public float TextSpeed;
        private ReadableDialogue Dialogue;
        //- ugly
        private bool ImproviseAfter;

        protected override void Awake()
        {
            base.Awake();
            Dialogue = GlobalUIManager.Instance.GetReadableDialogue();
            GlobalUIManager.Instance.SecondPlayerCam = SecondPlayerCam;
        }
        public void StartDialogue(DialogueContainer data, bool improvise = false)
        {
            ImproviseAfter = improvise;
            Dialogue.SetUp(data, TextSpeed);
            GlobalUIManager.Instance.ToggleUI(UI_Group.DIALOGUE);
            GameManager.Instance.SetGameState(GameState.DIALOGUE);
            DialogueCamera.Priority = 1;
            MainCamera.Priority = 0;
        }
        public void EndDialogue()
        {
            if (ImproviseAfter)
            {
                InputManager.Instance.PlayerInput.SwitchCurrentActionMap("Improvisation");
                AudioManager.Instance.StartImprovisation();
                GameManager.Instance.SetGameState(GameState.IMPROVISING);
            }

            DialogueCamera.Priority = 0;
            MainCamera.Priority = 1;
            GlobalUIManager.Instance.ToggleUI(UI_Group.DIALOGUE);
            InputManager.Instance.PlayerInput.SwitchCurrentActionMap("Player");
            GameManager.Instance.SetGameState(GameState.PLAYING);
        }

        public void UpdateDialogue()
        {
            Dialogue.UpdateReadableDialogue();
        }
    }
}
