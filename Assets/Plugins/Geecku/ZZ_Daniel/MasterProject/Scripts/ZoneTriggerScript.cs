using System;
using UnityEngine;
using UnityEngine.Events;

namespace Daniel.Master
{
    [RequireComponent(typeof(Collider))]
    public class ZoneTriggerScript : MonoBehaviour
    {
        public bool DeactivateAfterTrigger;
        public UnityEvent TriggerEnterAction;
        public UnityEvent TriggerStayAction;
        public UnityEvent TriggerExitAction;

        private void OnTriggerEnter(Collider other)
        {
            TriggerEnterAction?.Invoke();
            if (DeactivateAfterTrigger)
                Disable();
        }
        private void OnTriggerStay(Collider other)
        {
            TriggerStayAction?.Invoke();
        }
        private void OnTriggerExit(Collider other)
        {
            //- for now ok
            if (other.tag == "Player")
                TriggerExitAction?.Invoke();

            if (DeactivateAfterTrigger)
                Disable();
        }
        private void Disable()
        {
            gameObject.SetActive(false);
        }

    }
}
