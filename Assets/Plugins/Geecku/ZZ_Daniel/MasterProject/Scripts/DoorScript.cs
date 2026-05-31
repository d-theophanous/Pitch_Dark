using UnityEngine;

namespace Daniel.Master
{
    public class DoorScript : Interactable
    {
        //- add door open animation
        //- after animation zoom npc, dialogue etc...
        [SerializeField] private Camera DoorCamera;
        [SerializeField] private Animator DoorAnimator;
        [SerializeField] private BoxCollider DoorTrigger;
        [SerializeField] private GameObject NavMeshObstacle;
        public NPCScript DoorNPC;
        public override void ActivatePrompt()
        {
            PuzzleManager.Instance.SetCurrentDoor(this);
            Rumbler.Instance.StopRumble();
            GlobalUIManager.Instance.ToggleUI(UI_Group.NETWORK_GATE);
            tag = "Untagged";
            GameManager.Instance.Player.CurrentInteractable = null;
        }
        public override void EnterInteractionRange()
        {
            base.EnterInteractionRange();
            //- mode dependent here I think
            //- ToDo (HP)
            Rumbler.Instance.RumbleConstant(0.5f, 0.5f, 100f);
        }
        public override void ExitInteractionRange()
        {
            base.ExitInteractionRange();
            Rumbler.Instance.StopRumble();
        }
        public Camera GetCamera() => DoorCamera;

        public void ToggleDoor(bool open)
        {
            if (open)
            {
                //- sounds gets played in puzzle manager
                NavMeshObstacle.SetActive(false);
                DoorAnimator.SetBool("open_door", true);
                DoorAnimator.SetBool("close_door", false);
            }
            else
            {
                AudioManager.Instance.PlaySFX(SFX.CLOSE_DOOR);
                NavMeshObstacle.SetActive(true);
                DoorAnimator.SetBool("open_door", false);
                DoorAnimator.SetBool("close_door", true);
            }
        }
    }
}
