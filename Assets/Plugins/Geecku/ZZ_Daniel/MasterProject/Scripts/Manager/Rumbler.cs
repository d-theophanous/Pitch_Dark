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
        private Gamepad Gamepad => GetGamepad();
        private PlayerInput PlayerInput;
        public (float, float) DefaultRumbleFrequency = (0.9f, 0.9f);

        protected override void Awake()
        {
            base.Awake();
            PlayerInput = InputManager.Instance.PlayerInput;
        }
        protected override void Update()
        {
            base.Update();

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
            StopRumble();
        }
        private IEnumerator RumbleEndless()
        {
            Gamepad.SetMotorSpeeds(DefaultRumbleFrequency.Item1, DefaultRumbleFrequency.Item2);
            while (IsRumbling)
                yield return null;
            StopRumble();
        }
        public void StartRumble(float duration)
        {
            if (IsRumbling || Gamepad == null)
                return;
            IsRumbling = true;
            StopAllCoroutines();
            StartCoroutine(RumbleDuration(duration));
        }
        public void StartRumble()
        {
            Debug.Log("start rumble");
            if (IsRumbling || Gamepad == null)
                return;
            Debug.Log("rumbling fr");
            IsRumbling = true;
            StopAllCoroutines();
            StartCoroutine(RumbleEndless());
        }
        public void StopRumble()
        {
            IsRumbling = false;
            if (Gamepad != null)
            {
                Gamepad.SetMotorSpeeds(0, 0);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            StopAllCoroutines();
            StopRumble();
        }

        // Private helpers
        private Gamepad GetGamepad()
        {
            Debug.Log($"Gamepad.all count: {Gamepad.all.Count}");
            Debug.Log($"PlayerInput.devices count: {PlayerInput.devices.Count}");
            foreach (var d in PlayerInput.devices)
                Debug.Log($"PlayerInput device: {d.name}, id: {d.deviceId}");
            foreach (var g in Gamepad.all)
                Debug.Log($"Gamepad.all entry: {g.name}, id: {g.deviceId}");

            var fromPlayerInput = Gamepad.all.FirstOrDefault(g =>
                PlayerInput.devices.Any(d => d.deviceId == g.deviceId));

            Debug.Log($"fromPlayerInput: {fromPlayerInput}, Gamepad.current: {Gamepad.current}");

            return fromPlayerInput ?? Gamepad.current;
        }
    }
    }