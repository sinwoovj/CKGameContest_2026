using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Shurub
{
    public enum SoundType
    {
        BGM,
        SFX
    }

    [Serializable]
    public class Sound
    {
        public string Name;
        public SoundType Type;
        public AudioClip Clip;
        public bool Loop;

        [Range(0f, 1f)] public float Volume = 1f;
    }

    public class SoundManager : SingletonPun<SoundManager>
    {
        [SerializeField] private AudioMixer audioMixer;

        [SerializeField] private AudioSource bgmAudioSource;
        [SerializeField] private AudioSource sfxAudioSource;

        [SerializeField] private Sound[] sounds;

        private Dictionary<string, Sound> soundDictionary = new Dictionary<string, Sound>();

        protected override void OnAwake()
        {
            for (int i = 0; i < sounds.Length; i++)
            {
                soundDictionary[sounds[i].Name] = sounds[i];
            }
        }

        public void Play(string name)
        {
            if (!soundDictionary.TryGetValue(name, out Sound sound))
            {
                Debug.LogError($"{name} is not contains in soundDictionary");
                return;
            }

            switch (sound.Type)
            {
                case SoundType.BGM:
                    bgmAudioSource.clip = sound.Clip;
                    bgmAudioSource.loop = sound.Loop;
                    bgmAudioSource.volume = sound.Volume;
                    bgmAudioSource.Play();
                    break;
                case SoundType.SFX:
                    sfxAudioSource.volume = sound.Volume;
                    sfxAudioSource.PlayOneShot(soundDictionary[name].Clip);
                    break;
                default:
                    Debug.LogError($"unknown type: {sound.Type}");
                    break;
            }
        }

        public void PauseBGM()
        {
            bgmAudioSource.Pause();
        }

        public void UnPauseBGM()
        {
            bgmAudioSource.UnPause();
        }

        public void StopBGM()
        {
            bgmAudioSource.Stop();
        }

        public void PauseSFX()
        {
            sfxAudioSource.Pause();
        }

        public void UnPauseSFX()
        {
            sfxAudioSource.UnPause();
        }

        public void StopSFX()
        {
            sfxAudioSource.Stop();
        }

        /// <summary>
        /// 볼륨 조절
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value">0 ~ 1 사이</param>
        public void SetAudioMixerVolume(SoundType type, float value)
        {
            //float volume = Mathf.Log10(Mathf.Max(0.0001f, value)) * 20f;
            float volume = (value * 10f) * 8f - 80f;
            string parameterName = type.ToString();

            if (!audioMixer.SetFloat(parameterName, volume))
            {
                Debug.LogWarning($"connot found '{parameterName}'.");
            }
        }
    }
}
