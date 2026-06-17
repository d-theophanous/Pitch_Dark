using System.Collections;
using UnityEngine;

namespace Daniel.Master
{
    public class ReadablePuzzleLabel : ReadableLabel
    {
        //- this is the worst ever opt
        public override void Activate()
        {
            if (Audio == null) return;
            Number tmp_num = Number.FIRST;
            AudioManager.Instance.PlayNumberInput(tmp_num, Audio.name);
        }
    }
}
