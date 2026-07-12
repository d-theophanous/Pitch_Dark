using System;
using System.Collections.Generic;
using UnityEngine;

namespace Daniel.Master
{
    public class MalikNPC : NPCScript
    {
        public DoorScript Door;
        protected override void Awake()
        {
            base.Awake();
            Pitch = -1;
        }
        protected override void Start()
        {
            base.Start();
            SetUpDialogueActions();
            if (GameManager.Instance.SkipTutorial)
                SetFollowing(true);
            ImproviseAfterSecondDialogue = false;
            IsFollowingFromStart = true;
        }
        public override void ActivatePrompt()
        {
            base.ActivatePrompt();
            GameManager.Instance.CurrentGateReadyAction = () => { PuzzleManager.Instance.StartPuzzle(); };
        }
        private void SetUpDialogueActions()
        {
            Dictionary<int, Action> dialogue_1_dic = new();
            dialogue_1_dic.Add(0, () =>
            {
                DialogueManager.Instance.TutorialAfter = false; 
                DialogueManager.Instance.ContinueWithDialogue = true;
                DirectionChecker.Instance.StartDirectionChecking();
            });
            dialogue_1_dic.Add(DialogueList[0].DialogueList[0].Lines.Count, () =>
            {
                DialogueManager.Instance.ContinueWithDialogue = true;
            });

            Dictionary<int, Action> dialogue_2_dic = new();
            dialogue_2_dic.Add(DialogueList[1].DialogueList[0].Lines.Count, () =>
            {
                Door.TutorialNPC = null;
                Door.ActivatePrompt();
            });

            ActionDicList.Add(dialogue_1_dic);
            ActionDicList.Add(dialogue_2_dic);
        }
    }
}
