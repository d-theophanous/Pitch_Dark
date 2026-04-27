using FMOD.Studio;
using FMODUnity;
using Geecku.GlobalMangers;
using System;
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
            currentDialogueInstance = CreateEventInstance(FMODEvents.Instance.Dialogue);

            SetUpEvents();
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
        #region Play Audio
        public void PlayOneShot(EventReference reference, Vector3 world_pos)
        {
            RuntimeManager.PlayOneShot(reference, world_pos);
        }
        public void PlayDialogue(string key)
        {
            StopDialogue();

            currentDialogueInstance.setParameterByNameWithLabel("Tone", "Continue");

            // Pin the key string in memory and pass a pointer through the user data
            GCHandle stringHandle = GCHandle.Alloc(key);
            currentDialogueInstance.setUserData(GCHandle.ToIntPtr(stringHandle));

            currentDialogueInstance.setCallback(dialogueCallback);
            currentDialogueInstance.start();
        }
        public void PlayDialogue(int dialogue, int line)
        {
            string key = Helper.GetLanguageString() + "_" + dialogue 
                + "_" + line;
            Debug.Log(key);
            PlayDialogue(key);
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
        private void SetUpEvents()
        {
            ImprovTrackEventInstance = CreateEventInstance(FMODEvents.Instance.ImprovisationTrack);
            NoteEventInstance = CreateEventInstance(FMODEvents.Instance.Note);
        }
        public void StartImprovisation()
        {
            ImprovTrackEventInstance.start();
        }
        public void StopImprovisation()
        {
            ImprovTrackEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        public void PlayNote(Note note)
        {
            NoteEventInstance.setParameterByName("Note", (float)note);
            NoteEventInstance.start();
        }
        public void SetInstrument(Instrument instrument)
        {
            //- can cause layer problems right now
            NoteEventInstance.setParameterByName("Instrument", (float)instrument);
        }
        public void SetGenre(Genre genre)
        {
            ImprovTrackEventInstance.setParameterByName("Genre", (float)genre);
        }
        public void SetInstrumentCount(int count)
        {
            ImprovTrackEventInstance.setParameterByName("LayerController", (float)count);
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
        PIANO, SAXOPHONE, VIOLIN, GUITAR, DRUMS
    }
    public enum Genre
    {
        UPRIGHT, BOSSANOVA
    }
    public enum Note
    {
        LOW_C, LOW_E, LOW_G, HIGH_C
    }
    public enum Message_Tone
    {
        CONTINUE
    }
}
