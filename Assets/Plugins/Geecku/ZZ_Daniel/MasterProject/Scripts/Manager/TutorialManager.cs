using Geecku.GlobalMangers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
         * Controllers. Probier es mal aus!
         * Sehr gut! 
         * Möchtest du den letzten Satz wiederholen, drücke Quadrat, der linke knopf der
         * vier Knöpfe.
         * Momentan befindest du dich im Vorlesemodus, dass heißt, das es Text auf dem 
         * Bildschirm gibt, der vorgelesen wird. 
         * Im Vorlesemodus kannst du durch die verschiedenen Texte auf dem Bildschirm 
         * navigieren. Auf deiner linken Seite des Controllers hast du vier 
         * Knöpfe. Benutze den oberen und den unteren 
         * Knopf, um zu navigieren. Probiere es einmal aus!
         * Super! Sobald es Text auf dem Bildschirm erklingt dieser Ton und es wird automatisch der erste
         * Text vorgelesen 
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

        public TutorialSegment CurrentSegment;
        public DialogueContainer CurrentDialogue;
        private List<TutorialSegment> TutorialSegmentList = new();
        public List<DialogueContainer> DialogueList = new();
        private int SegmentIndex = 0;
        private int DialogueIndex = 0;
        public bool IsDialogue;
        public Transform SegmentSpawnExit;
        public Transform SegmentSpawnDoor;

        [SerializeField] private NPCScript StartNPC;
        [SerializeField] private NPCScript AfterTutorialNPC;

        #region MonoBehaviour commons
        protected override void Start()
        {
            base.Start();
            SetUpSegments();

            //- send NPC to you
            StartNPC.SetFollowing(true);

            //- Testing
            //StartTutorial();
        }
        public void UpdateTutorialManager(object sender, EventArgs e)
        {
            if (IsDialogue)
            {

            }
            else
            {
                if (CurrentSegment == null)
                    return;
                CurrentSegment.UpdateTutorialSegment();

                if (CurrentSegment.SegmentComplete)
                {
                    GameManager.Instance.UpdateEvent -= UpdateTutorialManager;
                    CurrentSegment.EndAction?.Invoke();
                }
            }
        }

        #endregion

        public void SetUpSegments()
        {
            //- Segment 5:
            TriggerTutorialSegment seg_5 = new();
            seg_5.StartAction = () =>
            {
                Debug.Log("start last semgent");
                GameManager.Instance.SetGameState(GameState.PLAYING);
                InputManager.Instance.PlayerInput.actions.FindActionMap("Tutorial").Enable();
                InputManager.Instance.PlayerInput.actions.FindActionMap("Player").Enable();
            };
            seg_5.EndAction = () =>
            {
                InputManager.Instance.PlayerInput.actions.FindActionMap("Tutorial").Disable();
                InputManager.Instance.PlayerInput.actions.FindActionMap("Player").Disable();
                //- opt ToDo "Filmsequenz" zuerst wie man durch Tür läuft und Tüt sich schließt
                ToggleStatus(true);
            };
            TutorialSegmentList.Add(seg_5);
        }
        public void ProcessButtonPress(TutorialButton button, bool is_one_time, bool start_press = true)
        {
            if (CurrentSegment is ButtonTutorialSegment tmp && tmp.RequiredButtonDic.ContainsKey(button))
            {
                if (is_one_time)
                    tmp.RequiredButtonDic[button].StartPressed(is_one_time);
                else if (start_press)
                    tmp.RequiredButtonDic[button].StartPressed(false);
                else
                    tmp.RequiredButtonDic[button].StopPressed();
            }
        }
        public void StartTutorial()
        {
            if (TutorialSegmentList.Count == 0 || DialogueList.Count == 0)
                return;

            GameManager.Instance.UpdateEvent += UpdateTutorialManager;
            ToggleStatus(true);
        }
        public void StartSegment(TutorialSegment segment)
        {
            if (segment == null) return;
            GameManager.Instance.UpdateEvent += UpdateTutorialManager;
            InputManager.Instance.PlayerInput.SwitchCurrentActionMap("Tutorial");
            SwitchSegment(segment);
        }
        public void EndSegment()
        {
            InputManager.Instance.PlayerInput.SwitchCurrentActionMap("UI");
            CurrentSegment = null;
        }
        public void SwitchSegment(TutorialSegment segment)
        {
            GameManager.Instance.UpdateEvent += UpdateTutorialManager;
            CurrentSegment = segment;
            CurrentSegment.OnStartSegment();
        }

        //- based on the assumption that we end the tutorial with a dialogue
        private void EndTutorial()
        {
            GameManager.Instance.UpdateEvent -= UpdateTutorialManager;
            GlobalUIManager.Instance.ToggleUI(UI_Group.DIALOGUE);
            GameManager.Instance.SetGameState(GameState.PLAYING);
            InputManager.Instance.PlayerInput.SwitchCurrentActionMap("Player");

            //- ToDo wait for other player but for now this is fine
            //- eigentlich auch dass man hier durch Tür läuft
            GameManager.Instance.Player.SpawnPlayerAtStart();
            GameManager.Instance.Player.TeleportNPCsToPlayer();

            //- NPC walks to you
            AfterTutorialNPC.SetFollowing(true);
        }
        public void ToggleStatus(bool switch_to_dialogue)
        {
            if (switch_to_dialogue)
            {             
                CurrentDialogue = DialogueList[DialogueIndex];
                DialogueIndex++;
                DialogueManager.Instance.StartTutorialDialogue(CurrentDialogue);
            }
            else
            {
                if (SegmentIndex == TutorialSegmentList.Count)
                {
                    EndTutorial();
                    return;
                }
                GlobalUIManager.Instance.ToggleUI(UI_Group.DIALOGUE);
                InputManager.Instance.PlayerInput.SwitchCurrentActionMap("Tutorial");
                CurrentSegment = TutorialSegmentList[SegmentIndex];
                CurrentSegment.OnStartSegment();
                SegmentIndex++;
            }
            IsDialogue = switch_to_dialogue;
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

        #region Public Trigger Functions
        public void TriggerSegmentComplete()
        {
            CurrentSegment.SegmentComplete = true;
        }
        #endregion
    }

    public class TutorialSegment
    {
        public bool SegmentComplete = false;
        public Action StartAction;
        public Action EndAction;

        public virtual void UpdateTutorialSegment() { }
        public virtual void OnStartSegment() 
        {
            StartAction?.Invoke();
        }
    }
    public class ButtonTutorialSegment : TutorialSegment
    {
        public Dictionary<TutorialButton, TutorialButtonInfo> RequiredButtonDic = new();
        public override void UpdateTutorialSegment()
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
    public class TriggerTutorialSegment : TutorialSegment
    {

    }
    public class TutorialButtonInfo
    {
        private const float HelpTime = 15f;
        public float RequiredPressTime;
        public string HelpAudioName;
        public float TimePassedWithNoPress;
        public bool Completed = false;
        public float CurrentPressTime = 0f;
        public bool IsBeingPressed;
        public bool SpeechOver = false;

        public TutorialButtonInfo(string help_audio_name, float required_press_time = 1f)
        {
            RequiredPressTime = required_press_time;
            HelpAudioName = help_audio_name;
            TimePassedWithNoPress = 0f;
        }
        public void CheckButtonStatus()
        {
            if (IsBeingPressed)
                CurrentPressTime += Time.deltaTime;
            else if (SpeechOver)
                TimePassedWithNoPress += Time.deltaTime;

            if (CurrentPressTime >= RequiredPressTime)
            {
                Completed = true;
            }
            else if (TimePassedWithNoPress >= HelpTime)
            {
                AudioManager.Instance.PlayDialogue2(HelpAudioName, null, Message_Tone.NONE);
                TimePassedWithNoPress = 0;
            }
        }
        public void StartPressed(bool is_one_time)
        {
            if (is_one_time)
                CurrentPressTime = RequiredPressTime;
            else
                IsBeingPressed = true;

            TimePassedWithNoPress = 0f;
        }
        public void StopPressed()
        {
            IsBeingPressed = false;
        }
    }
    public enum TutorialButton
    {
        Left_Joystick, Right_Joystick, Up_Arrow, Down_Arrow, X, Square, Right_Back
    }
}
