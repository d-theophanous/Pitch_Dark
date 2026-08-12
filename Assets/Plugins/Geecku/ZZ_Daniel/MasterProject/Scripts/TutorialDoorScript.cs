using FMOD;
using FMODUnity;
using UnityEngine;
using UnityEngine.AI;

namespace Daniel.Master
{
    public class TutorialDoorScript : Interactable
    {
        [SerializeField] private Animator DoorAnimator;

        public override void ActivatePrompt()
        {
            DialogueManager.Instance.TutorialAfter = false;
            TutorialManager.Instance.TriggerSegmentComplete();
            tag = "Untagged";
            //StudioEventEmitter emitter = gameObject.GetComponentInChildren<StudioEventEmitter>();
            //emitter.Stop();
            //emitter.gameObject.SetActive(false);

            AudioManager.Instance.StopInteractable();
            ToggleDoor(true);
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
        public void ToggleDoor(bool open)
        {
            if (open)
            {
                AudioManager.Instance.PlaySFX(SFX.OPEN_DOOR);
                DoorAnimator.SetBool("open_door", true);
                DoorAnimator.SetBool("close_door", false);
            }
            else
            {
                DoorAnimator.SetBool("open_door", false);
                DoorAnimator.SetBool("close_door", true);
            }
        }
    }
}
