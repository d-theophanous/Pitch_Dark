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
        private bool isMotorActive = false;
        private Gamepad Gamepad;
        private PlayerInput PlayerInput;

        protected override void Awake()
        {
            base.Awake();
            PlayerInput = InputManager.Instance.PlayerInput;
            Gamepad = GetGamepad();
        }
        public void RumbleConstant(float low, float high, float duration)
        {
            StartRumble(duration);
            Gamepad.SetMotorSpeeds(low, high);

        }
        private IEnumerator StartRumbleCo(float duration)
        {
            yield return new WaitForSeconds(duration);
            StopRumble();
        }
        private void StartRumble(float duration)
        {
            StopAllCoroutines();
            StartCoroutine(StartRumbleCo(duration));
        }

        //public void RumblePulse(float low, float high, float burstTime, float durration)
        //{
        //    activeRumbePattern = RumblePattern.Pulse;
        //    lowA = low;
        //    highA = high;
        //    rumbleStep = burstTime;
        //    pulseDurration = Time.time + burstTime;
        //    rumbleDurration = Time.time + durration;
        //    isMotorActive = true;
        //    var g = GetGamepad();
        //    g?.SetMotorSpeeds(lowA, highA);
        //}

        //public void RumbleLinear(float lowStart, float lowEnd, float highStart, float highEnd, float durration)
        //{
        //    activeRumbePattern = RumblePattern.Linear;
        //    lowA = lowStart;
        //    highA = highStart;
        //    lowStep = (lowEnd - lowStart) / durration;
        //    highStep = (highEnd - highStart) / durration;
        //    rumbleDurration = Time.time + durration;
        //}

        public void StopRumble()
        {
            if (Gamepad != null)
            {
                Gamepad.SetMotorSpeeds(0, 0);
            }
        }

        //        private void Update()
        //{
        //    if (Time.time > rumbleDurration)
        //    {
        //        StopRumble();
        //        return;
        //    }

        //    var gamepad = GetGamepad();
        //    if (gamepad == null)
        //        return;

        //    switch (activeRumbePattern)
        //    {
        //        case RumblePattern.Constant:
        //            gamepad.SetMotorSpeeds(lowA, highA);
        //            break;

        //        case RumblePattern.Pulse:

        //            if (Time.time > pulseDurration)
        //            {
        //                isMotorActive = !isMotorActive;
        //                pulseDurration = Time.time + rumbleStep;
        //                if (!isMotorActive)
        //                {
        //                    gamepad.SetMotorSpeeds(0, 0);
        //                }
        //                else
        //                {
        //                    gamepad.SetMotorSpeeds(lowA, highA);
        //                }
        //            }

        //            break;
        //        case RumblePattern.Linear:
        //            gamepad.SetMotorSpeeds(lowA, highA);
        //            lowA += (lowStep * Time.deltaTime);
        //            highA += (highStep * Time.deltaTime);
        //            break;
        //        default:
        //            break;
        //    }
        //}

        protected override void OnDestroy()
        {
            base.OnDestroy();
            StopAllCoroutines();
            StopRumble();
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