using System;
using UnityEngine;

public class CarrierHomingMissileController : MonoBehaviour, IProjectileController
{
    private Projectile _projectile;
    private GameObjectWeapon _weapon;
    private bool _isEnabled;

    private const int weaponDamage = 10;
    private const float attacksPerSecond = 0.2f;

    public void Start()
    {
        _projectile = new Projectile(Resources.Load<GameObject>("Prefabs/Projectiles/RotatedHomingMissile"))
        {
            owner = gameObject,
            damage = weaponDamage,
            isEnemy = true
        };

        _weapon = new GameObjectWeapon(parent: gameObject, attacksPerSecond, WeaponDirection.Backward, _projectile);
    }

    public void Update()
    {
        if (!_isEnabled)
        {
            return;
        }

        _weapon.FireProjectiles<HomingMissileController>(transform, GameController.GetPlayer().transform);
    }

    public void EnableInSeconds(float seconds)
    {
        Invoke(nameof(Enable), seconds);
    }

    public void Enable()
    {
        _isEnabled = true;
    }

    public void Disable()
    {
        _isEnabled = false;
    }

    public void Initialize(Projectile projectile, Vector3 direction)
    {

    }
}
