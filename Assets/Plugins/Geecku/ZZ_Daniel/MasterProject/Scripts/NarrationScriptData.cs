using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject holding a list of narration lines for the TTS generator.
/// </summary>
[CreateAssetMenu(fileName = "NarrationScriptData", menuName = "TTS/Narration Script Data")]
public class NarrationScriptData : ScriptableObject
{
    [TextArea(2, 4)]
    public List<string> lines = new List<string>();
}
