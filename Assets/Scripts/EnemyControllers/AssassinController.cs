using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssassinController : TimedPhaseController
{
    public int damage;
    public float attacksPerSecond;

    private Vector3 _desiredPositionLeftOffset;
    private Vector3 _desiredPositionRightOffset;
    private Vector3 _spawnPointOffset;
    private Vector3 DesiredPositionLeft => AutoScroll.instance.transform.position + _desiredPositionLeftOffset;
    private Vector3 DesiredPositionRight => AutoScroll.instance.transform.position + _desiredPositionRightOffset;
    private Vector3 SpawnPoint => AutoScroll.instance.transform.position + _spawnPointOffset;
    private GameObjectWeapon _sniperWeapon;
    private bool _leftToRight;
    private const float screenTransitionTime = 3;

    public new void Start()
    {
        base.Start();

        _desiredPositionLeftOffset = new Vector3(0.2f, 0.8f, 0).ViewportPointToSpawnPoint() - AutoScroll.instance.transform.position;
        _desiredPositionRightOffset = new Vector3(0.8f, 0.8f, 0).ViewportPointToSpawnPoint() - AutoScroll.instance.transform.position;

        OverrideSpawnPosition();

        _phases = new Dictionary<int, (Action action, float durationInS, Action closingAction)> {
            {0, (ProgressPosition, 2, null)},
            {1, (StayInPosition, 2, null)},
            {2, (FireSniper, 0.5f, null)},
            {3, (StayInPosition, 2, LookBack)},
            {4, (MoveAcrossScreen, screenTransitionTime, null)},
            {5, (StayInPosition, 2, null)},
            {6, (FireSniper, 0.5f, null)},
            {7, (StayInPosition, 2, LookBack)},
            {8, (MoveAcrossScreen, screenTransitionTime, null)},
        };

        SetupSniperWeapon();
    }

    public new void Update()
    {
        base.Update();
    }

    private void OverrideSpawnPosition()
    {
        _leftToRight = UnityEngine.Random.value > 0.5f;
        var startingPosition = _leftToRight
            ? DesiredPositionLeft
            : DesiredPositionRight;

        _spawnPointOffset = new Vector3(startingPosition.x, transform.position.y, transform.position.z) - AutoScroll.instance.transform.position;
        transform.position = SpawnPoint;
    }

    private void MoveAcrossScreen()
    {
        if (_currentPhase == (_leftToRight ? 4 : 8))
        {
            transform.position = Vector3.Lerp(DesiredPositionLeft, DesiredPositionRight, (Time.time - _phaseStartTime) / CurrentPhaseDuration);
        }
        else
        {
            transform.position = Vector3.Lerp(DesiredPositionRight, DesiredPositionLeft, (Time.time - _phaseStartTime) / CurrentPhaseDuration);
        }

    }

    private void LookBack()
    {
        var currentRotation = transform.rotation;
        var desiredRotation = Quaternion.LookRotation(Vector3.back, Vector3.up);

        StartCoroutine(LerpRotation(currentRotation, desiredRotation, screenTransitionTime));
    }

    private IEnumerator LerpRotation(Quaternion start, Quaternion desired, float time)
    {
        var currentTime = 0f;

        while (currentTime < time)
        {
            currentTime += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(start, desired, currentTime / time);
            yield return null;
        }

        yield break;
    }

    private void SetupSniperWeapon()
    {
        var bulletPrefab = Resources.Load<GameObject>("Prefabs/Projectiles/Sniper");
        var soundEffect = Resources.Load<AudioClip>("Sounds/Explosions/ES_Laser Gun Fire 5");
        var bulletProjectile = new Projectile(bulletPrefab)
        {
            damage = damage,
            isEnemy = true,
        };

        _sniperWeapon = new GameObjectWeapon(gameObject, attacksPerSecond, WeaponDirection.Backward, bulletProjectile, soundEffect: soundEffect);
    }

    private void StayInPosition()
    {
        var rotationToPlayer = Vector3.RotateTowards(transform.forward,
         GameController.GetPlayer().transform.position - transform.position,
         2 * Time.deltaTime, 0f);
        transform.rotation = Quaternion.LookRotation(rotationToPlayer);

        if (_currentPhase <= 4)
        {
            transform.position = _leftToRight ? DesiredPositionLeft : DesiredPositionRight;
        }
        else
        {
            transform.position = _leftToRight ? DesiredPositionRight : DesiredPositionLeft;
        }
    }

    private void ProgressPosition()
    {
        transform.position = Vector3.Lerp(SpawnPoint, _leftToRight
            ? DesiredPositionLeft
            : DesiredPositionRight,
            (Time.time - _phaseStartTime) / CurrentPhaseDuration);
    }

    void FireSniper()
    {
        StayInPosition();

        _sniperWeapon.FireProjectiles<BulletController>(transform, GameController.GetPlayer().transform, parent: AutoScroll.instance.transform);
    }
}
