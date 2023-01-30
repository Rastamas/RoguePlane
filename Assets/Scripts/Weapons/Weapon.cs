using UnityEngine;

public abstract class Weapon
{
    public Vector3 spawnOffset;
    protected float _lastFireTime = 0;
    protected float _weaponCooldownInMs;
    protected WeaponDirection _direction;
    protected GameObject _parent;
    protected bool _isSingleShot;
    protected AudioSource _audioSource;

    public Weapon(GameObject parent, float attacksPerSecond, WeaponDirection direction, bool isSingleShot, Vector3? spawnOffset, AudioClip soundEffect, float volume)
    {
        _parent = parent;
        _weaponCooldownInMs = GetWeaponCooldownInMs(attacksPerSecond);
        _direction = direction;
        _isSingleShot = isSingleShot;
        this.spawnOffset = spawnOffset ?? Vector3.zero;

        if (soundEffect != null)
        {
            _audioSource = parent.AddComponent<AudioSource>();
            _audioSource.volume = volume * PlayerPrefs.GetFloat(Constants.SfxVolumePlayerPrefKey, 1f);
            _audioSource.clip = soundEffect;
        }
    }

    protected static float GetWeaponCooldownInMs(float attacksPerSecond) => 1000f / (attacksPerSecond == 0f ? 1f : attacksPerSecond);

    public virtual void ChangeAttackSpeed(float multiplier)
    {
        _weaponCooldownInMs /= multiplier;
    }

    protected bool WeaponIsOnCooldown => Time.time - _lastFireTime < _weaponCooldownInMs / 1000;

    protected Quaternion DefaultRotation => _direction == WeaponDirection.Forward
                    ? Quaternion.AngleAxis(180, Vector3.up)
                    : Quaternion.identity;

    protected Vector3 DefaultDirection => _direction == WeaponDirection.Forward
                    ? Vector3.forward
                    : Vector3.back;
}
