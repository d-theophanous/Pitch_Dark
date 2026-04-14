using Daniel.Master;
using UnityEngine;

namespace Geecku
{
    public class DoorScript : Interactable
    {
        public override void ActivatePrompt()
        {
            Debug.Log("Activated");
            //- Send message to other player
            //- Display waiting UI
            GlobalUIManager.Instance.ToggleUI(UI.GATE_NET);
        }
        public override void EnterInteractionRange()
        {
            base.EnterInteractionRange();
            Rumbler.Instance.RumbleConstant(0.5f, 0.5f, 3f);
        }
    }
}
