using Daniel.Master;
using Geecku.GlobalMangers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Daniel.Master
{
    public class InputManager : PersistantDSingleton<InputManager>
    {
        #region Input Triggers
        //- default = TAB
        public void OnSettings(InputValue value)
        {
            GlobalUIManager.Instance.ToggleSettings();
            TTSManager.Instance.Speak("Your dialogue text here");
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
    }
}
