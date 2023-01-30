using System;
using UnityEngine;

public class DamagingParticleController : MonoBehaviour
{
    public float damage;
    public Action<float> onEnemyHitCallback;
    private bool _isEnemyProjectile;
    private AudioSource _audioSource;
    private AudioClip _audioClip;
    private float _lastHitSoundTime;
    private const float hitSoundCooldown = .1f;

    public void Awake()
    {
        _isEnemyProjectile = GetComponentInParent<PlayerController>() == null;

        if (!_isEnemyProjectile)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.volume = .1f * PlayerPrefs.GetFloat(Constants.SfxVolumePlayerPrefKey, 1f);
            _audioClip = Resources.Load<AudioClip>("Sounds/Explosions/LaserHit");
        }
    }

    public void OnParticleCollision(GameObject other)
    {
        if (!_isEnemyProjectile && other.tag == Constants.EnemyTag)
        {
            var enemyController = other.GetComponent<EnemyController>();

            var damageDealt = enemyController.Hit(damage);

            if (Time.time - _lastHitSoundTime > hitSoundCooldown)
            {
                _audioSource.PlayOneShot(_audioClip);
                _lastHitSoundTime = Time.time;
            }

            onEnemyHitCallback?.Invoke(damageDealt);
        }

        if (_isEnemyProjectile && other.tag == Constants.PlayerTag)
        {
            var playerController = other.GetComponent<PlayerController>();

            playerController.Hit(damage);
        }
    }
}
