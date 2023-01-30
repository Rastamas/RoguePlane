using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameObjectWeapon : Weapon
{
    private readonly bool _connectProjectilesToParent;
    private readonly List<GameObject> _projectiles;
    private readonly Projectile _projectile;

    public GameObjectWeapon(GameObject parent, float attacksPerSecond, WeaponDirection direction, Projectile projectile,
        bool isSingleShot = false, Vector3? spawnOffset = null, bool connectToParent = true, AudioClip soundEffect = null, float volume = .5f) : base(parent, attacksPerSecond, direction, isSingleShot, spawnOffset, soundEffect, volume)
    {
        if (projectile.isEnemy && PermanentProgressionManager.IsChallengeEnabed(Challenge.Deadly))
        {
            projectile.damage *= 2;
        }

        _connectProjectilesToParent = connectToParent;
        _projectiles = new List<GameObject>();
        _projectile = projectile;
    }

    public void Reset()
    {
        _lastFireTime = 0;
        foreach (var projectile in _projectiles.Where(p => p != null))
        {
            GameObject.Destroy(projectile);
        }
        _projectiles.Clear();
    }

    public T FireProjectiles<T>(Quaternion? directionOverride = null, Transform parent = null) where T : IProjectileController
    {
        if (!_isSingleShot && WeaponIsOnCooldown || _isSingleShot && _lastFireTime != 0)
        {
            return default;
        }

        return SpawnProjectiles<T>(directionOverride, parent);
    }

    public T FireProjectiles<T>(Transform source, Transform target, Transform parent = null) where T : IProjectileController
    {
        if (!_isSingleShot && WeaponIsOnCooldown || _isSingleShot && _lastFireTime != 0)
        {
            return default;
        }

        var playerDirection = target.transform.position - source.transform.position;

        var directionOverride = Quaternion.FromToRotation(Vector3.back, playerDirection);

        return SpawnProjectiles<T>(directionOverride, parent);
    }

    private T SpawnProjectiles<T>(Quaternion? directionOverride, Transform parent) where T : IProjectileController
    {
        var rotation = (directionOverride ?? Quaternion.identity) * DefaultRotation;

        var projectileGameObject = MonoBehaviour.Instantiate(_projectile.prefab, _parent.transform.position + spawnOffset - _projectile.colliderOffset, rotation,
            _connectProjectilesToParent ? parent : null);

        _audioSource?.PlayOneShot(_audioSource.clip);

        var projectileController = projectileGameObject.GetComponent<T>();

        _projectiles.Add(projectileGameObject);

        projectileController.Initialize(_projectile, (directionOverride ?? Quaternion.identity) * DefaultDirection);

        _lastFireTime = Time.time;

        return projectileController;
    }
}
