using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Daniel.Master
{
    public class PlayerScript : MonoBehaviour
    {
        private float playerSpeed = 5.0f;

        public CharacterController controller;
        private Vector3 playerVelocity;


        void Update()
        {
            Vector3 move = new Vector3(Movement.x, 0f, Movement.y);
            move = Vector3.ClampMagnitude(move, 1f);

            if (move != Vector3.zero)
                transform.forward = move;

            // Move
            Vector3 finalMove = move * playerSpeed + Vector3.up * playerVelocity.y;
            controller.Move(finalMove * Time.deltaTime);
        }

        #region Movement
        //- in INputManager auslagern
        public void OnLook(InputValue value)
        {
            Vector2 lookDelta = value.Get<Vector2>();
            Debug.Log(lookDelta);
        }
        private Vector2 Movement = new();
        public void OnMove(InputValue value)
        {
            Movement = value.Get<Vector2>();
        }
        #endregion
    }
}
