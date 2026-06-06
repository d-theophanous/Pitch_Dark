using UnityEngine;

namespace Daniel.Master
{
    public class TutorialDoorScript : Interactable
    {
        [SerializeField] private Animator DoorAnimator;

        public override void ActivatePrompt()
        {
            AudioManager.Instance.PlaySFX(SFX.OPEN_DOOR);
            TutorialManager.Instance.TriggerSegmentComplete();
            Rumbler.Instance.StopRumble();
            tag = "Untagged";
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
    }
}
