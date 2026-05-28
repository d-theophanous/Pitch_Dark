using Geecku.GlobalMangers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Composites;

namespace Daniel.Master
{
    public class TutorialManager : Singleton<TutorialManager>
    {
        /*
         * Also:
         * Bevor du mit dem Spiel starten kannst, stellen wir sicher, dass du mit
         * der Steuerung vertraut bist. 
         * Wenn du diesen Ton hörst <Ton> heißt dass das du am Ende des aktuellen 
         * Satzes bist. Du kannst den nächsten Satz abspielen indem du X drückst. 
         * Der X knopf ist der untere der vier Knöpfe auf der rechten Seite deines 
         * Controllers. 
         * Sehr gut! 
         * Möchtest du den letzten Satz wiederholen, drücke Quadrat, der linke knopf der
         * vier Knöpfe.
         * Momentan befindest du dich im Vorlesemodus, dass heißt, das es Text auf dem 
         * Bildschirm gibt, der vorgelesen wird. 
         * Im Vorlesemodus kannst du durch die verschiedenen Texte auf dem Bildschirm 
         * navigieren. Auf deiner linken Seite des Controllers hast du vier 
         * Knöpfe. Benutze den oberen und den unteren 
         * Knopf, um zu navigieren. Probiere es einmal aus!
         * Super! Immer wenn es Text auf dem Bildschirm gibt wird automatisch der erste
         * Text vorgelesen und dieser Ton erklingt: 
         * Befindest du dich wieder im normalen Spielemodus erklingt dieser Sound:
         * 
         * Weiter mit der Steuerung deines Charakters: 
         * Mit dem  linken Joystick bewegst du deinen Charakter. Bewegest du den Joystick
         * nach links bewegt sich dein Charakter nach links.
         * Mit dem rechten Joystick kannst du ändern, in welche Richtung der Charakter guckt
         * Bewegst du ihn nach rechts dreht sich dein Charakter 90° nach rechts. 
         * Probiere es einmal aus!
         * 
         * Super! Damit du auch in geschlossenen Räumen weißt, wo du bist gibt es 
         * drei Arten von Sounds. 
         * Wenn du gegen eine Wand läufst hörst du diesen Sound:
         * Wenn du an einer Wand entlangläufst hörst du diesen SOund:
         * Wenn links oder rechts von dir einen Gang gibt, hörst du ein Windrauschen 
         * in deinem linken oder rechten Ohr: 
         * Du befindest dich jetzt in einem Raum mit einem AUsgang. Versuche den Ausgang 
         * zu finden.
         * 
         * Super, eine letzte Sache noch: wenn du in der Nähe von einem Objekt bist,
         * mit dem du interagieren kannst ertönt dieser Sound: 
         * Drücke den hinteren Rechten Knopf auf der rechten Seite um mit dem Objekt
         * zu interargieren.
         * 
         * Du befindest dich in einem Raum. Finde die Tür mit der du interargieren kannst,
         * und drück dann den hintern rechten Knopf. 
         * 
         * Brauche irgendwas das: sagt welcher Knopf gedrückt werden muss
         * wie lange
         * was passiert wenn nicht gedrückt wird
         * Liste mit: Knopf der gedrückt werden soll, wie lange, wie lange warten bis 
         * speech, was  für ne speech
         */

        private TutorialSegment CurrentSegment;
        private List<TutorialSegment> TutorialSegmentList = new();
        private int SegmentIndex = 0;

        #region MonoBehaviour commons
        protected override void Start()
        {
            base.Start();
            SetUpSegments();
        }
        public void UpdateTutorialManager(object sender, EventArgs e)
        {
            if (CurrentSegment == null)
                return;
            CurrentSegment.UpdateTutorialSegment();

            if (CurrentSegment.SegmentComplete)
                Debug.Log("Next segment");
        }

        #endregion
        public void SetUpSegments()
        {
            TutorialSegment seg_1 = new TutorialSegment();
            seg_1.RequiredButtonDic.Add(TutorialButton.X,
                new TutorialButtonInfo(""));

            TutorialSegmentList.Add(seg_1);
        }
        public void ProcessButtonPress(TutorialButton button, bool one_time)
        {
            if (CurrentSegment.RequiredButtonDic.ContainsKey(button))
            {
                if (one_time)
                    CurrentSegment.RequiredButtonDic[button].PressedOnce();
                else
                    CurrentSegment.RequiredButtonDic[button].Pressed();
            }
        }

        public void StartTutorial()
        {
            if (TutorialSegmentList.Count == 0)
                return;
            CurrentSegment = TutorialSegmentList[SegmentIndex];
            SegmentIndex++;
            GameManager.Instance.UpdateEvent += UpdateTutorialManager;
        }


        public void SkipTutorial()
        {

        }
        public void PauseTutorial()
        {

        }
        public void UnpauseTutorial()
        {

        }
    }

    public class TutorialSegment
    {
        public Dictionary<TutorialButton, TutorialButtonInfo> RequiredButtonDic;
        public bool SegmentComplete = false;

        public void UpdateTutorialSegment()
        {
            bool is_complete = true;
            foreach (var button in RequiredButtonDic)
            {
                button.Value.CheckButtonStatus();
                if (!button.Value.Completed)
                    is_complete = false;
            }
            if (is_complete)
            {
                SegmentComplete = true;
            }
        }
    }
    public class TutorialButtonInfo
    {
        private const float HelpTime = 10f;
        public float RequiredPressTime;
        public string HelpAudioName;
        public float TimePassedWithNoPress;
        public bool Completed = false;
        public float CurrentPressTime = 0f;

        public TutorialButtonInfo(string help_audio_name, float required_press_time = 1f)
        {
            RequiredPressTime = required_press_time;
            HelpAudioName = help_audio_name;
            TimePassedWithNoPress = 0f;
        }
        public void CheckButtonStatus()
        {
            if (TimePassedWithNoPress >= HelpTime)
            {
                Debug.Log("Play help info");
                TimePassedWithNoPress = 0;
            }
            if (CurrentPressTime >= RequiredPressTime)
            {
                Completed = true;
            }
        }
        public void PressedOnce()
        {
            CurrentPressTime = RequiredPressTime;
            TimePassedWithNoPress = 0f;
        }
        public void Pressed()
        {
            CurrentPressTime += Time.deltaTime;
            TimePassedWithNoPress = 0f;
        }
    }
    public enum TutorialButton
    {
        Left_Joystick, Right_Joystick, Up_Arrow, Down_Arrow, X, Square, Right_Back
    }
}
