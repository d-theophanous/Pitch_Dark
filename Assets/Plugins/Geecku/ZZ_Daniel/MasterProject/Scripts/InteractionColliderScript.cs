using Daniel.Master;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractionColliderScript : MonoBehaviour
{
    public Collider CurrentWallCollider;
    public Collider NextWallCollider;
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
            StartCoroutine(ExitWallCoroutine(other));
    }

    #region Wall Collision Check
    private IEnumerator ExitWallCoroutine(Collider next)
    {
        if (NextWallCollider != null)
        {
            if (next != NextWallCollider)
            {
                CurrentWallCollider = NextWallCollider;
            }

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
    //- optimizable haha ToDo
    private void Instance_UpdateEvent(object sender, System.EventArgs e)
    {
        if (PlayWallSound)
        {
            float dot_prod_cur = GetDotProd(CurrentWallCollider.gameObject);
            //- case dot product: 1 faces the wall
            if ((NextWallCollider != null && GetDotProd(NextWallCollider.gameObject) > 0.9f ) ||
                dot_prod_cur > 0.9f)
            {
                AudioManager.Instance.StartFaceWall();
                AudioManager.Instance.StopWallScratch();
            }
            //- case dot product: 0 scratches the wall
            else if (dot_prod_cur < 0.1f || 
                (NextWallCollider != null && GetDotProd(NextWallCollider.gameObject) < 0.1f))
            {
                AudioManager.Instance.StopFaceWall();
                AudioManager.Instance.StartWallScratch();
            }
            else if (IsCorner())
            {
                AudioManager.Instance.StopWallScratch();
                AudioManager.Instance.StartFaceWall();
            }
            else
            {
                AudioManager.Instance.StartWallScratch();
                AudioManager.Instance.StartFaceWall();
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
    private float GetDotProd(GameObject wall)
    {
        return GetDotProd(wall, GameManager.Instance.Player.WalkDirection);
    }
    private float GetDotProd(GameObject wall, Vector3 check_vector)
    {
        float dot = Vector3.Dot(wall.transform.forward, check_vector);
        return Mathf.Abs(dot);
    }
    private bool IsCorner()
    {
        if (NextWallCollider == null)
            return false;
        if (
            (GetDotProd(NextWallCollider.gameObject, new Vector3(1f,0f,0f)) >= 0.9f ||
            GetDotProd(NextWallCollider.gameObject, new Vector3(0f,0f,1f)) >= 0.9f) &&
            (GetDotProd(CurrentWallCollider.gameObject, new Vector3(1f, 0f, 0f)) >= 0.9f ||
            GetDotProd(CurrentWallCollider.gameObject, new Vector3(0f, 0f, 1f)) >= 0.9f))
        {
            return true;
        }
        return false;
    }
    #endregion
}