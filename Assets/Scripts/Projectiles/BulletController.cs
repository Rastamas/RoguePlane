using System;
using UnityEngine;

public class BulletController : MonoBehaviour, IProjectileController
{
    public float speed;
    public int damage;

    public bool isEnemyProjectile;

    public Vector3 direction;

    [SerializeField]
    private Material _glowingMaterial;

    private Rigidbody _rigidbody;
    private bool _trailEnabled;

    public void Start()
    {
        gameObject.layer = isEnemyProjectile
            ? Constants.EnemiesLayer
            : Constants.PlayerLayer;

        var trailRenderer = GetComponent<TrailRenderer>();
        if (trailRenderer != null)
        {
            trailRenderer.enabled = _trailEnabled;
        }
    }

    public void EnableTrail()
    {
        _trailEnabled = true;
        GetComponent<MeshRenderer>().material = _glowingMaterial;
    }

    public void OnTriggerEnter(Collider collider)
    {
        if (IsOffScreen)
        {
            return;
        }

        if (!isEnemyProjectile && collider.tag == Constants.EnemyTag)
        {
            Destroy(gameObject);
            collider.gameObject.GetComponent<EnemyController>().Hit(damage);
            return;
        }

        if (isEnemyProjectile && collider.tag == Constants.PlayerTag)
        {
            Destroy(gameObject);
            collider.gameObject.GetComponent<PlayerController>().Hit(damage);
        }
    }

    private bool IsOffScreen => transform.position.z > EnemySpawner.instance.transform.position.z + EnemySpawner.instance.spawnZOffset;

    public void Initialize(Projectile projectile, Vector3 direction)
    {
        damage = projectile.damage;

        this.direction = direction;

        speed *= projectile.isEnemy
            ? 0.5f
            : 1;

        isEnemyProjectile = projectile.isEnemy;

        var drawTrail = projectile.owner?.GetComponent<WeaponController>()?.damageBoostActive ?? false;
        if (drawTrail)
        {
            EnableTrail();
        }

        _rigidbody = GetComponent<Rigidbody>();

        _rigidbody.velocity = direction * speed;
    }
}
