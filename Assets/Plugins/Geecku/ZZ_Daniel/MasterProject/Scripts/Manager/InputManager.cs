using Daniel.Master;
using Geecku.GlobalMangers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Daniel.Master
{
    public class InputManager : PersistantDSingleton<InputManager>
    {
        public PlayerInput PlayerInput;

        protected override void Awake()
        {
            base.Awake();
            PlayerInput = GetComponent<PlayerInput>();
        }
        protected override void Start()
        {
            base.Start();
            foreach (var action_map in PlayerInput.actions.actionMaps)
            {
                action_map.Disable();
            }
            PlayerInput.actions.FindActionMap("UI").Enable();                  
        }

        #region Input Triggers

        #region Music and Improvisation
        public void OnImprovTest(InputValue value)
        {
            if (PlayerInput.currentActionMap.name == "Improvisation")
            {
                PlayerInput.SwitchCurrentActionMap("Player");
                AudioManager.Instance.StopImprovisation();
            }
            else
            {
                PlayerInput.SwitchCurrentActionMap("Improvisation");
                AudioManager.Instance.StartImprovisation();
            }
        }
        public void OnPlayRoot()
        {
            AudioManager.Instance.PlayNote(Note.LOW_C);
            if (PuzzleManager.Instance.IsSolving)
                PuzzleManager.Instance.CheckPuzzle(Interval.PRIME);
        }
        public void OnPlayThird()
        {
            AudioManager.Instance.PlayNote(Note.LOW_E);
            if (PuzzleManager.Instance.IsSolving)
                PuzzleManager.Instance.CheckPuzzle(Interval.THIRD);
        }
        public void OnPlayFifth()
        {
            AudioManager.Instance.PlayNote(Note.LOW_G);
            if (PuzzleManager.Instance.IsSolving)
                PuzzleManager.Instance.CheckPuzzle(Interval.FIFTH);
        }
        public void OnPlayOctave()
        {
            AudioManager.Instance.PlayNote(Note.HIGH_C);
            if (PuzzleManager.Instance.IsSolving)
                PuzzleManager.Instance.CheckPuzzle(Interval.OCTAVE);
        }
        public void OnSwitchInstrument(InputValue value)
        {
            Debug.Log("input:" + (int)value.Get<Vector2>().x);
            AudioManager.Instance.SwitchInstrument((int)value.Get<Vector2>().x);
        }
        #endregion

        #region Settings and UI

        public void OnClick()
        {
            TTSManager.Instance.ActivateCurElement();
        }
        public void OnBack()
        {
            TTSManager.Instance.ReturnCurElement();
        }
        public void OnRepeat()
        {
            TTSManager.Instance.RepeatCurElement();
        }
        public void OnNavigate(InputValue value)
        {
            Vector2 input = value.Get<Vector2>();
            if (input == Vector2.zero) return;
            if (Mathf.Abs(input.x) >= Mathf.Abs(input.y))
            {
                if (input.x > 0)
                    TTSManager.Instance.ActivateCurElement();
                else
                    TTSManager.Instance.ReturnCurElement();
            }
            else
            {
                if (input.y <= 0)
                    TTSManager.Instance.SwitchToNextElement();
                else
                    TTSManager.Instance.SwitchToPreviousElement();
            }
        }


        //- default = TAB
        public void OnSettings(InputValue value)
        {
            GlobalUIManager.Instance.ToggleUI(UI_Group.SETTINGS_GENERAL);
        }
        //- Magnifier
        public void OnMagnify(InputValue value)
        {
            AccessibilityManager.Instance.ToggleMagnifier();
        }
        public Vector2 MoveMagnifyDelta { get; private set; }
        public void OnMoveMagnify(InputValue value)
        {
            MoveMagnifyDelta = value.Get<Vector2>();
        }

        #endregion

        #region Player

        public void OnInteract(InputValue value)
        {
            GameManager.Instance.Player.Interact();
        }
        public void OnMove(InputValue value)
        {
            GameManager.Instance.Player.Move(value.Get<Vector2>());
            Debug.Log("in on move");
        }
        public void OnLook(InputValue value)
        {
            GameManager.Instance.Player.Look(value.Get<Vector2>());
        }
        #endregion

        #region Tutorial
        public void OnSouth(InputValue value)
        {
            TutorialManager.Instance.ProcessButtonPress(TutorialButton.X, true);
        }
        public void OnWest(InputValue value)
        {
            TutorialManager.Instance.ProcessButtonPress(TutorialButton.Square, true);
        }
        public void OnRightStick(InputValue value)
        {
            TutorialManager.Instance.ProcessButtonPress(TutorialButton.Right_Joystick, true);
        }
        public void OnLeftStick(InputValue value)
        {         
            if (value.Get<Vector2>().magnitude > 0f)
            {
                TutorialManager.Instance.ProcessButtonPress(TutorialButton.Left_Joystick, false, true);                
            }
            else
                TutorialManager.Instance.ProcessButtonPress(TutorialButton.Left_Joystick, false, false);

        }
        public void OnUp(InputValue value)
        {
            TutorialManager.Instance.ProcessButtonPress(TutorialButton.Up_Arrow, true);
        }
        public void OnDown(InputValue value)
        {
            TutorialManager.Instance.ProcessButtonPress(TutorialButton.Down_Arrow, true);
        }

        #endregion

        public void OnDebug()
        {
            GameManager.Instance.DebugCurInteractable();
        }

        #endregion
    }
}
