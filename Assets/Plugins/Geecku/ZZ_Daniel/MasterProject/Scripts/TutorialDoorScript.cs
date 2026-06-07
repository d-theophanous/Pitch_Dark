using UnityEngine;
using FMODUnity;
using FMOD;

namespace Daniel.Master
{
    public class TutorialDoorScript : Interactable
    {
        [SerializeField] private Animator DoorAnimator;

        public override void ActivatePrompt()
        {
            AudioManager.Instance.PlaySFX(SFX.OPEN_DOOR);
            TutorialManager.Instance.TriggerSegmentComplete();
            tag = "Untagged";
            //StudioEventEmitter emitter = gameObject.GetComponentInChildren<StudioEventEmitter>();
            //emitter.Stop();
            //emitter.gameObject.SetActive(false);

            AudioManager.Instance.StopInteractable();
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
    }
}
