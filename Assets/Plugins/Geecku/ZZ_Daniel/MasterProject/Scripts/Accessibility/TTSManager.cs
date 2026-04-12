using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Daniel.Master
{
    public class TTSManager : MonoBehaviour
    {
        // ── Singleton ──────────────────────────────────────────────────────
        public static TTSManager Instance { get; private set; }

        // ── Inspector ──────────────────────────────────────────────────────
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;

        [Header("Default language (en / nl / de)")]
        [SerializeField] private string defaultLanguage = "en";

        [Header("eSpeak-NG settings")]
        [SerializeField] private int wordsPerMinute = 175;
        [SerializeField] private int pitch = 50;

        // ── eSpeak-NG native API ───────────────────────────────────────────
        private const string ESPEAK_LIB = "libespeak-ng";

        // Callback delegate that eSpeak calls with audio samples.
        // Return 0 to continue synthesis, 1 to abort.
        private delegate int SynthCallbackDelegate(
            IntPtr wav,       // pointer to short[] audio samples (IntPtr.Zero = end of utterance)
            int numSamples,   // number of samples (0 = end of utterance)
            IntPtr events);   // espeak_EVENT array (unused here)

        [DllImport(ESPEAK_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern int espeak_Initialize(
            int output,       // AUDIO_OUTPUT_RETRIEVAL (1) = deliver samples via callback
            int buflength,    // buffer length ms (0 = default ~200ms)
            string path,      // path to espeak-ng-data folder
            int options);     // 0

        [DllImport(ESPEAK_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern void espeak_SetSynthCallback(SynthCallbackDelegate callback);

        [DllImport(ESPEAK_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern int espeak_SetVoiceByName(string name);

        [DllImport(ESPEAK_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern int espeak_SetParameter(int parameter, int value, int relative);

        // Synthesise text — audio arrives via the registered callback
        [DllImport(ESPEAK_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern int espeak_Synth(
            string text,
            uint size,              // 0 = let eSpeak calculate
            uint position,          // 0 = start of text
            int positionType,       // 0 = POS_CHARACTER
            uint endPosition,       // 0 = end of text
            uint flags,             // 0 = espeakCHARS_AUTO
            IntPtr uniqueIdentifier,
            IntPtr userData);

        // Block until all queued audio has been delivered via the callback
        [DllImport(ESPEAK_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern int espeak_Synchronize();

        [DllImport(ESPEAK_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern int espeak_Terminate();

        private const int AUDIO_OUTPUT_RETRIEVAL = 1;
        private const int espeakRATE = 1;
        private const int espeakPITCH = 4;

        // ── State ──────────────────────────────────────────────────────────
        private string currentLanguage;
        private bool eSpeakAvailable = false;
        private int sampleRate = 22050;

        private readonly List<short> sampleBuffer = new List<short>();
        private bool synthComplete = false;

        // Hold a strong reference to the delegate so the GC never collects it
        // while eSpeak is calling it from a native thread
        private SynthCallbackDelegate synthCallbackRef;

        private readonly Dictionary<string, string> voiceMap = new Dictionary<string, string>
        {
            { "en", "en" },
            { "nl", "nl" },
            { "de", "de" },
        };

        // ── Unity lifecycle ────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            currentLanguage = PlayerPrefs.GetString("TTSLanguage", defaultLanguage);
            InitializeESpeak();
        }

        private void OnDestroy()
        {
            if (eSpeakAvailable)
            {
                try { espeak_Terminate(); }
                catch { /* ignore on shutdown */ }
            }
        }

        // ── Public API ─────────────────────────────────────────────────────

        public void SetLanguage(string languageCode)
        {
            if (!voiceMap.ContainsKey(languageCode))
            {
                Debug.LogWarning($"[TTSManager] Unknown language '{languageCode}'. Supported: en, nl, de.");
                return;
            }

            currentLanguage = languageCode;
            PlayerPrefs.SetString("TTSLanguage", languageCode);
            PlayerPrefs.Save();

            if (eSpeakAvailable)
                ApplyVoice(languageCode);

            Debug.Log($"[TTSManager] Language set to '{languageCode}'.");
        }

        public void Speak(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            StopAllCoroutines();
            if (audioSource.isPlaying) audioSource.Stop();
            StartCoroutine(SpeakCoroutine(text));
        }

        public void Stop()
        {
            StopAllCoroutines();
            if (audioSource.isPlaying) audioSource.Stop();
        }

        // ── Initialisation ─────────────────────────────────────────────────

        private void InitializeESpeak()
        {
            string dataPath = System.IO.Path.Combine(
                Application.streamingAssetsPath, "espeak-ng-data");

            try
            {
                int result = espeak_Initialize(AUDIO_OUTPUT_RETRIEVAL, 0, dataPath, 0);
                if (result < 0)
                {
                    Debug.LogWarning("[TTSManager] espeak_Initialize failed (returned " +
                                     result + "). Check that espeak-ng-data is in StreamingAssets.");
                    return;
                }

                sampleRate = result; // espeak_Initialize returns the sample rate on success

                // Store delegate in a field — critical to prevent GC collection
                synthCallbackRef = OnSynthCallback;
                espeak_SetSynthCallback(synthCallbackRef);

                espeak_SetParameter(espeakRATE, wordsPerMinute, 0);
                espeak_SetParameter(espeakPITCH, pitch, 0);
                ApplyVoice(currentLanguage);

                eSpeakAvailable = true;
                Debug.Log($"[TTSManager] eSpeak-NG initialised. Sample rate: {sampleRate} Hz. " +
                          $"Language: {currentLanguage}");
            }
            catch (DllNotFoundException e)
            {
                Debug.LogWarning($"[TTSManager] libespeak-ng not found: {e.Message}\n" +
                                 "Place libespeak-ng.dll/.dylib/.so in Assets/Plugins/.");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[TTSManager] Unexpected error during eSpeak init: {e.Message}");
            }
        }

        private void ApplyVoice(string languageCode)
        {
            string voice = voiceMap.ContainsKey(languageCode) ? voiceMap[languageCode] : "en";
            int r = espeak_SetVoiceByName(voice);
            if (r != 0)
                Debug.LogWarning($"[TTSManager] espeak_SetVoiceByName('{voice}') returned {r}.");
        }

        // ── Synth callback — called by eSpeak on its internal thread ───────

        private int OnSynthCallback(IntPtr wav, int numSamples, IntPtr events)
        {
            if (wav == IntPtr.Zero || numSamples <= 0)
            {
                // End of utterance signal
                synthComplete = true;
                return 0;
            }

            short[] chunk = new short[numSamples];
            Marshal.Copy(wav, chunk, 0, numSamples);

            lock (sampleBuffer)
            {
                sampleBuffer.AddRange(chunk);
            }

            return 0; // return 1 to abort synthesis early
        }

        // ── Speech coroutine — runs on the Unity main thread ───────────────

        private IEnumerator SpeakCoroutine(string text)
        {
            if (!eSpeakAvailable)
            {
                Debug.LogWarning("[TTSManager] eSpeak-NG is not available.");
                yield break;
            }

            // Clear previous audio
            lock (sampleBuffer) { sampleBuffer.Clear(); }
            synthComplete = false;

            // Start synthesis — non-blocking, samples arrive via OnSynthCallback
            espeak_Synth(text, 0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero);

            // espeak_Synchronize() blocks until done, so run it off the main thread
            bool syncDone = false;
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                espeak_Synchronize();
                syncDone = true;
            });

            // Yield until both the synchronize call and the final callback have fired
            yield return new WaitUntil(() => syncDone && synthComplete);

            // Convert the 16-bit PCM buffer to Unity float samples
            short[] allSamples;
            lock (sampleBuffer) { allSamples = sampleBuffer.ToArray(); }

            if (allSamples.Length == 0)
            {
                Debug.LogWarning("[TTSManager] eSpeak returned no audio samples.");
                yield break;
            }

            float[] floatSamples = new float[allSamples.Length];
            for (int i = 0; i < allSamples.Length; i++)
                floatSamples[i] = allSamples[i] / 32768f; // normalise short → [-1, 1] float

            AudioClip clip = AudioClip.Create(
                name: "tts_clip",
                lengthSamples: floatSamples.Length,
                channels: 1,
                frequency: sampleRate,
                stream: false);

            clip.SetData(floatSamples, 0);
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}