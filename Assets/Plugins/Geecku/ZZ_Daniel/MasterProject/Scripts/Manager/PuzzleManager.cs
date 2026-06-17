using Geecku.DefaultNetworking;
using Geecku.GlobalMangers;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Components;

namespace Daniel.Master
{
    public class PuzzleManager : PersistantDSingleton<PuzzleManager>
    {
        [SerializeField] private PuzzleUI PuzzleSolveUI;
        [SerializeField] private PuzzleUI PuzzleSolutionUI;
        [SerializeField] private LocalizeStringEvent InputNumberStringEvent;
        [SerializeField] private int DeductionPoints;
        [SerializeField] private int SuccessPoints;
        [SerializeField] private GameObject BlackBackground;

        private List<Puzzle> PuzzleList = new();
        private Puzzle CurPuzzle;
        private bool ThisPlayerSolves;
        private int RemainingDoorCount = 7;

        private List<Interval> SolutionSequence => CurPuzzle.Intervals;
        private int SolutionIdx;

        private DoorScript CurDoor;
        public int PuzzleIdx = 0;

        public bool PuzzleActive;
        public bool IsSolving;
        public bool IsPuzzleSolved;
        private Camera CurCamera;
        public int TotalMistakes;

        protected override void Start()
        {
            SetUpPuzzleList();
        }
        private void SetUpPuzzleList()
        {
            Puzzle puzzle_1 = new Puzzle(true, new() { Interval.OCTAVE, Interval.PRIME }, 2, () => 
            {
                AudioManager.Instance.UnlockInterval(Interval.PRIME);
                AudioManager.Instance.UnlockInterval(Interval.OCTAVE);
            });

            //- 2.Puzzle
            Puzzle puzzle_2 = new Puzzle(false, new() { Interval.PRIME, Interval.OCTAVE, Interval.OCTAVE }, 3);

            //- 3.Puzzle
            Puzzle puzzle_3 = new Puzzle(true, new() { Interval.OCTAVE, Interval.FIFTH, Interval.PRIME }, 3, () =>
            {
                AudioManager.Instance.UnlockInterval(Interval.FIFTH);
                AudioManager.Instance.UnlockInstrument(Instrument.Guitar);
                AudioManager.Instance.UnlockInstrument(Instrument.Piano);
            });

            //- 4.Puzzle
            Puzzle puzzle_4 = new Puzzle(false, new() { Interval.FIFTH, Interval.PRIME, Interval.FIFTH, Interval.OCTAVE }, 4);

            //- 5.Puzzle
            Puzzle puzzle_5 = new Puzzle(true, new() { Interval.PRIME, Interval.OCTAVE, Interval.FIFTH, Interval.PRIME }, 4);

            //- 6.Puzzle
            Puzzle puzzle_6 = new Puzzle(false, new() { Interval.FIFTH, Interval.FIFTH, Interval.OCTAVE, Interval.PRIME }, 4);

            //- 6.Puzzle
            Puzzle puzzle_7 = new Puzzle(true, new() { Interval.FIFTH, Interval.OCTAVE, Interval.FIFTH }, 3, () =>
            {
                AudioManager.Instance.UnlockInstrument(Instrument.Violin);
            });

            PuzzleList.Add(puzzle_1);
            PuzzleList.Add(puzzle_2);
            PuzzleList.Add(puzzle_3);
            PuzzleList.Add(puzzle_4);
            PuzzleList.Add(puzzle_5);
            PuzzleList.Add(puzzle_6);
            PuzzleList.Add(puzzle_7);
        }
        public void CheckPuzzle(Interval interval)
        {
            Debug.Log("solution index: " + SolutionIdx);
            Debug.Log("input: " + interval + ", Lösung: " + SolutionSequence[SolutionIdx]);
            
            InputManager.Instance.PlayerInput.DeactivateInput();
            if (interval == SolutionSequence[SolutionIdx])
            {
                if (NetworkManager.Instance._Client.IsInConnection)
                    GameManager.Instance.ClientSend_AdvanceSequence(true);
                else
                    AdvancePuzzle(true);
            }
            else
            {
                if (NetworkManager.Instance._Client.IsInConnection)
                    GameManager.Instance.ClientSend_AdvanceSequence(false);
                else
                    AdvancePuzzle(false);
            }
        }

        #region Puzzle Logic
        public void StartPuzzle()
        {
            DirectionChecker.Instance.CanReceiveDirectionInfo = false;
            if (CurDoor.TutorialNPC == null)
                SetUpPuzzle();
            else
                CurDoor.TutorialNPC.ActivateSecondDialogue();
        }
        //- for debug public TODo
        public void PuzzleSolved()
        {
            SolutionIdx = 0;
            CurDoor.ToggleDoor(true);
            if (PuzzleIdx == PuzzleList.Count - 1)
            {
                GameManager.Instance.GameFinished = true;
            }
            Action action = () =>
            {
                //- close UI, open door and activate the dialogue
                GlobalUIManager.Instance.ToggleUI(UI_Group.PUZZLE);
                PuzzleIdx++;
                RemainingDoorCount--;
                CurCamera.gameObject.SetActive(false);
                IsSolving = false;
                CurPuzzle.OnEndAction?.Invoke();
                InputManager.Instance.PlayerInput.ActivateInput();

                StartCoroutine(PlayDoorSequence());
            };
            AudioManager.Instance.PlaySFX(SFX.OPEN_DOOR, action);
        }

        //- player who solves
        public void AdvancePuzzle(bool was_correct)
        {
            if (was_correct)
            {
                Action action;
                //- if the final sequence was solved correctly
                if (SolutionIdx + 1 >= CurPuzzle.Intervals.Count)
                {
                    GlobalUIManager.Instance.SetScore(SuccessPoints);
                    action = () => { PuzzleSolved(); };
                }
                //- if a sequence was solved correctly
                else
                {
                    action = () =>
                    {
                        SolutionIdx++;
                        SetNumber(SolutionIdx + 1);
                        int number = (int)PuzzleSolveUI.Label.Number;
                        number++;
                        PuzzleSolveUI.Label.Number = (Number)number;
                        GlobalUIManager.Instance.SetScore(SuccessPoints);

                        PuzzleSolveUI.ElementGroup.SetCurElement(1);
                        InputManager.Instance.PlayerInput.ActivateInput();
                    };
                }
                AudioManager.Instance.PlaySFX(SFX.CORRECT, action);
            }
            else
            {
                AudioManager.Instance.PlaySFX(SFX.WRONG, () =>
                {
                    TotalMistakes++;
                    PuzzleSolveUI.ElementGroup.SetCurElement(1);
                    var cur_element = PuzzleSolveUI.ElementGroup.GetCurElement();
                    cur_element.Activate();
                    InputManager.Instance.PlayerInput.ActivateInput();
                    GlobalUIManager.Instance.SetScore(DeductionPoints);
                });
            }
        }
        //- player who has solution
        public void OnReceiveSolutionInput(bool was_correct)
        {
            if (was_correct)
            {
                Action action = null;
                if (SolutionIdx >= CurPuzzle.Intervals.Count - 1)
                {
                    action = () => { PuzzleSolved(); };
                }
                AudioManager.Instance.PlaySFX(SFX.CORRECT, action);
                SolutionIdx++;
                GlobalUIManager.Instance.SetScore(SuccessPoints);
            }
            else
            {
                AudioManager.Instance.PlaySFX(SFX.WRONG);
                GlobalUIManager.Instance.SetScore(DeductionPoints);
            }
        }
        public void SetUpPuzzle()
        {
            IsSolving = true;
            CurCamera.gameObject.SetActive(true);
            CurPuzzle = PuzzleList[PuzzleIdx];

            if (CurPuzzle.Player1Solves && GameManager.Instance.PlayerNumber == 1 ||
                !CurPuzzle.Player1Solves && GameManager.Instance.PlayerNumber == 2)
                SetUpSolveUI();
            else
                SetUpSolutionUI();
            Debug.Log("set up puzzle");
            GlobalUIManager.Instance.ToggleUI(UI_Group.PUZZLE, false);
        }
        private void SetUpSolveUI()
        {
            ThisPlayerSolves = true;
            GlobalUIManager.Instance.ThisPlayerSolves = true;

            //- Set UI
            SetNumber(1);
            PuzzleSolveUI.Label.Number = Number.FIRST;

            PuzzleSolveUI.ElementList[0].gameObject.SetActive(CurPuzzle.Intervals.Contains(Interval.PRIME));
            PuzzleSolveUI.ElementList[1].gameObject.SetActive(CurPuzzle.Intervals.Contains(Interval.FIFTH));
            PuzzleSolveUI.ElementList[2].gameObject.SetActive(CurPuzzle.Intervals.Contains(Interval.OCTAVE));
            
            //- to make sure its in right order
            foreach (var button in PuzzleSolveUI.ElementList)
            {
                PuzzleSolveUI.ElementGroup.RemoveElement(button);
            }
            //- then add the right ones
            foreach (var button in PuzzleSolveUI.ElementList)
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
            GlobalUIManager.Instance.ThisPlayerSolves = false;

            for (int i = 0; i < PuzzleSolutionUI.ElementList.Count; i++)
            {
                if (CurPuzzle.Intervals.Count - 1 >= i)
                {
                    PuzzleSolutionUI.ElementList[i].gameObject.SetActive(true);
                    SetInput(i + 1, PuzzleSolutionUI.ElementList[i].GetComponent<LocalizeStringEvent>());
                    PuzzleSolutionUI.ElementList[i].Interval = CurPuzzle.Intervals[i];
                }
                else
                    PuzzleSolutionUI.ElementList[i].gameObject.SetActive(false);
            }

            foreach (var button in PuzzleSolutionUI.ElementList)
            {
                if (!button.gameObject.activeSelf && PuzzleSolutionUI.ElementGroup.GetElements().Contains(button))
                {
                    PuzzleSolutionUI.ElementGroup.RemoveElement(button);
                }
                else if (button.gameObject.activeSelf && !PuzzleSolutionUI.ElementGroup.GetElements().Contains(button))
                    PuzzleSolutionUI.ElementGroup.AddElement(button);
            }
        }
        public void SetNumber(int first_number)
        {
            InputNumberStringEvent.StringReference.Arguments = new object[] { first_number };
            InputNumberStringEvent.RefreshString();
        }
        public void SetInput(int number, LocalizeStringEvent string_event)
        {
            string_event.StringReference.Arguments = new object[] { number };
            string_event.RefreshString();
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
        public IEnumerator PlayDoorSequence()
        {
            InputManager.Instance.PlayerInput.DeactivateInput();
            BlackBackground.SetActive(true);
            GameManager.Instance.Player.TeleportCharacter(CurDoor.AfterPuzzlePositionPlayer.position);
            GameManager.Instance.Player.TeleportNPCsToPlayer(CurDoor.AfterPuzzlePositionNPC);

            AudioManager.Instance.PlayScreenInfo(ScreenInfo.CUTSCENE, () =>
            {
                AudioManager.Instance.PlaySFX(SFX.DOOR_SEQUENCE, () =>
                {
                    string key = Helper.GetLanguageString() + "_doors_remaining";
                    AudioManager.Instance.PlayReadableElement(key, UI_Element.DIALOGUE, Interval.NONE, (Number)RemainingDoorCount - 1, () =>
                    {
                        InputManager.Instance.PlayerInput.ActivateInput();
                        BlackBackground.SetActive(false);
                        if (GameManager.Instance.GameFinished)
                        {
                            GameManager.Instance.SetGameState(GameState.IMPROVISING);
                            GlobalUIManager.Instance.ToggleUI(UI_Group.IMPROVISATION, false);
                            InputManager.Instance.SwitchCurrentActionMap("Improvisation");
                            AudioManager.Instance.StartImprovisation();
                            PuzzleManager.Instance.IsSolving = false;
                            CurDoor.DoorNPC.GetCamera().gameObject.SetActive(true);
                        }
                        else if (CurDoor != null && CurDoor.DoorNPC != null)
                            CurDoor.DoorNPC.ActivateSecondDialogue();
                        else
                        {
                            GameManager.Instance.SetGameState(GameState.PLAYING);
                            DirectionChecker.Instance.StartDirectionChecking();
                        }
                        CurDoor.ToggleDoor(false);
                    });
                });
            });
            yield return null;
        }
        public IEnumerator PlayTutorialDoorSequence()
        {
            InputManager.Instance.PlayerInput.DeactivateInput();
            BlackBackground.SetActive(true);
            GameManager.Instance.Player.SpawnPlayerAtStart();
            GameManager.Instance.Player.TeleportNPCsToPlayer(GameManager.Instance.Player.transform);
            

            AudioManager.Instance.PlaySFX(SFX.CLOSE_DOOR, () =>
            {
                InputManager.Instance.PlayerInput.ActivateInput();
                BlackBackground.SetActive(false);
                TutorialManager.Instance.AfterTutorialNPC.SetFollowing(true);
                GameManager.Instance.SetGameState(GameState.PLAYING);
            });
            yield return null;
        }
        #endregion
    }
    public enum Interval
    {
        PRIME, THIRD, FIFTH, OCTAVE, NONE
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
