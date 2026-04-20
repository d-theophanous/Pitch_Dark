using JetBrains.Annotations;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace Daniel.Master
{
    public class PlayerScript : MonoBehaviour
    {
        [SerializeField] private Transform Player1Spawn;
        [SerializeField] private Transform Player2Spawn;
        [SerializeField] private NavMeshAgent NavMeshAgent;

        public CinemachineCamera CinCam;
        public Interactable CurrentInteractable;

        private float PlayerSpeed = 5.0f;

        private void Awake()
        {
            GameManager.Instance.Player = this;
        }
        private void Start()
        {
            SpawnPlayer();
        }

        public void UpdatePlayer()
        {
            RotateCharacter();
            MoveCharacter();
        }

        private void SpawnPlayer()
        {
            Transform tmp;
            if (GameManager.Instance.PlayerNumber == 1)
                tmp = Player1Spawn;
            else
                tmp = Player2Spawn;


            transform.position = tmp.position;
            transform.forward = tmp.forward;

            CurRotationIdx = 1;
            CinCam.transform.rotation = LookDirList[CurRotationIdx];
        }

        public void Interact()
        {
            if (CurrentInteractable == null)
            {
                Debug.Log("bin null");
                return;
            }
            CurrentInteractable.ActivatePrompt();
        }

        #region Movement
        private Vector2 Movement = new();
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
        #endregion
    }
}
