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
            GlobalUIManager.Instance.ToggleUI(UI_Group.NETWORK_GATE);
        }
        public override void EnterInteractionRange()
        {
            base.EnterInteractionRange();
            //- mode dependent here I think
            Rumbler.Instance.RumbleConstant(0.5f, 0.5f, 3f);
        }
    }
}
