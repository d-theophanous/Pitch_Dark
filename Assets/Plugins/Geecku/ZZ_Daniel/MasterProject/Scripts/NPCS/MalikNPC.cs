using System;
using System.Collections.Generic;
using UnityEngine;

namespace Daniel.Master
{
    public class MalikNPC : NPCScript
    {
        protected override void Start()
        {
            base.Start();
            SetUpDialogueActions();
            if (GameManager.Instance.SkipTutorial)
                SetFollowing(true);
            Pitch = -1;
            ImproviseAfterSecondDialogue = false;
            IsFollowingFromStart = true;
        }
        private void SetUpDialogueActions()
        {
            Dictionary<int, Action> dialogue_1_dic = new();
            dialogue_1_dic.Add(0, () =>
            {
                DialogueManager.Instance.TutorialAfter = false; 
                DialogueManager.Instance.ContinueWithDialogue = true;
            });
            dialogue_1_dic.Add(DialogueList[0].DialogueList[0].Lines.Count, () =>
            {
                DirectionChecker.Instance.StartDirectionChecking();
                DialogueManager.Instance.ContinueWithDialogue = true;
            });

            Dictionary<int, Action> dialogue_2_dic = new();
            dialogue_2_dic.Add(DialogueList[1].DialogueList[0].Lines.Count, () =>
            {
                PuzzleManager.Instance.SetUpPuzzle();
            });

            ActionDicList.Add(dialogue_1_dic);
            ActionDicList.Add(dialogue_2_dic);
        }
    }
}
