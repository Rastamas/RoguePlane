using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class ParticleWeapon : Weapon
{
    public GameObject particleSystemGameObject;
    public ParticleSystem particleSystem;
    public DamagingParticleController damagingParticleController;
    public float damage;
    private float _attacksPerSecond;

    private readonly GameObject _particleSystemPrefab;
    private readonly Vector3? _spawnOffset;

    public ParticleWeapon(GameObject parent, float attacksPerSecond, WeaponDirection direction, GameObject particleSystemPrefab, float damage,
        bool isSingleShot = false, Vector3? spawnOffset = null, bool isEnemy = true, AudioClip soundEffect = null, float volume = .5f) : base(parent, attacksPerSecond, direction, isSingleShot, spawnOffset, soundEffect, volume)
    {

        _parent = parent;
        _particleSystemPrefab = particleSystemPrefab;
        _spawnOffset = spawnOffset;
        _attacksPerSecond = attacksPerSecond;

        this.damage = damage;

        if (isEnemy && PermanentProgressionManager.IsChallengeEnabed(Challenge.Deadly))
        {
            this.damage *= 2;
        }

        Reinstantiate();
    }

    public void Reinstantiate()
    {
        particleSystemGameObject = GameObject.Instantiate(_particleSystemPrefab,
            _parent.transform.position + (_spawnOffset ?? Vector3.zero), Quaternion.identity, _parent.transform);

        damagingParticleController = particleSystemGameObject.GetComponent<DamagingParticleController>();
        if (damagingParticleController != null)
        {
            damagingParticleController.damage = damage;
        }

        particleSystem = particleSystemGameObject.GetComponent<ParticleSystem>();

        ApplyAttackSpeedToAllParticleSystems();
    }

    private void ApplyAttackSpeedToAllParticleSystems()
    {
        foreach (var childParticleSystem in particleSystemGameObject.GetComponentsInChildren<ParticleSystem>())
        {
            ApplyAttackSpeed(childParticleSystem);
        }
    }

    public void ChangeAttackDamage(float weaponDamage)
    {
        damage = weaponDamage;
        var damageController = particleSystemGameObject.GetComponent<DamagingParticleController>();
        damageController.damage = weaponDamage;
    }

    public override void ChangeAttackSpeed(float multiplier)
    {
        base.ChangeAttackSpeed(multiplier);
        _attacksPerSecond *= multiplier;

        ApplyAttackSpeedToAllParticleSystems();
    }

    private void ApplyAttackSpeed(ParticleSystem particleSystem)
    {
        particleSystem.Stop(withChildren: false, ParticleSystemStopBehavior.StopEmittingAndClear);
        var module = particleSystem.main;
        module.duration = GetWeaponCooldownInMs(_attacksPerSecond) / 1000;
    }

    public void SetOutsideColorScheme(Color color)
    {
        particleSystemGameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_Color", color);
    }

    public void SetInsideColorScheme(Color color)
    {
        particleSystemGameObject.transform.GetChild(2)
            .GetComponent<ParticleSystemRenderer>().material.SetColor("_Color", color);
    }

    public void Reset()
    {
        _lastFireTime = 0;
        particleSystem.SetParticles(new Particle[0], 0, 0);
    }

    public (int count, float delay) FireBurst(Transform target = null) => FireBurst(target.position);
    public (int count, float delay) FireBurst(Vector3? target = null)
    {
        if (!_isSingleShot && WeaponIsOnCooldown || _isSingleShot && _lastFireTime != 0 || particleSystemGameObject == null)
        {
            return (0, 0);
        }

        if (target != null)
        {
            particleSystemGameObject.transform.LookAt(target.Value);
        }

        particleSystem.Play();

        var burst = particleSystem.emission.GetBurst(0);

        _lastFireTime = Time.time;

        return (burst.cycleCount, burst.repeatInterval);
    }

    public void StartEmission()
    {
        particleSystem.Play();
    }

    public void StopEmission()
    {
        particleSystem.Stop();
    }

    public void FireProjectiles(int count, Transform target = null) => FireProjectiles(count, target?.position);
    public void FireProjectiles(int count, Vector3? target = null)
    {
        if (!_isSingleShot && WeaponIsOnCooldown || _isSingleShot && _lastFireTime != 0 || particleSystemGameObject == null)
        {
            return;
        }

        if (target != null)
        {
            particleSystemGameObject.transform.LookAt(target.Value);
        }

        particleSystem.Emit(count);

        _audioSource?.PlayOneShot(_audioSource.clip);

        _lastFireTime = Time.time;
    }

    public void DetachParticleSystem(float destroyInSeconds = 3.0f, Vector3? updateLocalScale = null)
    {
        if (particleSystemGameObject == null)
        {
            return;
        }

        particleSystemGameObject.transform.parent = AutoScroll.instance.transform;
        particleSystemGameObject.transform.localScale = updateLocalScale ?? Vector3.one;

        GameObject.Destroy(particleSystemGameObject, destroyInSeconds);
    }
}
