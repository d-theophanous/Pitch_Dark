using JetBrains.Annotations;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Daniel.Master
{
    public class PlayerScript : MonoBehaviour
    {
        private float playerSpeed = 5.0f;

        public CharacterController Controller;
        public CinemachineCamera CinCam;
        private Vector3 PlayerVelocity;


        void Update()
        {
            Vector3 move = new Vector3(Movement.x, 0f, Movement.y);
            move = Vector3.ClampMagnitude(move, 1f);

            if (move != Vector3.zero)
            {
                transform.forward = move;
                CinCam.transform.forward = move;
            }

            // Move
            Vector3 finalMove = move * playerSpeed + Vector3.up * PlayerVelocity.y;
            Controller.Move(finalMove * Time.deltaTime);
        }

        #region Movement
        //- in InputManager auslagern (ToDo)
        public void OnLook(InputValue value)
        {
            Vector2 lookDelta = value.Get<Vector2>();
        }
        private Vector2 Movement = new();
        public void OnMove(InputValue value)
        {
            Movement = value.Get<Vector2>();
        }
        #endregion
    }
}
