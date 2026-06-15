using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

namespace Daniel.Master
{
    public class PlayerScript : MonoBehaviour
    {
        //- ToDo change to List
        [SerializeField] private Transform Player1Spawn;
        [SerializeField] private Transform Player2Spawn;
        [SerializeField] private List<Transform> PlayerStartSpawnList;
        [SerializeField] private NavMeshAgent NavMeshAgent;
        [SerializeField] private CinemachineCamera NPCCamera;
        [SerializeField] private Camera PlayerCamera;
        public List<NPCScript> NPCFollowerList = new();
        public bool IsMoving => Movement.magnitude > 0;
        //public Vector3 WalkDirection => new Vector3(Movement.y, 0, -Movement.x);
        public Vector3 WalkDirection
        {
            get
            {
                Vector3 move = new Vector3(Movement.x, 0f, Movement.y);
                move = Vector3.ClampMagnitude(move, 1f);
                move = CinCam.transform.rotation * move;
                move.y = 0f;
                return move.normalized;
            }
        }

        public CinemachineCamera CinCam;
        public Interactable CurrentInteractable;

        public float PlayerSpeed {  get; private set; }

        #region Unity Commons
        private void Awake()
        {
            GameManager.Instance.Player = this;
            PlayerSpeed = 2.5f;
            GlobalUIManager.Instance.PlayerViewCamera = PlayerCamera;
        }
        private void Start()
        {
            SpawnPlayer();
            GameManager.Instance.MovementEvents.Add(UpdatePlayer);
            GameManager.Instance.SubscribeMovementEvents();
        }
        public void UpdatePlayer(object sender, EventArgs e)
        {
            RotateCharacter();
            MoveCharacter();
            CheckForChange();
        }
        #endregion

        private void SpawnPlayer()
        {
            Transform tmp;
            //- CurRotationIdx is very unflexible this way
            if (GameManager.Instance.PlayerNumber == 1)
            {
                if (GameManager.Instance.SkipTutorial)
                    tmp = PlayerStartSpawnList[0];
                else
                    tmp = Player1Spawn;
                CurRotationIdx = 1;
            }
            else
            {
                if (GameManager.Instance.SkipTutorial)
                    tmp = PlayerStartSpawnList[1];
                else
                    tmp = Player2Spawn;
                CurRotationIdx = 1;
            }

            TeleportCharacter(tmp.position);
            transform.forward = tmp.forward;
            CinCam.transform.rotation = LookDirList[CurRotationIdx];
        }
        public void SpawnPlayerAtStart()
        {
            TeleportCharacter(PlayerStartSpawnList[GameManager.Instance.PlayerIdx].transform.position);
        }

        public void Interact()
        {
            if (CurrentInteractable == null)
                return;
            CurrentInteractable.ActivatePrompt();
        }
        public void TeleportNPCsToPlayer(Transform position)
        {
            foreach (var npc in NPCFollowerList)
            {
                npc.Agent.Warp(position.position);
            }
        }

        #region Movement
        public Vector2 Movement = new();
        public void Move(Vector2 value)
        {
            Movement = value;
        }
        private Vector2 LookDir = new();
        public void Look(Vector2 value)
        {
            LookDir = value;
        }
        private void MoveCharacter()
        {
            Vector3 move = new Vector3(Movement.x, 0f, Movement.y);
            move = Vector3.ClampMagnitude(move, 1f);


            move = CinCam.transform.rotation * move;
            move.y = 0f; // safety — keep movement flat


            // Move
            Vector3 finalMove = move * PlayerSpeed;
            NavMeshAgent.Move(finalMove * Time.deltaTime);
        }

        private List<Quaternion> LookDirList = new()
        {
            Quaternion.Euler(10, 0, 0), Quaternion.Euler(10, 90, 0),
            Quaternion.Euler(10, 180, 0), Quaternion.Euler(10, 270, 0)
        };
        private int CurRotationIdx = 0;
        private bool LookWasActive = false;
        private void RotateCharacter()
        {
            //- prevent rotation triggering multiple times when moving the stick
            bool look_is_active = LookDir != Vector2.zero;
            if (look_is_active && !LookWasActive)
            {
                if (Mathf.Abs(LookDir.x) >= Mathf.Abs(LookDir.y))
                {
                    if (LookDir.x > 0)
                        CurRotationIdx++;
                    else
                        CurRotationIdx--;
                }
                else
                    if (LookDir.y <= 0)
                    CurRotationIdx = CurRotationIdx + 2;

                if (CurRotationIdx < 0)
                    CurRotationIdx = LookDirList.Count - 1;
                CinCam.transform.rotation = LookDirList[CurRotationIdx % LookDirList.Count];
                transform.rotation = CinCam.transform.rotation;
            }
            LookWasActive = look_is_active;
        }
        public  void ResetMovement()
        {
            Movement = PreviousMoveInput = new Vector2 (0, 0);
            AudioManager.Instance.StopFootsteps();
            NavMeshAgent.isStopped = true;
        }
        private Vector2 PreviousMoveInput = Vector2.zero;
        private void CheckForChange()
        {
            if (PreviousMoveInput.magnitude == 0 && Movement.magnitude != 0)
                AudioManager.Instance.PlayFootsteps();
            else if (PreviousMoveInput.magnitude != 0 && Movement.magnitude == 0)
                AudioManager.Instance.StopFootsteps();
            PreviousMoveInput = Movement;
        }
        public void TeleportCharacter(Vector3 new_pos)
        {
            NavMeshAgent.Warp(new_pos);
        }
        #endregion
    }
}
