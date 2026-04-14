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
    }
}
