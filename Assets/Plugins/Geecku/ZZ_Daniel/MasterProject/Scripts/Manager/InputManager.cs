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
            PlayerInput.actions.FindActionMap("Player").Enable();
        }

        #region Input Triggers

        #region Music and Improvisation
        public void OnImprovTest(InputValue value)
        {
            Debug.Log("in improv test");
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
            Debug.Log("in play root");
            AudioManager.Instance.PlayNote(Note.LOW_C);
        }
        public void OnPlayThird()
        {
            AudioManager.Instance.PlayNote(Note.LOW_E);
        }
        public void OnPlayFifth()
        {
            AudioManager.Instance.PlayNote(Note.LOW_G);
        }
        public void OnPlayOctave()
        {
            AudioManager.Instance.PlayNote(Note.HIGH_C);
        }
        #endregion

        #region Settings and UI
        //- default = TAB
        public void OnSettings(InputValue value)
        {
            Debug.Log("settings");
            GlobalUIManager.Instance.ToggleUI(UI.SETTINGS);
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
        }
        public void OnLook(InputValue value)
        {
            GameManager.Instance.Player.Look(value.Get<Vector2>());
        }
        #endregion

        #endregion
    }
}
