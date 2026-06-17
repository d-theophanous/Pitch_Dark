using FMODUnity;
using Geecku.DefaultNetworking;
using UnityEngine;

namespace Daniel.Master
{
    public class DoorScript : Interactable
    {
        [SerializeField] private Camera DoorCamera;
        [SerializeField] private Animator DoorAnimator;
        [SerializeField] private BoxCollider DoorTrigger;
        [SerializeField] private GameObject NavMeshObstacle;
        public Transform AfterPuzzlePositionNPC;
        public Transform AfterPuzzlePositionPlayer;
        public NPCScript DoorNPC;
        public NPCScript TutorialNPC;
        public override void ActivatePrompt()
        {
            PuzzleManager.Instance.SetCurrentDoor(this);

            AudioManager.Instance.StopInteractable();
            GlobalUIManager.Instance.ToggleUI(UI_Group.NETWORK_GATE);

            if (NetworkManager.Instance._Client.IsInConnection)
                GameManager.Instance.ClientSend_PlayerAtGate();
            else
            {
                GameManager.Instance.PlayerGateReady();
                GameManager.Instance.OtherPlayerGateReady();
            }
            tag = "Untagged";
            GameManager.Instance.Player.CurrentInteractable = null;
            DirectionChecker.Instance.StopDirectionChecking();
        }
        public override void EnterInteractionRange()
        {
            base.EnterInteractionRange();
            AudioManager.Instance.PlayInteractable();
        }
        public override void ExitInteractionRange()
        {
            base.ExitInteractionRange();
            AudioManager.Instance.StopInteractable();
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
                NavMeshObstacle.SetActive(true);
                DoorAnimator.SetBool("open_door", false);
                DoorAnimator.SetBool("close_door", true);
            }
        }
    }
}
