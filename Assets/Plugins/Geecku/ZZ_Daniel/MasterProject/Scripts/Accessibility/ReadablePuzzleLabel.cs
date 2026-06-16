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
            //- ToDo noch button oder label am ende sagen
            switch (AudioIndex)
            {
                case 0:
                    tmp_num = Number.FIRST;
                    break;
                case 1:
                    tmp_num = Number.SECOND;
                    break;
                case 2:
                    tmp_num = Number.THIRD;
                    break;
                case 3:
                    tmp_num = Number.FOURTH;
                    break;
                default:
                    break;
            }
            AudioManager.Instance.PlayNumberInput(tmp_num, Audio.name);
        }
    }
}
