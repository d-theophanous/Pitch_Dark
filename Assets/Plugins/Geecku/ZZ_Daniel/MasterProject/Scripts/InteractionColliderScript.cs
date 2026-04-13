using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractionColliderScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Interactable")
            return;

        if (other.gameObject.activeSelf)
        {
            Debug.Log("in ontrgigger enter");
            Interactable tmp = other.GetComponent<Interactable>();
            tmp.EnterInteractionRange();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Interactable")
            return;

        if (other.gameObject.activeSelf)
        {
            Interactable tmp = other.GetComponent<Interactable>();
            tmp.ExitInteractionRange();
        }
    }
}