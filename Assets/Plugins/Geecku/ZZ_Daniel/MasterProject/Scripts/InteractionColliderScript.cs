using Daniel.Master;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractionColliderScript : MonoBehaviour
{
    private Collider CurrentWallCollider;
    private Collider NextWallCollider;
    private bool PlayWallSound => PlayerTouchesWall && GameManager.Instance.Player.IsMoving;
    private bool PlayerTouchesWall;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable") && other.gameObject.activeSelf)
        {
            Interactable tmp = other.GetComponent<Interactable>();
            tmp.EnterInteractionRange();
        }
        if (other.CompareTag("Wall"))
            CheckInitialWallTouch(other);
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactable") && other.gameObject.activeSelf)
        {
            Interactable tmp = other.GetComponent<Interactable>();
            tmp.ExitInteractionRange();
        }
        if (other.CompareTag("Wall"))
            StartCoroutine(ExitWallCoroutine());
    }

    #region Wall Collision Check
    private IEnumerator ExitWallCoroutine()
    {
        if (NextWallCollider != null)
        {
            CurrentWallCollider = NextWallCollider;
            NextWallCollider = null;
            yield break;
        }

        PlayerTouchesWall = false;
        GameManager.Instance.UpdateEvent -= Instance_UpdateEvent;
        StopAllSound();
        CurrentWallCollider = null;
        NextWallCollider = null;
    }
    private void CheckInitialWallTouch(Collider wall)
    {
        if (CurrentWallCollider != null)
        {
            NextWallCollider = wall;
        }
        else
        {
            PlayerTouchesWall = true;
            GameManager.Instance.UpdateEvent += Instance_UpdateEvent;
            CurrentWallCollider = wall;
        }

    }

    private void Instance_UpdateEvent(object sender, System.EventArgs e)
    {
        if (PlayWallSound)
        {
            if (IsFacingTheWall(CurrentWallCollider.gameObject))
            {
                AudioManager.Instance.StartFaceWall();
            }
            else
            {
                AudioManager.Instance.StartWallScratch();
            }
        }
        else
        {
            StopAllSound();
        }
    }
    private void StopAllSound()
    {
        AudioManager.Instance.StopWallScratch();
        AudioManager.Instance.StopFaceWall();
    }
    private bool IsFacingTheWall(GameObject wall)
    {
        GameObject player = GameManager.Instance.Player.gameObject;
        float dot = Vector3.Dot(wall.transform.forward, player.transform.forward);
        Debug.Log("dot: " + Mathf.Abs(dot));
        return Mathf.Abs(dot) > 0.5f;
    }
    #endregion
}