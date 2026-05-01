using Geecku.GlobalMangers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Daniel.Master
{
    public class PuzzleManager : PersistantDSingleton<PuzzleManager>
    {
        [SerializeField] private List<PuzzleData> PuzzleList;

        private PuzzleData CurPuzzle => PuzzleList[PuzzleCount];
        private DoorScript CurDoor;
        public int PuzzleCount = 0;

        public bool PuzzleActive;
        public bool IsSolving;
        public bool PuzzleSolved;
        public Note[] SolutionInterval = new Note[2];
        private Camera CurCamera;

        public void UpdatePuzzle()
        {
            //- maybe I dont need?
            //if (PuzzleSolved)
            //{
            //    EndPuzzle();
            //}
        }
        /// <summary>
        /// Gets called when you press the solve button. Waits for input and 
        /// checks if your answer is right or wrong
        /// </summary>
        /// <param name="interval"></param>
        public void CheckPuzzle(Interval interval)
        {
            StartCoroutine(CheckPuzzleCoroutine(interval));
        }
        private IEnumerator CheckPuzzleCoroutine(Interval interval)
        {
            InputManager.Instance.PlayerInput.actions.FindActionMap("Improvisation").Disable();
            yield return new WaitForSeconds(1f);
            if (interval == CurPuzzle.SolutionInterval)
            {
                //- close UI, open door and activate the dialogue
                GlobalUIManager.Instance.ToggleUI(UI_Group.PUZZLE);
                CurDoor.ToggleDoor(true);
                yield return new WaitForSeconds(1f);
                CurDoor.DoorNPC.ActivateSecondDialogue();
            }
            else
            {
                Debug.Log("Wrong");
                InputManager.Instance.PlayerInput.actions.FindActionMap("Improvisation").Enable();

            }
        }

        #region Puzzle Logic
        public void StartPuzzle()
        {
            CurCamera.gameObject.SetActive(true);

            SetUpPuzzle();
        }
        public void SolvePuzzle()
        {
            IsSolving = true;
            InputManager.Instance.PlayerInput.actions.FindActionMap("Improvisation").Enable();
        }
        public void EndPuzzle()
        {
        }
        private void SetUpPuzzle()
        {
            GlobalUIManager.Instance.ToggleUI(UI_Group.PUZZLE, false);
        }
        #endregion
        public void SetCurrentDoor(DoorScript door)
        {
            CurDoor = door;
            CurCamera = door.GetCamera();
        }
        public void CloseDoor()
        {
            if (CurDoor != null)
                CurDoor.ToggleDoor(false);
        }
    }
    public enum Interval
    {
        PRIME, THIRD, FIFTH, OCTAVE
    }
}
