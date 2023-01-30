using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class PezController : MonoBehaviour, IEnemyDestroyed
{
    public PezController abovePez;
    public delegate void GoToPosition(Vector3 position);
    public event GoToPosition OnTopPezDestroyed;

    private ParticleWeapon _weapon;
    private Vector3 _startingPosition;
    private Vector3 _abovePosition;
    private bool _isRising;
    private bool _isQuitting;
    private Rigidbody _rigidBody;
    private float _height;

    public void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    public void Awake()
    {
        SetupWeapon();

        _startingPosition = transform.position;
        _rigidBody = GetComponent<Rigidbody>();
        _height = GetComponent<NavMeshObstacle>().size.y * transform.localScale.y;

        if (transform.position.y == EnemySpawner.playingLevel)
        {
            return;
        }

        var pezesAbove = EnemySpawner.instance.activeEnemies.Where(e =>
        {
            if (e == null)
            {
                return false;
            }

            var pez = e.GetComponent<PezController>();

            return pez != null && e.transform.position.x == transform.position.x &&
                e.transform.position.z == transform.position.z &&
                e.transform.position.y > transform.position.y;
        });

        abovePez = pezesAbove.OrderBy(p => p.transform.position.y).FirstOrDefault()?.GetComponent<PezController>();

        if (abovePez == null)
        {
            return;
        }

        abovePez.OnTopPezDestroyed += StartRising;

        _abovePosition = abovePez.transform.position;
    }

    private void SetupWeapon()
    {
        var size = GetComponent<NavMeshObstacle>().size;
        var weaponOffset = Vector3.back * size.z / 2 * transform.localScale.z;
        var fireballPrefab = Resources.Load<GameObject>("Prefabs/Projectiles/Fireball");
        var soundEffect = Resources.Load<AudioClip>("Sounds/Explosions/ES_Inter-Dimensional Blast 6");

        _weapon = new ParticleWeapon(gameObject, attacksPerSecond: 1, WeaponDirection.Backward, fireballPrefab,
            spawnOffset: weaponOffset, damage: 10, soundEffect: soundEffect);
    }

    public void Update()
    {
        if (!_isRising)
        {
            if (StuckNotOnPlayingLevel())
            {
                transform.position = new Vector3(transform.position.x, EnemySpawner.playingLevel, transform.position.z);
            }

            return;
        }

        var riseDone = transform.position.y >= _abovePosition.y;

        if (riseDone)
        {
            FinishRising();
        }
    }

    private bool StuckNotOnPlayingLevel()
    {
        return abovePez == null &&
            (transform.position.y > EnemySpawner.playingLevel + _height / 2 ||
            transform.position.y < EnemySpawner.playingLevel - _height / 2);
    }

    public void StartRising(Vector3 newTargetPosition)
    {
        if (OnTopPezDestroyed != null)
        {
            OnTopPezDestroyed.Invoke(_isRising ? _abovePosition : transform.position);
        }

        _isRising = true;
        _rigidBody.constraints &= ~RigidbodyConstraints.FreezePositionY;
        _rigidBody.AddForce(Vector3.up * 3, ForceMode.VelocityChange);

        _startingPosition = transform.position;
        _abovePosition = newTargetPosition;
    }

    private void FinishRising()
    {
        _rigidBody.constraints |= RigidbodyConstraints.FreezePositionY;

        if (transform.position.y >= EnemySpawner.playingLevel)
        {
            _weapon.FireProjectiles(1, GameController.GetPlayer().transform);
        }

        _isRising = false;
        _startingPosition = transform.position;
        if (abovePez == null)
        {
            return;
        }

        _abovePosition = abovePez.transform.position;
    }

    public void OnDestroy()
    {
        if (_isQuitting || !gameObject.scene.isLoaded)
        {
            return;
        }

        if (OnTopPezDestroyed != null)
        {
            OnTopPezDestroyed.Invoke(_isRising ? _abovePosition : _startingPosition);
        }
    }

    public void OnEnemyDestroyed()
    {
        _weapon.DetachParticleSystem();
    }
}
