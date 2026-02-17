using UnityEngine;

namespace Core
{
    public class SFXManager : MonoBehaviour
    {
        public static SFXManager I;

        [Header("Audio Source")]
        public AudioSource source;

        [Header("Clips")]
        public AudioClip cardThrow;
        public AudioClip trickWin;
        public AudioClip roundEnd;
        public AudioClip finalResult;
        public AudioClip buttonClick;

        [Header("Volume")]
        [Range(0f, 1f)] public float masterVolume = 0.8f;

        void Awake()
        {
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }

            I = this;
            DontDestroyOnLoad(gameObject); // 👈 VERY IMPORTANT
        }


        public void Play(AudioClip clip, float volume = 1f)
        {
            if (!clip || !source) return;
            source.PlayOneShot(clip, volume * masterVolume);
        }

        public void PlayCardThrow() => Play(cardThrow, 0.9f);
        public void PlayTrickWin() => Play(trickWin, 1f);
        public void PlayRoundEnd() => Play(roundEnd, 1f);
        public void PlayFinal() => Play(finalResult, 1f);
        public void PlayButton() => Play(buttonClick, 0.8f);
    }
}