using System;
using UnityEngine;
using UnityEngine.Events;

namespace Daniel.Master
{
    [RequireComponent(typeof(Collider))]
    public class ZoneTriggerScript : MonoBehaviour
    {
        public UnityEvent TriggerEnterAction;
        public UnityEvent TriggerStayAction;
        public UnityEvent TriggerExitAction;
        public Collider Collider { get; private set; }

        private void Awake()
        {
            Collider = GetComponent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            TriggerEnterAction?.Invoke();
        }
        private void OnTriggerStay(Collider other)
        {
            TriggerStayAction?.Invoke();
        }
        private void OnTriggerExit(Collider other)
        {
            //- for now ok
            Debug.Log(other.tag);
            if (other.tag == "Player")
                TriggerExitAction?.Invoke();
        }

    }
}
