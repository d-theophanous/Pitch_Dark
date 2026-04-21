using System;
using System.Collections.Generic;
using UnityEngine;

namespace Daniel.Master
{
    [CreateAssetMenu(
        fileName = "DialogueContainer",
        menuName = "Dialogue/Dialogue Container",
        order = 0)]
    public class DialogueContainer : ScriptableObject
    {
        [Tooltip("All dialogue data, all lines and audio in all languages")]
        public List<DialogueData> DialogueList;

        [Tooltip("All sounds that should be played after a dialogue line")]
        public List<Message_Tone> Tones = new();

        [Tooltip("All actions that should be invoked after or during a dialogue line")]
        public List<Action> Actions = new();
    }
}
