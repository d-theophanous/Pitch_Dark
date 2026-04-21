// Assets/Scripts/Dialogue/DialogueData.cs

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Daniel.Master
{
    /// <summary>
    /// ScriptableObject that holds all dialogue lines for ONE language.
    ///
    /// Create one asset per language:
    ///   Right-click in Project → Create → Dialogue → Dialogue Data
    ///   Name them e.g. DialogueData_EN, DialogueData_NL, DialogueData_DE
    ///
    /// The TTSGeneratorWindow reads the 'lines' list to generate audio clips.
    /// </summary>
    [CreateAssetMenu(
        fileName = "DialogueData",
        menuName  = "Dialogue/Dialogue Data",
        order     = 0)]
    public class DialogueData : ScriptableObject
    {
        [Tooltip("Language identifier for this asset, e.g. EN, NL, DE.")]
        public string Language = "EN";

        [Tooltip("All dialogue lines for this language.")]
        public List<string> Lines = new();

        [Tooltip("All dialogue lines for this language.")]
        public List<AudioClip> Audio = new();

    }
}
