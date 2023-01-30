using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SFXPlayer : MonoBehaviour
{
    private static SFXPlayer _instance;

    public static SFXPlayer instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SFXPlayer>();
            }

            return _instance;
        }
    }

    private static AudioSource _audioSource;
    private static Dictionary<SfxOneshot, AudioClip> _audioClips;
    private static Dictionary<SfxOneshot, float> _volumes;

    public void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.volume *= PlayerPrefs.GetFloat(Constants.SfxVolumePlayerPrefKey, 1f);
        _audioClips = EnumExtensions.GetValues<SfxOneshot>()
            .ToDictionary(type => type, type => Resources.Load<AudioClip>(type switch
            {
                SfxOneshot.Coin => "Sounds/Collectibles/ES_Coin Flip Toss Ring",
                SfxOneshot.Mine => "Sounds/Explosions/ES_Laser Cannon Explosion 10",
                SfxOneshot.Powerup => "Sounds/Powerups/Powerup Damage",
                _ => throw new NotImplementedException("Unkown sfx type: " + type)
            }));

        _volumes = EnumExtensions.GetValues<SfxOneshot>()
            .ToDictionary(type => type, type => type switch
            {
                SfxOneshot.Coin => 1f,
                SfxOneshot.Mine => .5f,
                SfxOneshot.Powerup => 2f,
                _ => throw new NotImplementedException("Unkown sfx type: " + type)
            });
    }

    public static void PlayOneshot(SfxOneshot type)
    {
        _audioSource.PlayOneShot(_audioClips[type], _volumes[type]);
    }
}

public enum SfxOneshot
{
    Coin,
    Mine,
    Powerup
}
