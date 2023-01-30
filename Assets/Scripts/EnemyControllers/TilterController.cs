using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TilterController : TimedPhaseController, IEnemyDestroyed
{
    public int damage;
    public float attacksPerSecond;

    private ParticleWeapon _weapon;
    private Vector3 _spawnPointOffset;
    private Vector3 spawnPoint => EnemySpawner.instance.transform.position + _spawnPointOffset;
    private Vector3 _turningPointOffset;
    private Vector3 turningPoint => EnemySpawner.instance.transform.position + _turningPointOffset;
    private Vector3 _controlPointOffset;
    private Vector3 controlPoint => EnemySpawner.instance.transform.position + _controlPointOffset;

    public void Awake()
    {
        SetupSideWeapon();
    }

    public new void Start()
    {
        OverrideSpawnPosition();
        _spawnPointOffset = transform.position - EnemySpawner.instance.transform.position;
        _turningPointOffset = new Vector3(0.2f, 0.7f, 0).ViewportPointToSpawnPoint() - EnemySpawner.instance.transform.position;
        _controlPointOffset = new Vector3(turningPoint.x, turningPoint.y, transform.position.z) - EnemySpawner.instance.transform.position;

        _phases = new Dictionary<int, (Action action, float durationInS, Action closingAction)> {
            {0, (MoveSideways, 3, null)},
            {1, (MoveBack, 5, DestroySelf)},
        };

        base.Start();
    }

    private void MoveSideways()
    {
        var time = (Time.time - _phaseStartTime) / CurrentPhaseDuration;
        transform.position = Bezier.GetQuadraticBezierPoint(time, spawnPoint, controlPoint, turningPoint);
        transform.LookAt(transform.position + Bezier.GetQuadraticBezierTangent(time, spawnPoint, controlPoint, turningPoint), Vector3.up);

        if (Time.time - _phaseStartTime > 0.5f)
        {
            _weapon.FireProjectiles(1, target: GameController.GetPlayer().transform);
        }
    }

    private void MoveBack()
    {
        transform.position += Vector3.back * Time.deltaTime / 2;
    }

    private void SetupSideWeapon()
    {
        var size = GetComponent<NavMeshObstacle>().size;
        var weaponOffset = Vector3.right * size.x / 2 * transform.localScale.x;
        var artilleryPrefab = Resources.Load<GameObject>("Prefabs/Projectiles/Artillery");

        _weapon = new ParticleWeapon(gameObject, attacksPerSecond: 1, WeaponDirection.Backward, artilleryPrefab, spawnOffset: weaponOffset, damage: 20);
    }

    private void OverrideSpawnPosition()
    {
        var size = GetComponent<NavMeshObstacle>().size;

        transform.position = EnemySpawner.instance.GetCorner(Corner.TopRight)
            + Vector3.right * size.x / 2 * transform.localScale.x
            + Vector3.back * size.z / 2 * transform.localScale.z;
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }

    public void OnEnemyDestroyed()
    {
        _weapon.DetachParticleSystem(updateLocalScale: transform.localScale);
    }
}
