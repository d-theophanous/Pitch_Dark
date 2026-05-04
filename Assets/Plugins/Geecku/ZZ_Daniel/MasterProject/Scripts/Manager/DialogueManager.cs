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
        //- should probably not be here
        [SerializeField] private List<NPCScript> FirstPuzzleNPCList;
        [SerializeField] private List<NPCScript> SecondPuzzleNPCList;
        private List<NPCScript> CurrentNPCs = new();
        public float TextSpeed;
        private ReadableDialogue Dialogue;
        private Camera CurNPCCamera;
        private NPCScript CurNPC;
        //- ugly ToDo opt
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

            Dialogue.StartDialogue();
        }
        public void EndDialogue()
        {
            if (ImproviseAfter)
            {
                GameManager.Instance.SetGameState(GameState.IMPROVISING);
                GlobalUIManager.Instance.ToggleUI(UI_Group.IMPROVISATION, false);
                InputManager.Instance.PlayerInput.SwitchCurrentActionMap("Improvisation");
                AudioManager.Instance.StartImprovisation();
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
        public void SetCurrentNPC(NPCScript npc) 
        {
            CurNPCCamera = npc.GetCamera();
            CurNPC = npc;
        }
        //- ToDo opt
        public void AddNPCFollowers()
        {
            if (CurrentNPCs == null)
                Debug.Log("Current NPCs null");
            if (GetNPCFollowers() == null)
                Debug.Log("Current followers null");

            CurrentNPCs.AddRange(GetNPCFollowers());
            foreach (NPCScript npc in GetNPCFollowers())
            {
                npc.ToggleFollowing();
            }
        }
        private List<NPCScript> GetNPCFollowers()
        {
            Debug.Log("puzzle cound: " + PuzzleManager.Instance.PuzzleCount);
            if (PuzzleManager.Instance.PuzzleCount == 0)
            {
                return FirstPuzzleNPCList;
            }
            else if (PuzzleManager.Instance.PuzzleCount == 1)
            {
                return SecondPuzzleNPCList;
            }
            return null;
        }
    }
}
