using UnityEngine;

namespace Daniel.Master
{
    public class DoorScript : Interactable
    {
        [SerializeField] private Camera DoorCamera;
        public override void ActivatePrompt()
        {
            Debug.Log("Activated");
            //- Send message to other player
            //- Display waiting UI
            PuzzleManager.Instance.SetCurrentDoor(this);
            GlobalUIManager.Instance.ToggleUI(UI_Group.NETWORK_GATE);
        }
        public override void EnterInteractionRange()
        {
            base.EnterInteractionRange();
            //- mode dependent here I think
            //- ToDo (HP)
            //Rumbler.Instance.RumbleConstant(0.5f, 0.5f, 3f);
        }
        public Camera GetCamera() => DoorCamera;
    }
}
