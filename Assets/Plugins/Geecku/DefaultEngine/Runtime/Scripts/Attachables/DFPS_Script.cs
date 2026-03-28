using TMPro;
using UnityEngine;

namespace Geecku.DefaultEngine.Attachables
{
    public class DFPS_Script : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI FPS_Text;

        private float deltaTime = 0.0f;
        private void Update()
        {
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;
            FPS_Text.text = Mathf.Ceil(fps).ToString() + " FPS";
        }
    }
}
