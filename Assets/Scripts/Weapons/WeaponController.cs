using System;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public GameObject projectilePrefab;
    public int weaponDamage;
    public float attacksPerSecond;
    public WeaponDirection direction;
    public AudioClip attackSpeedBoostClip;
    public AudioClip attackDamageBoostClip;
    public AudioClip fireClip;
    public bool damageBoostActive;
    public bool speedBoostActive;

    private Projectile _projectile;
    private GameObjectWeapon _weapon;
    private AudioSource _audioSource;
    private bool _isEnabled;

    public void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource != null)
        {
            _audioSource.volume *= PlayerPrefs.GetFloat(Constants.SfxVolumePlayerPrefKey, 1f);
        }
        damageBoostActive = false;

        _projectile = new Projectile(projectilePrefab)
        {
            owner = gameObject,
            damage = weaponDamage,
            isEnemy = GetComponent<EnemyController>() != null
        };

        _weapon = new GameObjectWeapon(parent: gameObject, attacksPerSecond, direction, _projectile);
    }

    public void Update()
    {
        if (!_isEnabled)
        {
            return;
        }

        var projectile = _weapon.FireProjectiles<BulletController>();

        if (projectile != null && _audioSource != null && fireClip != null)
        {
            _audioSource.PlayOneShot(fireClip);
        }
    }

    public void Enable()
    {
        _isEnabled = true;
    }

    public void Disable()
    {
        _isEnabled = false;
    }

    public void IncreaseAttackSpeed(float multiplier)
    {
        _audioSource?.PlayOneShot(attackSpeedBoostClip);
        attacksPerSecond *= multiplier;
        _weapon.ChangeAttackSpeed(multiplier);
        speedBoostActive = true;
    }

    public void IncreaseAttackDamage(int multiplier)
    {
        _audioSource?.PlayOneShot(attackDamageBoostClip);
        weaponDamage *= multiplier;
        damageBoostActive = true;
    }
}
