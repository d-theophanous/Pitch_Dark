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
        //- Camera aufsetzen die in Render texture outputet, die in Puzzle UI ist
        //- Simple puzzle UI aufsetzen
        //- ReadableGroup aufsetzen
        //- lösungsloop aufsetzen
        //- Tür öffnen logic einbauen
        //- collider nach durchgehen einbauen

        [SerializeField] private List<PuzzleData> PuzzleList;

        private PuzzleData CurPuzzle => PuzzleList[PuzzleCount];
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
                Debug.Log("puzzle solved");
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


        public void StartPuzzle()
        {
            CurCamera.gameObject.SetActive(true);

            SetUpPuzzle();
        }
        public void SolvePuzzle()
        {
            IsSolving = true;
        }
        private void SetUpPuzzle()
        {
            //- assign right group for TTSManager to open
            //TTSManager.Instance.SetPuzzleGroup(CurPuzzle.Group);

            GlobalUIManager.Instance.ToggleUI(UI_Group.PUZZLE, false);
        }
        public void SetCameraTransform(Camera camera) { CurCamera = camera; }
    }
    public enum Interval
    {
        PRIME, THIRD, FIFTH, OCTAVE
    }
}
