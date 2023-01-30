using System;
using System.Collections.Generic;
using UnityEngine;

public class HomingMissileController : TriggeredPhaseController, IProjectileController
{
    private int _damage;
    private ParticleSystem _propulsion;
    private ParticleSystem _explosion;
    private Transform _target;
    private float _speed = 10;
    private readonly float _rotationSpeed = 2f;
    private readonly int _lifeTimeInS = 4;
    private Vector3 startingDirection = new Vector3(0, 5.77f, -10);

    public void Awake()
    {
        _target = GameController.GetPlayer().transform;

        _propulsion = transform.GetChild(0).GetChild(0).GetComponent<ParticleSystem>();
        _propulsion.Stop();

        _explosion = transform.GetChild(1).GetComponent<ParticleSystem>();

        _phases = new Dictionary<int, (Action action, Func<bool> checkAction, Action closingAction)> {
            {0, (Fall, IsBelowSpawnLevel, EnablePropulsion)},
            {1, (FlyToPlayer, IsOldEnough, Explode)},
            {2, (null, null, null)}
        };
    }

    public void ExplodeWithDamage()
    {
        Explode();

        GameController.GetPlayerController().Hit(_damage);
    }

    public void Explode()
    {
        if (_currentPhase != 2)
        {
            _currentPhase = 2;
        }

        GetComponentInChildren<MeshRenderer>().enabled = false;
        GetComponentInChildren<MeshCollider>().enabled = false;
        _propulsion.Stop();
        _explosion.Play();

        var destroyInS = _explosion.main.startLifetime.constantMax;

        Invoke(nameof(DestroySelf), destroyInS);
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }

    private bool IsOldEnough()
    {
        return Time.time - _phaseStartTime > _lifeTimeInS;
    }

    private void Fall()
    {
        var direction = startingDirection + (Time.time - _phaseStartTime) * Vector3.down * 10 * 2;

        transform.LookAt(transform.position + direction);
        transform.position += direction * Time.deltaTime * 0.9f;
    }

    private void EnablePropulsion()
    {
        _propulsion.Play();
    }

    private bool IsBelowSpawnLevel()
    {
        return transform.position.y <= EnemySpawner.playingLevel;
    }

    private void FlyToPlayer()
    {
        var targetDirection = _target.position - transform.position;

        var newDirection = Vector3.RotateTowards(transform.forward, targetDirection, _rotationSpeed * Time.deltaTime, 0.0F);

        transform.Translate(Vector3.forward * Time.deltaTime * _speed, Space.Self);

        _speed += Time.deltaTime * 2;

        transform.rotation = Quaternion.LookRotation(newDirection);
    }

    protected new void Start()
    {
        base.Start();

        transform.LookAt(transform.position + startingDirection);
    }

    protected new void Update()
    {
        base.Update();
    }

    public void Initialize(Projectile projectile, Vector3 direction)
    {
        _damage = projectile.damage;

        gameObject.layer = Constants.EnemiesLayer;
    }
}
