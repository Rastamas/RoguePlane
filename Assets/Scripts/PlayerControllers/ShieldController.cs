using UnityEngine;

public class ShieldController : MonoBehaviour
{
    public AudioClip shieldOnSound;
    public AudioClip shieldOffSound;
    private AudioSource _audioSource;
    private DissolveTrigger _dissolveTrigger;
    private float _lastShieldBreakTime;
    private float _cooldownTimeInS;
    public bool shieldIsOn;

    public void Awake()
    {
        _dissolveTrigger = GetComponent<DissolveTrigger>();
        _audioSource = GetComponent<AudioSource>();
        _audioSource.volume *= PlayerPrefs.GetFloat(Constants.SfxVolumePlayerPrefKey, 1f);
        _lastShieldBreakTime = float.MinValue;
    }

    public void Update()
    {
        if (!shieldIsOn && Time.time > _lastShieldBreakTime + _cooldownTimeInS)
        {
            TurnOnShield();
        }
    }

    public void Enable(float cooldown)
    {
        _cooldownTimeInS = cooldown;
    }

    private void TurnOnShield()
    {
        shieldIsOn = true;
        _dissolveTrigger.Appear();
        _audioSource.PlayOneShot(shieldOnSound, 0.1f);
    }

    public void TurnOffShield()
    {
        shieldIsOn = false;
        _dissolveTrigger.Dissolve();
        _audioSource.PlayOneShot(shieldOffSound, 0.1f);
        _lastShieldBreakTime = Time.time;
    }
}
