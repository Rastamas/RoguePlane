using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class InterceptorController : TimedPhaseController, IProjectileController, IEnemyDestroyed
{
    private Vector3 _spawnPoint;
    private Vector3 _bezierPoint3;
    private Vector3 _bezierPoint2;
    private Vector3 _bezierPoint1;
    private Vector3 _bezierPoint0;
    private ParticleWeapon _weapon;
    public InterceptorDirection direction;

    public void Initialize(Projectile projectile, Vector3 direction)
    {
    }

    public void Awake()
    {
        _spawnPoint = transform.position;

        var size = GetComponent<NavMeshObstacle>().size;
        var weaponOffset = Vector3.forward * size.z / 2 * transform.localScale.z;
        var laserPrefab = Resources.Load<GameObject>("Prefabs/Projectiles/LaserShot");

        _weapon = new ParticleWeapon(gameObject, attacksPerSecond: 3, WeaponDirection.Backward, laserPrefab, spawnOffset: weaponOffset, damage: 10);

        _phases = new Dictionary<int, (Action action, float durationInS, Action closingAction)> {
            {0, (SpeedUp, 3, null)},
            {1, (FlyBezierPath, 3, DestroySelf)}
        };
    }

    public new void Start()
    {
        if (direction == InterceptorDirection.Backward)
        {
            SetupFrontRunningBezierCurve();
        }
        else
        {
            SetupSideRunningBezierCurve();
        }
        base.Start();
    }

    public new void Update()
    {
        base.Update();
    }

    public void SetDirection(InterceptorDirection direction)
    {
        this.direction = direction;

        transform.LookAt(transform.position + direction switch
        {
            InterceptorDirection.Backward => Vector3.back,
            InterceptorDirection.Side => Vector3.left,
            _ => throw new NotImplementedException("Unexpected InterceptorDirection")
        });
    }

    private void SpeedUp()
    {
        var progress = Mathf.Pow(Time.time - _phaseStartTime, 2) / Mathf.Pow(CurrentPhaseDuration, 2);
        transform.position = Vector3.Lerp(_spawnPoint, _bezierPoint0, progress);
    }

    private void SetupFrontRunningBezierCurve()
    {
        var screenMiddle = EnemySpawner.instance.transform.position;
        _bezierPoint0 = _spawnPoint + Vector3.back * 25;

        var xLeftBound = EnemySpawner.instance.GetCorner(Corner.BottomLeft).x;
        var xRightBound = EnemySpawner.instance.GetCorner(Corner.BottomRight).x;

        _bezierPoint1 = screenMiddle + EnemySpawner.instance.spawnZOffset * Vector3.back;

        _bezierPoint3 = _bezierPoint1;
        _bezierPoint3.x = Random.Range(xLeftBound, xRightBound);

        var controlPoint = screenMiddle;
        controlPoint.x = _bezierPoint3.x <= screenMiddle.x
            ? xLeftBound
            : xRightBound;

        _bezierPoint2 = controlPoint;
    }

    private void SetupSideRunningBezierCurve()
    {
        _bezierPoint0 = _spawnPoint + Vector3.left * 25;
        _bezierPoint1 = _bezierPoint0 + Vector3.left * 10;

        _bezierPoint2 = EnemySpawner.instance.GetCorner(Corner.BottomRight);
        _bezierPoint2.x = _bezierPoint1.x;

        var randomScreenHeight = Random.Range(0.2f, 0.6f);
        _bezierPoint3 = new Vector3(1, randomScreenHeight, 0).ViewportPointToSpawnPoint();
    }

    private void FlyBezierPath()
    {
        var time = (Time.time - _phaseStartTime) / CurrentPhaseDuration;
        transform.position = Bezier.GetCubicBezierPoint(time, _bezierPoint0, _bezierPoint1, _bezierPoint2, _bezierPoint3);
        var lookAt = transform.position + Bezier.GetCubicBezierTangent(time, _bezierPoint0, _bezierPoint1, _bezierPoint2, _bezierPoint3);

        transform.LookAt(lookAt, Vector3.up);

        _weapon.FireProjectiles(1, lookAt);
    }

    public void OnEnemyDestroyed()
    {
        _weapon.DetachParticleSystem();
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}

public enum InterceptorDirection
{
    Backward,
    Side,
}
