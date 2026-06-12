using Geecku.GlobalMangers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Daniel.Master
{
    public class PuzzleManager : PersistantDSingleton<PuzzleManager>
    {
        [SerializeField] private PuzzleUI PuzzleSolveUI;
        [SerializeField] private PuzzleUI PuzzleSolutionUI;
        private List<Puzzle> PuzzleList;

        private List<Interval> SolutionSequence;
        private int SolutionIdx;

        private DoorScript CurDoor;
        public int PuzzleCount = 0;

        public bool PuzzleActive;
        public bool IsSolving;
        public bool IsPuzzleSolved;
        private Camera CurCamera;

        public List<NPCScript> TutorialNPCs = new();

        protected override void Start()
        {
            SetUpPuzzleList();
        }
        private void SetUpPuzzleList()
        {
            //- 1.Puzzle
            Puzzle puzzle_1 = new Puzzle(true, new() { Interval.PRIME, Interval.OCTAVE }, 2);

            //- 2.Puzzle
            Puzzle puzzle_2 = new Puzzle(false, new() { Interval.PRIME, Interval.OCTAVE }, 3);

            //- 3.Puzzle
            Puzzle puzzle_3 = new Puzzle(true, new() { Interval.PRIME, Interval.OCTAVE, Interval.FIFTH }, 3);
            
            //- 4.Puzzle
            Puzzle puzzle_4 = new Puzzle(false, new() { Interval.PRIME, Interval.OCTAVE, Interval.FIFTH }, 4);

            //- 5.Puzzle
            Puzzle puzzle_5 = new Puzzle(true, new() { Interval.PRIME, Interval.OCTAVE, Interval.FIFTH }, 4);

            //- 6.Puzzle
            Puzzle puzzle_6 = new Puzzle(false, new() { Interval.PRIME, Interval.OCTAVE, Interval.FIFTH }, 4);

            PuzzleList.Add(puzzle_1);
            PuzzleList.Add(puzzle_2);
            PuzzleList.Add(puzzle_3);
            PuzzleList.Add(puzzle_4);
            PuzzleList.Add(puzzle_5);
            PuzzleList.Add(puzzle_6);
        }

        public void CheckPuzzle(Interval interval)
        {
            //- ToDo
        }

        //- ToDo (opt) Beide Puzzle check funktion generalisieren etc.
        private IEnumerator CheckPuzzleCoroutine(Interval interval)
        {
            //InputManager.Instance.PlayerInput.actions.FindActionMap("Improvisation").Disable();
            //yield return new WaitForSeconds(1f);
            //if (interval == CurPuzzle.SolutionInterval)
            //{
            //    AudioManager.Instance.PlaySFX(SFX.CORRECT, () => { PuzzleSolved(); });
            //}
            //else
            //{
            //    AudioManager.Instance.PlaySFX(SFX.WRONG);
            //    InputManager.Instance.PlayerInput.actions.FindActionMap("Improvisation").Enable();

            //}
            yield return null;
        }


        #region Puzzle Logic
        public void StartPuzzle()
        {
            if (CurDoor.TutorialNPC == null)
                SetUpPuzzle();
            else
                CurDoor.TutorialNPC.ActivateSecondDialogue();
        }
        //- for first puzzle ToDo
        public void StartSolvingPuzzle()
        {
            IsSolving = true;            
        }
        private void PuzzleSolved()
        {
            CurDoor.ToggleDoor(true);
            Action action = () =>
            {
                //- close UI, open door and activate the dialogue
                GlobalUIManager.Instance.ToggleUI(UI_Group.PUZZLE);
                PuzzleCount++;
                CurCamera.gameObject.SetActive(false);
                CurDoor.DoorNPC.ActivateSecondDialogue();
            };
            AudioManager.Instance.PlaySFX(SFX.OPEN_DOOR, action);
        }
        public void EndPuzzle()
        {
        }
        public void SetUpPuzzle()
        {
            CurCamera.gameObject.SetActive(true);
            GlobalUIManager.Instance.ToggleUI(UI_Group.PUZZLE, false);


        }
        private void SetUpSolveUI()
        {

        }
        private void SetUpSolutionUI()
        {

        }
        #endregion

        #region Door Stuff
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
        #endregion
    }
    public enum Interval
    {
        PRIME, THIRD, FIFTH, OCTAVE
    }
    public class Puzzle
    {
        public bool Player1Solves;
        public List<Interval> Intervals;
        public int PasswordLength;
        public Action OnEndAction;

        public Puzzle(bool player1_solves, List<Interval> interval_list, int password_length, Action on_end_action = null)
        {
            Player1Solves = player1_solves;
            Intervals = interval_list; 
            PasswordLength = password_length;
            OnEndAction = on_end_action;
        }
    }    
    /*
     * So was wie
     * puzzle 
     * wie viele lösungseingaben
     * welche intervalle
     * action für wenn fertig?
     * lösungseingaben sollen random sein aber ähnlich häufig die intervalle vorkommen
     * lassen
     * 
     * puzzle1: 2 lösungen, prime und oktave
     * 
     * PuzzleContainer
     * drei buttons
     * element group
     * label
     *  
     */
}
