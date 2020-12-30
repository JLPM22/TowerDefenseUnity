using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public static SoundController Instance;

    public AudioClip BackgroundMusic;
    public SoundPair[] Sounds;

    private AudioClip[] Clips = new AudioClip[Enum.GetNames(typeof(Sound)).Length];
    private AudioSource AudioSource;

    private void Awake()
    {
        Instance = this;

        AudioSource = GetComponent<AudioSource>();
        for (int i = 0; i < Sounds.Length; ++i)
        {
            Clips[(int)Sounds[i].Sound] = Sounds[i].Clip;
        }
    }

    private void Start()
    {
        AudioSource.clip = BackgroundMusic;
        AudioSource.loop = true;
        AudioSource.Play();
    }

    public void PlaySoundDelay(Sound sound, float time, float scale = 1.0f)
    {
        StartCoroutine(PlayOneShotDelay(sound, time, scale));
    }

    private IEnumerator PlayOneShotDelay(Sound sound, float time, float scale)
    {
        yield return new WaitForSeconds(time);
        PlaySound(sound, scale);
    }

    public void PlaySound(Sound sound, float scale = 1.0f)
    {
        AudioSource.PlayOneShot(GetClip(sound), scale);
    }

    public void PlayUIClick()
    {
        PlaySound(Sound.UIPress, 2.0f);
    }

    public void PlayUIHover()
    {
        PlaySound(Sound.UIHighlight);
    }

    private AudioClip GetClip(Sound sound)
    {
        return Clips[(int)sound];
    }

    public enum Sound
    {
        Spawn,
        ImproveGold,
        SwordAttack,
        MagoAttack,
        Explosion,
        TowerDestroyed,
        UIHighlight,
        UIPress
    }

    [System.Serializable]
    public struct SoundPair
    {
        public Sound Sound;
        public AudioClip Clip;
    }
}
