using UnityEngine;
using UnityEngine.AI;

namespace Daniel.Master
{
    public class NPCScript : Interactable
    {
        [SerializeField] private Animator Animator;

        [Header("Follow Settings")]
        public float StopDistance = 2.5f;
        public float UpdateRate = 0.1f; // seconds between destination updates

        [Header("State")]
        public bool IsFollowing = false;

        [SerializeField] private NavMeshAgent Agent;
        [SerializeField] private DialogueData Dialogue;
        private float UpdateTimer;
        private Transform Player;

        void Awake()
        {
            Player = GameManager.Instance.Player.transform;
        }
        //- (LP) ToDo GameManager Update steuern lassen?
        void Update()
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
        public override void ActivatePrompt()
        {
            Debug.Log("NPC interaction");
            DialogueManager.Instance.StartDialogue(Dialogue);
            ToggleFollowing();
        }
        public void StartPlay()
        {

        }
        public void StopPlay()
        {

        }
    }
}
