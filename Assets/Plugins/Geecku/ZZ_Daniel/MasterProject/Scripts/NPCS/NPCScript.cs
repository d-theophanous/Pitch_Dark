using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Daniel.Master
{
    public class NPCScript : Interactable
    {
        [SerializeField] private Animator Animator;
        protected bool ImproviseAfterSecondDialogue;
        protected bool IsFollowingFromStart;

        [Header("Follow Settings")]
        public float StopDistance = 4f;
        public float UpdateRate = 0.1f; // seconds between destination updates

        [Header("State")]
        public bool IsFollowing = false;

        [SerializeField] public NavMeshAgent Agent;
        [SerializeField] protected List<DialogueContainer> DialogueList;
        [SerializeField] private Transform Parent;
        [SerializeField] private Camera NPCCamera;
        private float UpdateTimer;
        private Transform Player;
        protected List<Dictionary<int, Action>> ActionDicList = new(); //-  I am going insane

        void Awake()
        {
            Player = GameManager.Instance.Player.transform;
        }
        protected virtual void Start()
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
            var player = GameManager.Instance.Player;
            if (IsFollowing && !player.NPCFollowerList.Contains(this))
                player.NPCFollowerList.Add(this);
            else
                player.NPCFollowerList.Remove(this);

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
            Debug.Log("in activate prompt");
            GameManager.Instance.Player.ResetMovement();
            DialogueManager.Instance.SetCurrentNPC(this);
            if (!IsFollowingFromStart)
                ToggleFollowing();
            LookAtPlayer();
            //- nur zum Testen?
            //- ToDo 
            if (ActionDicList.Count == 0)
            {
                ActionDicList.Add(new());
            }
            DialogueManager.Instance.StartNPCDialogue(
                DialogueList[0], ActionDicList[0]);
            tag = "Untagged";
        }
        public void ActivateSecondDialogue()
        {
            GameManager.Instance.Player.ResetMovement();
            DialogueManager.Instance.SetCurrentNPC(this);
            LookAtPlayer();
            //- ToDo
            if (ActionDicList.Count < 2)
            {
                Debug.Log("in ugly if");
                ActionDicList.Add(new());
            }

            DialogueManager.Instance.StartNPCDialogue(
                DialogueList[1], ActionDicList[1], ImproviseAfterSecondDialogue);
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
        protected void LookAtPlayer()
        {
            Parent.forward = GameManager.Instance.Player.transform.position - Parent.position;            
        }
        public Camera GetCamera()
        {
            return NPCCamera;
        }
    }
}
