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

        [Header("Input Actions")]
        public InputActionReference moveAction;

        private void OnEnable()
        {
            moveAction.action.Enable();
        }

        private void OnDisable()
        {
            moveAction.action.Disable();
        }

        void Update()
        {
            // Read input
            Vector2 input = moveAction.action.ReadValue<Vector2>();
            Vector3 move = new Vector3(input.x, 0, input.y);
            move = Vector3.ClampMagnitude(move, 1f);

            if (move != Vector3.zero)
                transform.forward = move;

            // Move
            //- ToDo
            //Vector2 movement =  
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
        public void OnMove(InputValue value)
        {

        }
        #endregion
    }
}
