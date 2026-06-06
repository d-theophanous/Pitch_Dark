using Geecku.GlobalMangers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Daniel.Master
{

public enum RumblePattern
{
    Constant,
    Pulse,
    Linear
}

    public class Rumbler : PersistantDSingleton<Rumbler>
    {
        //private float rumbleDurration;
        //private float pulseDurration;
        //private float lowA;
        //private float lowStep;
        //private float highA;
        //private float highStep;
        //private float rumbleStep;
        private bool IsRumbling;
        private Gamepad Gamepad;
        private PlayerInput PlayerInput;
        public (float, float) DefaultRumbleFrequency = (0.1f, 0.1f);

        protected override void Awake()
        {
            base.Awake();
            PlayerInput = InputManager.Instance.PlayerInput;
            Gamepad = GetGamepad();
        }
        public void RumbleConstant(float low, float high, float duration)
        {
            if (Gamepad == null || IsRumbling)
                return;
            StartRumble(duration);
            Gamepad.SetMotorSpeeds(low, high);

        }
        private IEnumerator RumbleDuration(float duration)
        {
            Gamepad.SetMotorSpeeds(DefaultRumbleFrequency.Item1, DefaultRumbleFrequency.Item2);
            yield return new WaitForSeconds(duration);
            ResetRumble();
        }
        private IEnumerator RumbleEndless()
        {
            Gamepad.SetMotorSpeeds(DefaultRumbleFrequency.Item1, DefaultRumbleFrequency.Item2);
            while (IsRumbling)
                yield return null;
            ResetRumble();
        }
        public void StartRumble(float duration)
        {
            if (IsRumbling)
                return;
            IsRumbling = true;
            StopAllCoroutines();
            StartCoroutine(RumbleDuration(duration));
        }
        public void StartRumble()
        {
            if (IsRumbling)
                return;
            IsRumbling = true;
            StopAllCoroutines();
            StartCoroutine(RumbleEndless());
        }
        private void ResetRumble()
        {
            if (Gamepad != null)
            {
                Gamepad.SetMotorSpeeds(0, 0);
                IsRumbling = false;
            }
        }

        public void StopRumble()
        {
            IsRumbling = false;
            Gamepad.SetMotorSpeeds(0, 0);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            StopAllCoroutines();
            ResetRumble();
        }

        // Private helpers

        private Gamepad GetGamepad()
        {
            return Gamepad.all.FirstOrDefault(g => PlayerInput.devices.Any(d => d.deviceId == g.deviceId));

            #region Linq Query Equivalent Logic
            //Gamepad gamepad = null;
            //foreach (var g in Gamepad.all)
            //{
            //    foreach (var d in _playerInput.devices)
            //    {
            //        if(d.deviceId == g.deviceId)
            //        {
            //            gamepad = g;
            //            break;
            //        }
            //    }
            //    if(gamepad != null)
            //    {
            //        break;
            //    }
            //}
            //return gamepad;
            #endregion
        }
}
    }