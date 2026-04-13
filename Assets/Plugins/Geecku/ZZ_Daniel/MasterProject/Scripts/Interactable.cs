using Daniel.Master;
using Geecku.DefaultEngine;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Interactable : MonoBehaviour
{
    public virtual void ActivatePrompt() { }
    public bool IsOneTimeUse;

    [SerializeField] private List<GameObject> ObjectsToBeOutlined;

    public virtual void EnterInteractionRange()
    {
        GameManager.Instance.Player.CurrentInteractable = this;
        ToggleHighlight(true);
    }
    public virtual void ExitInteractionRange()
    {
        ToggleHighlight(false);
        GameManager.Instance.Player.CurrentInteractable = null;
    }

    #region Outline and MoveTo
    public List<GameObject> GetHighlightableObjects() => ObjectsToBeOutlined;
    public void MoveTo()
    {
        //PlayerScript player = GameManager.Instance.Player;
        //Vector3 player_pos = player.transform.position;
        //Vector3 tmp = Helper.GetNearestEdgeWithOffset(transform.position,
        //    player_pos, 0f);

        ////- if player is already very close to the object only rotate him and dont move
        //if (Vector3.Distance(player_pos, tmp) <= player.GetStopDistanceInteractables() + 1f)
        //{
        //    player.RotateCharacter(transform);
        //    return;
        //}

        //player.SetDestination(transform.position);
    }
    public void ToggleHighlight(bool turn_on)
    {
        if (turn_on)
        { 
            foreach (GameObject item in GetHighlightableObjects())
            {
                item.layer = LayerMask.NameToLayer("Outline");
            }
        }
        else
        {
            foreach (GameObject item in GetHighlightableObjects())
            {
                item.layer = LayerMask.NameToLayer("Interactable");
            }        
        }
    }
    private void OnMouseEnter()
    {
        //GameManager.Instance.CurHoverOverInteractable = this;
        //foreach (GameObject item in ObjectsToBeOutlined)
        //{
        //    item.layer = LayerMask.NameToLayer("Outline");
        //}
    }
    private void OnMouseExit()
    {
        //GameManager.Instance.CurHoverOverInteractable = null;
        //if (!(GameManager.Instance.Player.CurrentTarget == null))
        //    return;
        //foreach (GameObject item in ObjectsToBeOutlined)
        //{
        //    item.layer = LayerMask.NameToLayer("Interactable");
        //}
    }

    #endregion
}