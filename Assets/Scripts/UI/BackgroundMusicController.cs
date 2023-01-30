using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BackgroundMusicController : MonoBehaviour
{
    public static AudioClip menuMusic;
    public static AudioClip runMusic;
    public static AudioClip bossMusic;
    private static GameObject go;
    private static BackgroundMusicController _instance;
    public static BackgroundMusicController instance
    {
        get
        {
            if (_instance == null)
            {
                go = new GameObject();
                go.name = "BackgroundMusic";
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<BackgroundMusicController>();
                _audioSource1 = go.AddComponent<AudioSource>();
                _audioSource2 = go.AddComponent<AudioSource>();

                menuMusic = Resources.Load<AudioClip>("Sounds/Music/MenuMusic");
                runMusic = Resources.Load<AudioClip>("Sounds/Music/RunMusic1");
                bossMusic = Resources.Load<AudioClip>("Sounds/Music/BossMusic1");

                _audioSource1.loop = true;
                _audioSource2.loop = true;

                _firstSourceActive = true;
                _maxVolume = .5f * PlayerPrefs.GetFloat(Constants.MusicVolumePlayerPrefKey, 1f);
            }
            return _instance;
        }
    }

    private static AudioSource _audioSource1;
    private static AudioSource _audioSource2;
    private static bool _firstSourceActive;
    private static float _maxVolume;

    public void UpdateVolume()
    {
        _maxVolume = .5f * PlayerPrefs.GetFloat(Constants.MusicVolumePlayerPrefKey, 1f);
        var activeAudioSource = _firstSourceActive
            ? _audioSource1
            : _audioSource2;

        activeAudioSource.volume = _maxVolume;
    }

    public void SwitchToMusic(MusicType musicType)
    {
        var activeAudioSource = _firstSourceActive
            ? _audioSource1
            : _audioSource2;

        var inactiveAudioSource = _firstSourceActive
            ? _audioSource2
            : _audioSource1;

        var clip = musicType switch
        {
            MusicType.Menu => menuMusic,
            MusicType.Run => runMusic,
            MusicType.Boss => bossMusic,
            _ => throw new NotImplementedException("Unkown music type " + musicType)
        };

        StartCoroutine(StartFade(activeAudioSource, inactiveAudioSource, clip, duration: 1f, volume: _maxVolume));
    }

    public static IEnumerator StartFade(AudioSource activeSource, AudioSource inactiveSource, AudioClip newClip,
        float duration, float volume)
    {
        inactiveSource.clip = newClip;
        float currentTime = 0;
        float start = 0;
        inactiveSource.Play();
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            inactiveSource.volume = Mathf.Lerp(start, volume, currentTime / duration);
            activeSource.volume = volume - inactiveSource.volume;
            yield return null;
        }

        _firstSourceActive = !_firstSourceActive;
        activeSource.Stop();

        yield break;
    }
}

public enum MusicType
{
    Menu,
    Run,
    Boss,
}
