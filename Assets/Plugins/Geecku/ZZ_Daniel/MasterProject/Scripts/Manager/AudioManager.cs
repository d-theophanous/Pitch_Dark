using FMODUnity;
using Geecku.GlobalMangers;
using UnityEngine;
using FMOD.Studio;
using System.Collections.Generic;
using Unity.VisualScripting;

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
        public void PlayOneShot(EventReference reference, Vector3 world_pos)
        {
            RuntimeManager.PlayOneShot(reference, world_pos);
        }
        
        public EventInstance CreateEventInstance(EventReference reference)
        {
            EventInstance instance = RuntimeManager.CreateInstance(reference);
            EventInstanceList.Add(instance);
            return instance;
        }

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
}
