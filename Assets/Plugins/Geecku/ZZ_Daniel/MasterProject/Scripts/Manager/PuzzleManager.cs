using Geecku.GlobalMangers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Daniel.Master
{
    public class PuzzleManager : PersistantDSingleton<PuzzleManager>
    {
        //- Tür öffnen logic einbauen
        //- collider nach durchgehen einbauen

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
            if (PuzzleSolved)
            {
                EndPuzzle();
            }
        }
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
                PuzzleSolved = true;
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
            GlobalUIManager.Instance.ToggleUI(UI_Group.PUZZLE);
            GameManager.Instance.SetGameState(GameState.PLAYING);
        }
        private void SetUpPuzzle()
        {
            //- assign right group for TTSManager to open
            //TTSManager.Instance.SetPuzzleGroup(CurPuzzle.Group);

            GlobalUIManager.Instance.ToggleUI(UI_Group.PUZZLE, false);

        }
        #endregion
        public void SetCurrentDoor(DoorScript door)
        {
            CurDoor = door;
            CurCamera = door.GetCamera();
        }
    }
    public enum Interval
    {
        PRIME, THIRD, FIFTH, OCTAVE
    }
}
