using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace Daniel.Master
{
    public class NPCScript : Interactable
    {
        [SerializeField] private Animator Animator;

        [Header("Follow Settings")]
        public float StopDistance = 4f;
        public float UpdateRate = 0.1f; // seconds between destination updates

        [Header("State")]
        public bool IsFollowing = false;

        [SerializeField] private NavMeshAgent Agent;
        [SerializeField] private List<DialogueContainer> DialogueList;
        [SerializeField] private Transform Parent;
        [SerializeField] private Camera NPCCamera;
        private float UpdateTimer;
        private Transform Player;

        //- very ugly, very temporary, ToDo
        [SerializeField] private List<DialogueContainer> DialogueList2;
        private List<DialogueContainer> DialogueListFinal
        {
            get
            {
                if (GameManager.Instance.PlayerIdx == 0)
                    return DialogueList;
                return DialogueList2;
            }
        }
        void Awake()
        {
            Player = GameManager.Instance.Player.transform;
        }
        private void Start()
        {
            Agent.speed = GameManager.Instance.Player.PlayerSpeed - 1.5f;
        }   
        //- (LP) ToDo GameManager Update steuern lassen?
        void Update()
        {
            UpdateMovement();
        }

        #region Movement
        private void UpdateMovement()
        {
            if (!IsFollowing || Player == null)
            {
                Agent.ResetPath();
                return;
            }

            UpdateTimer += Time.deltaTime;
            if (UpdateTimer < UpdateRate) return;
            UpdateTimer = 0f;

            float distanceToPlayer = Vector3.Distance(transform.position, Player.position);

            if (distanceToPlayer > StopDistance)
            {
                Agent.isStopped = false;
                Agent.SetDestination(Player.position);
                Animator.SetBool("IsWalking", true);
                Animator.SetBool("IsPlaying", false);
            }
            else
            {
                Agent.isStopped = true;
                Animator.SetBool("IsWalking", false);
                Animator.SetBool("IsPlaying", true);
                Agent.ResetPath();
            }
        }

        /// <summary>
        /// Toggle follow mode on or off. Call this from UI, animation events, or other scripts.
        /// </summary>
        public void SetFollowing(bool follow)
        {
            IsFollowing = follow;

            if (!IsFollowing)
            {
                Agent.isStopped = true;
                Agent.ResetPath();
                Animator.SetBool("IsWalking", false);
                Animator.SetBool("IsPlaying", true);
            }
            else
            {
                Agent.isStopped = false;
            }
        }
        /// <summary>
        /// Convenience method to flip the current follow state.
        /// </summary>
        public void ToggleFollowing()
        {
            SetFollowing(!IsFollowing);
        }
        #endregion

        public override void EnterInteractionRange()
        {
            ActivatePrompt();
        }
        public override void ActivatePrompt()
        {
            GameManager.Instance.Player.ResetMovement();
            DialogueManager.Instance.SetCurrentNPC(this);
            DialogueManager.Instance.AddNPCFollowers();
            LookAtPlayer();
            Debug.Log("NPC interaction");
            //- nur zum Testen?
            DialogueManager.Instance.StartDialogue(
                DialogueListFinal[0]);
            tag = "Untagged";
        }
        public void ActivateSecondDialogue()
        {
            GameManager.Instance.Player.ResetMovement();
            DialogueManager.Instance.SetCurrentNPC(this);
            LookAtPlayer();
            DialogueManager.Instance.StartDialogue(
                DialogueListFinal[1], true);
            //- after second dialogue NPC will not be interactable anymore
            this.tag = "Untagged";
        }
        //- gerade egal, weil die eh die ganze Zeit spielen haha
        public void StartPlay()
        {

        }
        public void StopPlay()
        {

        }
        private void LookAtPlayer()
        {
            Parent.forward = GameManager.Instance.Player.transform.position - Parent.position;            
        }
        public Camera GetCamera()
        {
            return NPCCamera;
        }
    }
}
