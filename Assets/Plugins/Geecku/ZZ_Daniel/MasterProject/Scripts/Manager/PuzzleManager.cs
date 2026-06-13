using Geecku.GlobalMangers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Components;

namespace Daniel.Master
{
    public class PuzzleManager : PersistantDSingleton<PuzzleManager>
    {
        [SerializeField] private PuzzleUI PuzzleSolveUI;
        [SerializeField] private PuzzleUI PuzzleSolutionUI;
        [SerializeField] private LocalizeStringEvent InputNumberStringEvent;

        private List<Puzzle> PuzzleList = new();
        private Puzzle CurPuzzle;
        private bool ThisPlayerSolves;

        private List<Interval> SolutionSequence;
        private int SolutionIdx;

        private DoorScript CurDoor;
        public int PuzzleIdx = 0;

        public bool PuzzleActive;
        public bool IsSolving;
        public bool IsPuzzleSolved;
        private Camera CurCamera;

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
            StartCoroutine(CheckPuzzleCoroutine(interval));
        }

        //- ToDo (opt) Beide Puzzle check funktion generalisieren etc.
        private IEnumerator CheckPuzzleCoroutine(Interval interval)
        {
            if (interval == SolutionSequence[SolutionIdx])
            {
                AudioManager.Instance.PlaySFX(SFX.CORRECT, () => 
                {
                    AdvancePuzzle();
                });
            }
            else
            {
                AudioManager.Instance.PlaySFX(SFX.WRONG);
            }
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
        private void PuzzleSolved()
        {
            CurDoor.ToggleDoor(true);
            Action action = () =>
            {
                //- close UI, open door and activate the dialogue
                GlobalUIManager.Instance.ToggleUI(UI_Group.PUZZLE);
                PuzzleIdx++;
                CurCamera.gameObject.SetActive(false);
                CurDoor.DoorNPC.ActivateSecondDialogue();
            };
            AudioManager.Instance.PlaySFX(SFX.OPEN_DOOR, action);
        }
        public void AdvancePuzzle()
        {
            if ()

            if (ThisPlayerSolves)
            {
                
            }
        }
        public void EndPuzzle()
        {
        }
        public void SetUpPuzzle()
        {
            IsSolving = true;
            CurCamera.gameObject.SetActive(true);
            CurPuzzle = PuzzleList[PuzzleIdx];
            Debug.Log("cur puzzle:" + CurPuzzle);
            if (CurPuzzle.Player1Solves && GameManager.Instance.PlayerNumber == 1)
                SetUpSolveUI();
            else
                SetUpSolutionUI();

            GlobalUIManager.Instance.ToggleUI(UI_Group.PUZZLE, false);
        }
        private void SetUpSolveUI()
        {
            ThisPlayerSolves = true;
            GlobalUIManager.Instance.IsSolving = true;

            //- Set UI
            SetNumber(1);
            PuzzleSolveUI.ButtonList[0].gameObject.SetActive(CurPuzzle.Intervals.Contains(Interval.PRIME));
            PuzzleSolveUI.ButtonList[1].gameObject.SetActive(CurPuzzle.Intervals.Contains(Interval.FIFTH));
            PuzzleSolveUI.ButtonList[2].gameObject.SetActive(CurPuzzle.Intervals.Contains(Interval.OCTAVE));
            
            foreach (var button in PuzzleSolveUI.ButtonList)
            {
                if (!button.gameObject.activeSelf && PuzzleSolveUI.ElementGroup.GetElements().Contains(button))
                {
                    PuzzleSolveUI.ElementGroup.RemoveElement(button);
                }
                else if (button.gameObject.activeSelf && !PuzzleSolveUI.ElementGroup.GetElements().Contains(button))
                    PuzzleSolveUI.ElementGroup.AddElement(button);
            }
        }
        private void SetUpSolutionUI()
        {
            ThisPlayerSolves = false;
            GlobalUIManager.Instance.IsSolving = false;

            for (int i = 0; i < PuzzleSolutionUI.ButtonList.Count; i++)
            {
                if (CurPuzzle.Intervals.Count - 1 <= i)
                {
                    PuzzleSolutionUI.ButtonList[i].gameObject.SetActive(true);
                    SetInterval(CurPuzzle.Intervals[i], PuzzleSolutionUI.ButtonList[i].GetComponent<LocalizeStringEvent>());
                }
                else
                    PuzzleSolutionUI.ButtonList[i].gameObject.SetActive(false);
            }

            foreach (var button in PuzzleSolutionUI.ButtonList)
            {
                if (!button.gameObject.activeSelf && PuzzleSolutionUI.ElementGroup.GetElements().Contains(button))
                {
                    PuzzleSolutionUI.ElementGroup.RemoveElement(button);
                }
                else if (button.gameObject.activeSelf && !PuzzleSolutionUI.ElementGroup.GetElements().Contains(button))
                    PuzzleSolutionUI.ElementGroup.AddElement(button);
            }
        }
        public void SetNumber(int number)
        {
            InputNumberStringEvent.StringReference.Arguments = new object[] { number };
            InputNumberStringEvent.RefreshString();
        }
        public void SetInterval(Interval interval, LocalizeStringEvent string_event)
        {
            //- vlt eher großer abstand instead of octave i.e.
            switch (interval)
            {
                case Interval.PRIME:
                    string_event.StringReference.SetReference("Language String Table", "prime");
                    break;
                case Interval.FIFTH:
                    string_event.StringReference.SetReference("Language String Table", "fifth");
                    break;
                case Interval.OCTAVE:
                    string_event.StringReference.SetReference("Language String Table", "octave");
                    break;
                default:
                    break;
            }
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
