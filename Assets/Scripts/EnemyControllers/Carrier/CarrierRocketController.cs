using System;
using System.Collections;
using UnityEngine;

public class CarrierRocketController : MonoBehaviour
{
    private ParticleWeapon _weapon;
    private AudioSource _audioSource;
    private bool _isEnabled;
    private const float attacksPerSecond = .25f;

    public void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        var rocketPrefab = Resources.Load<GameObject>("Prefabs/Projectiles/Rocket");

        _weapon = new ParticleWeapon(parent: gameObject, attacksPerSecond, WeaponDirection.Backward, rocketPrefab, damage: 10);
    }

    public void Update()
    {
        if (!_isEnabled)
        {
            return;
        }

        transform.LookAt(GameController.GetPlayer().transform);

        var (count, delay) = _weapon.FireBurst(GameController.GetPlayer().transform);

        if (count > 0)
        {
            StartCoroutine(PlayBurstedAudio(count, delay));
            Invoke(nameof(StopBurst), count * delay + .5f);
        }
    }

    private IEnumerator PlayBurstedAudio(int count, float delay)
    {
        var progressTime = 0f;
        var playedBursts = 0;

        while (playedBursts < count)
        {
            progressTime += Time.deltaTime;

            if (progressTime > playedBursts * delay)
            {
                _audioSource.PlayOneShot(_audioSource.clip, .5f);
                playedBursts++;
            }
            yield return null;
        }

        yield break;
    }

    private void StopBurst()
    {
        _weapon.StopEmission();
    }

    public void EnableInSeconds(float seconds)
    {
        Invoke(nameof(Enable), seconds);
    }

    public void Enable()
    {
        if (_weapon.particleSystemGameObject == null)
        {
            _weapon.Reinstantiate();
        }

        _isEnabled = true;
    }

    public void Disable()
    {
        _weapon.DetachParticleSystem();
        _isEnabled = false;
    }
}
