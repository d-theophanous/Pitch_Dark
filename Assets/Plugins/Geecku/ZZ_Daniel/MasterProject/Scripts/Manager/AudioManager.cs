using FMOD;
using FMOD.Studio;
using FMODUnity;
using Geecku.GlobalMangers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Daniel.Master
{
    public class AudioManager : PersistantDSingleton<AudioManager>
    {
        #region Volume Control + Busses
        [Header("Volume")]
        [Range(0, 1)] public float MasterVolume = 1f;
        [Range(0, 1)] public float SFXVolume = 1f;
        [Range(0, 1)] public float ImprovisationTrackVolume = 1f;
        [Range(0, 1)] public float DialogueVolume = 1f;
        [Range(0, 1)] public float NotesVolume = 1f;
        [Range(0, 1)] public float TTSVolume = 1f;

        private Bus MasterBus, SFXBus, ImprovisationTrackBus, DialogueBus, NotesBus,
            TTSBus;
        #endregion

        private List<EventInstance> EventInstanceList;
        private EventInstance FootstepEvent;

        //- ToDo opt, for now all of the readable element narrations are in the 
        //- SFX bank which is not optimal
        //- auch problem dass narration of readable elements die ui sounds überschreibt
        protected override void Awake()
        {
            base.Awake();
            EventInstanceList = new();

            //- assign busses
            MasterBus = RuntimeManager.GetBus("bus:/");
            SFXBus = RuntimeManager.GetBus("bus:/SFX");
            ImprovisationTrackBus = RuntimeManager.GetBus("bus:/ImprovisationTrack");
            NotesBus = RuntimeManager.GetBus("bus:/Notes");
            DialogueBus = RuntimeManager.GetBus("bus:/Dialogue");
            TTSBus = RuntimeManager.GetBus("bus:/TTS");
        }
        protected override void Start()
        {
            base.Start();

            //- Dialogue Callback
            dialogueCallback = new EVENT_CALLBACK(DialogueEventCallback);

            //- Set up events
            SetUpEvents();

            //- Instrument at the start
            CurInstrument = Instrument.None;
            UnlockInstrument(Instrument.Vocals);
            SetInstrument((int)Instrument.Vocals);
        }
        public void UpdateAudio()
        {
            //- better only do when value changes?
            MasterBus.setVolume(MasterVolume);
            SFXBus.setVolume(SFXVolume);
            ImprovisationTrackBus.setVolume(ImprovisationTrackVolume);
            NotesBus.setVolume(NotesVolume);
            DialogueBus.setVolume(DialogueVolume);
            TTSBus.setVolume(TTSVolume);
        }
        private IEnumerator WaitForEnd(EventInstance instance, Action onComplete)
        {
            PLAYBACK_STATE state;
            do
            {
                yield return null; // wait one frame
                instance.getPlaybackState(out state);
            }
            while (state != PLAYBACK_STATE.STOPPED);

            onComplete?.Invoke();
        }

        #region Play Audio
        public void PlayOneShot(EventReference reference, Vector3 world_pos)
        {
            RuntimeManager.PlayOneShot(reference, world_pos);
        }
        public void PlayDialogue(string key, Action on_complete, Message_Tone tone = Message_Tone.CONTINUE)
        {
            StopDialogue();

            currentDialogueInstance.setParameterByNameWithLabel("Tone", tone.ToString());

            // Pin the key string in memory and pass a pointer through the user data
            GCHandle stringHandle = GCHandle.Alloc(key);
            currentDialogueInstance.setUserData(GCHandle.ToIntPtr(stringHandle));

            currentDialogueInstance.setCallback(dialogueCallback);
            currentDialogueInstance.start();
            if (on_complete != null)
                StartCoroutine(WaitForEnd(currentDialogueInstance, on_complete));
        }
        public bool ChangeLines;
        public void PlayDialogue(int dialogue, int line, Action on_complete, Message_Tone tone = Message_Tone.CONTINUE)
        {
            string key = Helper.GetLanguageString() + "_" + dialogue 
                + "_" + line;
            PlayDialogue(key, on_complete, tone);
        }
        public void PlayDialogue2(string key, Action on_complete, Message_Tone tone = Message_Tone.CONTINUE)
        {
            if (key == "")
                return;
            string tmp = Helper.GetLanguageString() + "_" + key;
            PlayDialogue(tmp, on_complete, tone);
        }
        public void StopDialogue()
        {
            if (currentDialogueInstance.isValid())
            {
                currentDialogueInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
        }

        #endregion

        public EventInstance CreateEventInstance(EventReference reference)
        {
            EventInstance instance = RuntimeManager.CreateInstance(reference);
            EventInstanceList.Add(instance);
            return instance;
        }
        private void SetUpEvents()
        {
            ImprovTrackEventInstance = CreateEventInstance(FMODEvents.Instance.ImprovisationTrack);
            NoteEventInstance = CreateEventInstance(FMODEvents.Instance.Note);
            currentDialogueInstance = CreateEventInstance(FMODEvents.Instance.Dialogue);
            WallScratchEvent = CreateEventInstance(FMODEvents.Instance.WallScratchEvent);
            WallFaceEvent = CreateEventInstance(FMODEvents.Instance.WallFaceEvent);
            FootstepEvent = CreateEventInstance(FMODEvents.Instance.FootstepEvent);
        }

        #region Accessibility
        public void PlayReadableElement(string key)
        {
            EventInstance instance = RuntimeManager.CreateInstance(FMODEvents.Instance.OneShotEvent);
            instance.setUserData(GCHandle.ToIntPtr(GCHandle.Alloc(key)));
            instance.start();
            instance.release();

            instance.setCallback(ProgrammerSoundCallback,
                EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND |
                EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND);
        }
        #endregion

        #region Footsteps
        public void PlayFootsteps()
        {
            FootstepEvent.start();
        }
        public void StopFootsteps()
        {
            FootstepEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        #endregion

        #region SFX and UI
        public void PlaySFX(SFX sfx, Action on_complete = null)
        {
            string key = GetStringFromEnum(sfx);
            EventInstance instance = RuntimeManager.CreateInstance(FMODEvents.Instance.OneShotEvent);
            instance.setUserData(GCHandle.ToIntPtr(GCHandle.Alloc(key)));
            instance.start();
            instance.release();

            instance.setCallback(ProgrammerSoundCallback,
                EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND |
                EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND);

            if (on_complete != null)
                StartCoroutine(WaitForEnd(instance, on_complete));
        }
        private string GetStringFromEnum(SFX sfx)
        {
            string tmp = "";
            switch (sfx)
            {
                case SFX.CORRECT:
                    tmp = "correct";
                    break;
                case SFX.GO_BACK:
                    tmp = "go_back_option";
                    break;
                case SFX.MAGNIFY:
                    tmp = "magnifier";
                    break;
                case SFX.SWITCH_ELEMENT:
                    tmp = "menu_switch_element";
                    break;
                case SFX.NO_MORE_ELEMENTS:
                    tmp = "no_more_elements";
                    break;
                case SFX.REPEAT_SOUND:
                    tmp = "repeat_sound";
                    break;
                case SFX.SELECT_OPTION:
                    tmp = "select_option";
                    break;
                case SFX.CLOSE_UI:
                    tmp = "ui_close";
                    break;
                case SFX.OPEN_UI:
                    tmp = "ui_open";
                    break;
                case SFX.WRONG:
                    tmp = "wrong";
                    break;
                case SFX.CLOSE_DOOR:
                    tmp = "close_door";
                    break;
                case SFX.OPEN_DOOR:
                    tmp = "open_door";
                    break;
                default:
                    break;
            }
            return tmp;
        }

        [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
        static FMOD.RESULT ProgrammerSoundCallback(EVENT_CALLBACK_TYPE type,
            IntPtr instPtr, IntPtr paramPtr)
        {
            EventInstance instance = new EventInstance(instPtr);
            instance.getUserData(out IntPtr userData);
            GCHandle handle = GCHandle.FromIntPtr(userData);
            string key = handle.Target as string;

            if (type == EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND)
            {
                var param = (PROGRAMMER_SOUND_PROPERTIES)
                    Marshal.PtrToStructure(paramPtr, typeof(PROGRAMMER_SOUND_PROPERTIES));
                RuntimeManager.StudioSystem.getSoundInfo(key, out SOUND_INFO info);
                RuntimeManager.CoreSystem.createSound(info.name_or_data,
                    info.mode, ref info.exinfo, out Sound sound);
                param.sound = sound.handle;
                param.subsoundIndex = info.subsoundindex;
                Marshal.StructureToPtr(param, paramPtr, false);
            }
            else if (type == EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND)
            {
                var param = (PROGRAMMER_SOUND_PROPERTIES)
                    Marshal.PtrToStructure(paramPtr, typeof(PROGRAMMER_SOUND_PROPERTIES));
                new Sound(param.sound).release();
                handle.Free();
            }
            return FMOD.RESULT.OK;
        }
        #endregion

        #region Wall Stuff
        //-ToDo opt start and stop methods
        private EventInstance WallScratchEvent;
        private EventInstance WallFaceEvent;
        public void StartWallScratch()
        {
            PLAYBACK_STATE tmp;
            WallScratchEvent.getPlaybackState(out tmp);
            if (tmp != PLAYBACK_STATE.STOPPED)
                return;
            WallScratchEvent.start();
        }
        public void StopWallScratch()
        {
            WallScratchEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        public void StartFaceWall()
        {
            PLAYBACK_STATE tmp;
            WallFaceEvent.getPlaybackState(out tmp);
            if (tmp != PLAYBACK_STATE.STOPPED)
                return;
            WallFaceEvent.start();
        }
        public void StopFaceWall()
        {
            WallFaceEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        #endregion

        #region Dialogue
        private FMOD.Studio.EventInstance currentDialogueInstance;
        private FMOD.Studio.EVENT_CALLBACK dialogueCallback;

        [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
        static FMOD.RESULT DialogueEventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
        {
            FMOD.Studio.EventInstance instance = new FMOD.Studio.EventInstance(instancePtr);

            // Retrieve the user data
            IntPtr stringPtr;
            instance.getUserData(out stringPtr);

            // Get the string object
            GCHandle stringHandle = GCHandle.FromIntPtr(stringPtr);
            String key = stringHandle.Target as String;

            switch (type)
            {
                case FMOD.Studio.EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND:
                    {
                        FMOD.MODE soundMode = FMOD.MODE.LOOP_NORMAL | FMOD.MODE.CREATECOMPRESSEDSAMPLE | FMOD.MODE.NONBLOCKING;
                        var parameter = (FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES));

                        if (key.Contains("."))
                        {
                            FMOD.Sound dialogueSound;
                            var soundResult = FMODUnity.RuntimeManager.CoreSystem.createSound(Application.streamingAssetsPath + "/" + key, soundMode, out dialogueSound);
                            if (soundResult == FMOD.RESULT.OK)
                            {
                                parameter.sound = dialogueSound.handle;
                                parameter.subsoundIndex = -1;
                                Marshal.StructureToPtr(parameter, parameterPtr, false);
                            }
                        }
                        else
                        {
                            FMOD.Studio.SOUND_INFO dialogueSoundInfo;
                            var keyResult = FMODUnity.RuntimeManager.StudioSystem.getSoundInfo(key, out dialogueSoundInfo);
                            if (keyResult != FMOD.RESULT.OK)
                            {
                                break;
                            }
                            FMOD.Sound dialogueSound;
                            var soundResult = FMODUnity.RuntimeManager.CoreSystem.createSound(dialogueSoundInfo.name_or_data, soundMode | dialogueSoundInfo.mode, ref dialogueSoundInfo.exinfo, out dialogueSound);
                            if (soundResult == FMOD.RESULT.OK)
                            {
                                parameter.sound = dialogueSound.handle;
                                parameter.subsoundIndex = dialogueSoundInfo.subsoundindex;
                                Marshal.StructureToPtr(parameter, parameterPtr, false);
                            }
                        }
                        break;
                    }
                case FMOD.Studio.EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND:
                    {
                        var parameter = (FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES));
                        var sound = new FMOD.Sound(parameter.sound);
                        sound.release();

                        break;
                    }
                case FMOD.Studio.EVENT_CALLBACK_TYPE.DESTROYED:
                    {
                        // Now the event has been destroyed, unpin the string memory so it can be garbage collected
                        stringHandle.Free();

                        break;
                    }
            }
            return FMOD.RESULT.OK;
        }
        #endregion

        #region Improvisation
        EventInstance ImprovTrackEventInstance;
        EventInstance NoteEventInstance;
        private List<Instrument> UnlockedInstrumentList = new();
        private Instrument CurInstrument;
        public void StartImprovisation()
        {
            ImprovTrackEventInstance.start();
            StartCoroutine(StartImprovisationCoroutine());
        }
        private IEnumerator StartImprovisationCoroutine()
        {
            PLAYBACK_STATE tmp;
            ImprovTrackEventInstance.getPlaybackState(out tmp);
            while (tmp != PLAYBACK_STATE.STOPPED)
            {
                yield return null;
                ImprovTrackEventInstance.getPlaybackState(out tmp);
            }
            DialogueManager.Instance.CleanUpDialogue();
            GlobalUIManager.Instance.ToggleUI(UI_Group.IMPROVISATION);
        }
        public void StopImprovisation()
        {
            ImprovTrackEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        public void PlayNote(Note note)
        {
            NoteEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            NoteEventInstance.setParameterByName("Note",(float)note);
            NoteEventInstance.start();
        }
        public void SetInstrument(int instrument)
        {
            Instrument tmp = (Instrument)instrument;
            if (!UnlockedInstrumentList.Contains(tmp))
                return;
            if (CurInstrument != Instrument.None)
            {
                ImprovTrackEventInstance.setParameterByName(CurInstrument.ToString(), 1f);
            }
            CurInstrument = tmp;
            NoteEventInstance.setParameterByNameWithLabel("Instrument", CurInstrument.ToString());
            ImprovTrackEventInstance.setParameterByName(CurInstrument.ToString(), 0f);
        }
        public void SetGenre(int genre)
        {
            ImprovTrackEventInstance.setParameterByNameWithLabel("Genre", ((Genre)genre).ToString());
        }
        public void SetInitialGenre(int genre)
        {
            SetGenre(genre);
            GlobalUIManager.Instance.ToggleUI(UI_Group.NETWORK_CONNECT, false);
        }
        public void UnlockInstrument(Instrument instrument)
        {
            //- unlock and add to track
            UnlockedInstrumentList.Add(instrument);
            ImprovTrackEventInstance.setParameterByName(instrument.ToString(), 1f);
        }
        public void SwitchInstrument(int change)
        {
            int cur_instr_idx = 0;
            foreach (Instrument instrument in UnlockedInstrumentList)
            {
                if (instrument == CurInstrument)
                    break;
                cur_instr_idx++;
            }
           
            SetInstrument((int)UnlockedInstrumentList[
                Helper.GetLoopingIndex(UnlockedInstrumentList, (cur_instr_idx + change))]);
        }
        #endregion

        #region CleanUp
        private void CleanUp()
        {
            foreach (var instance in EventInstanceList)
            {
                instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                instance.release();
            }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            CleanUp();
        }
        #endregion

    }
    //- ToDo change according to what we end up with
    public enum Instrument
    {
        Piano, Violin, Guitar, Vocals, None
    }
    public enum Genre
    {
        EDM, HipHop, Pop, Jazz, Mystic
    }
    public enum Note
    {
        LOW_C, LOW_E, LOW_G, HIGH_C
    }
    public enum Message_Tone
    {
        CONTINUE, NONE, NOTES_EXAMPLE, HARMONIC_INTERVAL, TENSION_INTERVAL, PRIME, OCTAVE,
        BUTTON, CLOSE_INTERVAL, FAR_INTERVAL, FIFTH, THIRD, WAIT
    }
    public enum SFX
    {
        CORRECT, GO_BACK, MAGNIFY, SWITCH_ELEMENT, NO_MORE_ELEMENTS, REPEAT_SOUND,
        SELECT_OPTION, CLOSE_UI, OPEN_UI, WRONG, CLOSE_DOOR, OPEN_DOOR
    }
}
