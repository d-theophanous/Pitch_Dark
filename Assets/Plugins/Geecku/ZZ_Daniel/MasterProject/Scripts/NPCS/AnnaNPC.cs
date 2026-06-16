using System;
using System.Collections.Generic;
using UnityEngine;

namespace Daniel.Master
{
    public class AnnaNPC : NPCScript
    {
        protected override void Start()
        {
            base.Start();
            Pitch = 1f;
            SetUpSegments();
        }
        private void SetUpSegments()
        {
            Dictionary<int, Action> dialogue_1_dic = new();
            dialogue_1_dic.Add(DialogueList[0].DialogueList[0].Lines.Count, () =>
            {
                PuzzleManager.Instance.SetUpPuzzle();
            });

            ActionDicList.Add(dialogue_1_dic);
        }
    }
}
