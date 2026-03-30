using Daniel.Master;
using Geecku.GlobalMangers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Daniel.Master
{
    public class InputManager : PersistantDSingleton<InputManager>
    {
        #region Input Triggers
        public void OnSettings(InputValue value)
        {
            GlobalUIManager.Instance.ToggleSettings();
        }
        public void OnMagnify(InputValue value)
        {
            AccessibilityManager.Instance.ToggleMagnifier();
        }

        #endregion
    }
}
