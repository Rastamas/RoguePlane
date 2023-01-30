using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BulkyController : TriggeredPhaseController, IEnemyDestroyed
{
    private float zSize;
    private List<ParticleWeapon> _weapons;
    const float damage = 10;

    public void Awake()
    {
        zSize = GetComponent<NavMeshObstacle>().size.z * transform.localScale.z;
        SetupWeapon();

        _phases = new Dictionary<int, (Action action, Func<bool> checkAction, Action closingAction)> {
            {0, (null, IsFullyOnScreen, null)},
            {1, (FireWeapons, null, null)},
        };
    }

    private bool IsFullyOnScreen() =>
        transform.position.z < EnemySpawner.instance.GetCorner(Corner.TopLeft).z - zSize / 2;

    private void SetupWeapon()
    {
        var soundEffect = Resources.Load<AudioClip>("Sounds/Explosions/ES_Pressure Blast 16");
        _weapons = new List<ParticleWeapon>();
        var lightningBoltPrefab = Resources.Load<GameObject>("Prefabs/Projectiles/LightningBolt");

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            var weapon = new ParticleWeapon(child.gameObject, attacksPerSecond: .4f, WeaponDirection.Backward,
             lightningBoltPrefab, damage: damage, soundEffect: i == 0 ? soundEffect : null, volume: .1f);

            _weapons.Add(weapon);
        }
    }

    private void FireWeapons()
    {
        foreach (var weapon in _weapons)
        {
            if (weapon.particleSystemGameObject == null)
            {
                return;
            }

            weapon.FireProjectiles(1, weapon.particleSystemGameObject.transform.position + Vector3.back);
        }
    }

    public void OnEnemyDestroyed()
    {
        _weapons.ForEach(weapon => weapon.DetachParticleSystem());
    }
}
