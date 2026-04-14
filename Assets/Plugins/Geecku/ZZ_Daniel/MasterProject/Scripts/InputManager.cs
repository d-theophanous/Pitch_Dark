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

        #region Input Triggers

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
