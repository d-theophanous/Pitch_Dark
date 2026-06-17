using System;
using System.Collections.Generic;
using UnityEngine;

namespace Daniel.Master
{
    public class AnnaNPC : NPCScript
    {
        public DoorScript Door;
        protected override void Start()
        {
            base.Start();
            Pitch = 1f;
            SetUpSegments();
            ImproviseAfterSecondDialogue = true;
        }
        private void SetUpSegments()
        {
            Dictionary<int, Action> dialogue_1_dic = new();
            dialogue_1_dic.Add(DialogueList[0].DialogueList[0].Lines.Count, () =>
            {
                Door.ActivatePrompt();
            });
            Dictionary<int, Action> dialogue_2_dic = new();
            dialogue_2_dic.Add(DialogueList[1].DialogueList[0].Lines.Count - 1, () =>
            {
                string key = Helper.GetLanguageString() + "_switch_instruments";
                AudioManager.Instance.PlayDialogue(key, () =>
                {
                    DialogueManager.Instance.ContinueWithDialogue = true;
                    DialogueManager.Instance.Dialogue.Activate();
                });
            });

            ActionDicList.Add(dialogue_1_dic);
            ActionDicList.Add(dialogue_2_dic);
        }
    }
}
