using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class CarrierController : TriggeredPhaseController, IEnemyDestroyed
{
    private Rigidbody _rigidbody;
    private Dictionary<int, (Vector3 Position, Vector3 LookAt)> _positions;
    private Vector3 _spawnPoint;
    private Vector3 _spawnPointOffset;
    private GameObjectWeapon _interceptorWeapon;
    private CarrierShieldController _shieldController;
    private List<GameObject> _shieldGenerators;
    private CarrierNanobotController[] _nanobotWeapons;
    private EnemyController _enemyController;
    private AudioSource _audioSource;
    private NavMeshObstacle _navMeshObstacle;
    private List<InterceptorController> _interceptors;
    private readonly Dictionary<string, int> _childIndices = new Dictionary<string, int> {
        {"Shield", 0},
        {"FrontGuns", 1},
        {"LeftGuns", 2},
        {"LeftNanobots", 3},
        {"InterceptorSpawn", 4},
        {"BackGuns", 7},
        {"Engines", 8},
        {"ExplosionsFront", 11},
        {"ExplosionsLeft", 12},
        {"ExplosionsRight", 13},
        {"ExplosionsBack", 14},
    };

    private readonly Dictionary<int, float> _phaseWaitTimes = new Dictionary<int, float> {
        {0, .1f},
        {7, AutoScroll.rampTimeInS * 1.5f}
    };
    private int _spawnedInterceptorCount;
    private float _length;

    private const float PhaseTransitionTime = 4f;
    private const int MaxInterceptorCount = 7;

    public void Awake()
    {
        _interceptors = new List<InterceptorController>();
        _rigidbody = GetComponent<Rigidbody>();
        _enemyController = GetComponent<EnemyController>();
        _audioSource = GetComponent<AudioSource>();
        _navMeshObstacle = GetComponent<NavMeshObstacle>();
        _shieldController = transform.GetChild(_childIndices["Shield"]).gameObject.GetComponent<CarrierShieldController>();
        _shieldGenerators = new List<GameObject>();
        _spawnPoint = transform.position;
        _spawnPointOffset = _spawnPoint - EnemySpawner.instance.transform.position;
        _length = _navMeshObstacle.size.z * transform.localScale.z;

        SetupPhasePositions();

        _phases = new Dictionary<int, (Action action, Func<bool> checkAction, Action closingAction)> {
            {0, (null, WaitedPhaseTime, null)},
            {1, (MoveToCurrentPhase, IsInPosition, StartPhaseOne)},
            {2, (FirePhaseOneWeapons, EnoughInterceptorsLeftCarrier, DisableFrontWeapons)},
            {3, (MoveToCurrentPhase, IsInPosition, StartPhaseTwo)},
            {4, (FirePhaseTwoWeapons, EnoughInterceptorsLeftCarrier, DisableLeftWeapons)},
            {5, (MoveToCurrentPhase, IsInPosition, EnableBackWeapons)},
            {6, (null, MovedOffScreen, RestartPhases)},
            {7, (null, WaitedPhaseTime, ResetSpawn)}, // Wait for scrolling to stop again
        };

        SetupInterceptors();

        _nanobotWeapons = transform.GetChild(_childIndices["LeftNanobots"]).GetComponentsInChildren<CarrierNanobotController>();

        void SetupInterceptors()
        {
            var interceptorProjectile = new Projectile(Resources.Load<GameObject>("Prefabs/Bosses/Interceptor"))
            {
                damage = 5,
                isEnemy = true,
                owner = null,
            };

            var interceptorSpawn = transform.GetChild(_childIndices["InterceptorSpawn"]).transform.position;
            _interceptorWeapon = new GameObjectWeapon(gameObject, .5f, WeaponDirection.Backward, interceptorProjectile,
                spawnOffset: interceptorSpawn - transform.position, connectToParent: false);
        }
    }

    private List<AudioClip> _clips;

    internal void PlayDeathAnimation()
    {
        _disablePhases = true;
        var explosionParents = new List<Transform>();
        _clips = Enumerable.Range(7, 4)
            .Select(number => Resources.Load<AudioClip>("Sounds/Explosions/ES_Laser Cannon Explosion " + number))
            .ToList();

        if (new int[] { 7, 0, 1, 2, 3 }.Contains(_currentPhase))
        {
            DisableFrontWeapons();
            explosionParents.Add(transform.GetChild(_childIndices["ExplosionsFront"]));
        }
        if (new int[] { 3, 4, 5 }.Contains(_currentPhase))
        {
            DisableLeftWeapons();
            explosionParents.Add(transform.GetChild(_childIndices["ExplosionsLeft"]));
        }
        if (new int[] { 5, 6, 7 }.Contains(_currentPhase))
        {
            DisableBackWeapons();
            DisableEngines();
            transform.parent = AutoScroll.instance.transform;
            _rigidbody.constraints |= RigidbodyConstraints.FreezePositionZ;
            explosionParents.Add(transform.GetChild(_childIndices["ExplosionsBack"]));
            explosionParents.Add(transform.GetChild(_childIndices["ExplosionsLeft"]));
            explosionParents.Add(transform.GetChild(_childIndices["ExplosionsRight"]));
            explosionParents.Add(transform.GetChild(_childIndices["ExplosionsFront"]));
        }

        var explosions = explosionParents.SelectMany(
                explosionParent => Enumerable.Range(0, explosionParent.childCount)
                    .Select(i => explosionParent.GetChild(i).GetComponent<ParticleSystem>())
            ).ToArray();

        var explosionOrder = explosions.GetRandomValues(explosions.Length);
        GameController.GetPlayerWeaponController().Disable();
        GameController.GetPlayerMovementController().ResetPosition();

        StartCoroutine(EnableExplosions(explosionOrder));
    }

    private IEnumerator EnableExplosions(List<ParticleSystem> explosions)
    {
        var startTime = 0f;
        var delay = 2.5f / explosions.Count;
        var explosionIndex = 0;
        InvokeRepeating(nameof(PlayRandomExplosionSound), 0f, Math.Max(delay, .25f));

        while (explosionIndex < explosions.Count)
        {
            startTime += Time.deltaTime;

            if (explosionIndex * delay <= startTime)
            {
                var effect = explosions[explosionIndex++];
                effect.Play();
            }

            yield return null;
        }

        UIController.instance.ShowEndGamePopupInSeconds(3f);

        yield break;
    }

    private void PlayRandomExplosionSound()
    {
        if (UIController.instance.endGamePanel.gameObject.activeInHierarchy)
        {
            _audioSource.volume = 0;
            CancelInvoke(nameof(PlayRandomExplosionSound));
        }
        _audioSource.PlayOneShot(_clips.GetRandom());
    }

    private bool MovedOffScreen()
    {
        return transform.position.z - _length > EnemySpawner.instance.GetCorner(Corner.TopLeft).z;
    }

    private bool EnoughInterceptorsLeftCarrier()
    {
        if (_spawnedInterceptorCount < MaxInterceptorCount)
        {
            return false;
        }

        var lastInterceptor = _interceptors.LastOrDefault(i => i != null);

        if (lastInterceptor == null)
        {
            return true;
        }

        var interceptorPosition = lastInterceptor.transform.position;

        return lastInterceptor.direction == InterceptorDirection.Backward
            ? interceptorPosition.z < transform.position.z - _length / 2
            : interceptorPosition.x < transform.position.x - _length / 2;
    }

    private void SetupPhasePositions()
    {
        _positions = new Dictionary<int, (Vector3 Position, Vector3 LookAt)> {
            {0, (_spawnPoint, Vector3.back)},
            {1, (_spawnPoint + _length * 0.22f * Vector3.back, Vector3.back)},
            {3, (_spawnPoint + _length * 0.55f * Vector3.back + _length * 0.02f * Vector3.right, Vector3.left)},
            {5, (_spawnPoint + _length * 0.2f * Vector3.back + Vector3.up * 1.4f, Vector3.forward)},
        };
    }

    public new void Start()
    {
        base.Start();
        EnableShield();
    }

    public new void Update()
    {
        base.Update();
    }

    private void StartPhaseOne()
    {
        _spawnedInterceptorCount = 0;
        if (!_enemyController.isImmune)
        {
            EnableShield();
        }
        EnableFrontWeapons();
        SpawnShieldGenerators();
        _interceptorWeapon.spawnOffset = transform.GetChild(_childIndices["InterceptorSpawn"]).transform.position - transform.position;
    }

    private void StartPhaseTwo()
    {
        _spawnedInterceptorCount = 0;
        EnableLeftWeapons();
        _interceptorWeapon.spawnOffset = transform.GetChild(_childIndices["InterceptorSpawn"]).transform.position - transform.position;
    }

    private void SpawnShieldGenerators()
    {
        var generatorPrefab = Resources.Load<GameObject>("Prefabs/Bosses/CarrierShieldGenerator");

        var generators = Enumerable.Range(0, 2).Select(_ => Instantiate(generatorPrefab, transform.position, Quaternion.identity, parent: transform)).ToList();

        _shieldGenerators.AddRange(generators);

        EnemySpawner.instance.activeEnemies.AddRange(generators);

        generators.First().GetComponent<ShieldGeneratorController>().FlyTo(new Vector3(0.15f, 0.7f, 0).ViewportPointToSpawnPoint());
        generators.Last().GetComponent<ShieldGeneratorController>().FlyTo(new Vector3(0.85f, 0.7f, 0).ViewportPointToSpawnPoint());
    }

    public void RemoveGenerator(GameObject generator)
    {
        _shieldGenerators.Remove(generator);

        if (_shieldGenerators.Count == 0)
        {
            DisableShield();
        }
    }

    private void EnableShield()
    {
        _enemyController.isImmune = true;

        _shieldController.Enable();
    }

    private void DisableShield()
    {
        _enemyController.isImmune = false;

        _shieldController.Disable();
    }

    private void FirePhaseOneWeapons()
    {
        if (_shieldGenerators.Count == 0)
        {
            SpawnInterceptor(InterceptorDirection.Backward);
        }
    }

    private void SpawnInterceptor(InterceptorDirection direction)
    {
        if (_spawnedInterceptorCount >= MaxInterceptorCount)
        {
            return;
        }

        var interceptor = _interceptorWeapon.FireProjectiles<InterceptorController>();

        if (interceptor == null)
        {
            return;
        }

        EnemySpawner.instance.activeEnemies.Add(interceptor.gameObject);

        _interceptors.Add(interceptor);

        interceptor.SetDirection(direction);

        _spawnedInterceptorCount++;
    }

    private void FirePhaseTwoWeapons()
    {
        var playerXPos = GameController.GetPlayer().transform.position.x;

        if (playerXPos < 0 && !_nanobotWeapons[2].isFiring && !IsInvoking(nameof(SwitchToLeftSideNanobots)))
        {
            Invoke(nameof(SwitchToLeftSideNanobots), 1f);
        }

        if (playerXPos >= 0 && !_nanobotWeapons[0].isFiring && !IsInvoking(nameof(SwitchToRightSideNanobots)))
        {
            Invoke(nameof(SwitchToRightSideNanobots), 1f);
        }

        SpawnInterceptor(InterceptorDirection.Side);
    }

    private void SwitchToRightSideNanobots()
    {
        _nanobotWeapons[0].Enable();
        _nanobotWeapons[1].Enable();
        _nanobotWeapons[2].Disable();
    }

    private void SwitchToLeftSideNanobots()
    {
        _nanobotWeapons[0].Disable();
        _nanobotWeapons[1].Disable();
        _nanobotWeapons[2].Enable();
    }

    private void MoveToCurrentPhase()
    {
        var (desiredPosition, desiredLookAt) = _positions[_currentPhase];
        var (previousPosition, previousLookAt) = _positions.Last(x => x.Key < _currentPhase).Value;

        var startingRotation = Quaternion.LookRotation(previousLookAt, Vector3.up);
        var finishingRotation = Quaternion.LookRotation(desiredLookAt, Vector3.up);

        var phaseProgress = (Time.time - _phaseStartTime) / PhaseTransitionTime;

        transform.rotation = Quaternion.Lerp(startingRotation, finishingRotation, phaseProgress);
        transform.position = Vector3.Lerp(previousPosition, desiredPosition, phaseProgress);
    }

    private bool IsInPosition()
    {
        return transform.position == _positions[_currentPhase].Position;
    }

    private bool WaitedPhaseTime()
    {
        return _phaseStartTime + _phaseWaitTimes[_currentPhase] < Time.time;
    }

    private void EnableFrontWeapons()
    {
        var frontWeapons = transform.GetChild(_childIndices["FrontGuns"]);

        float delay = 0f;
        foreach (var weapon in frontWeapons.GetComponentsInChildren<CarrierGunController>())
        {
            weapon.EnableInSeconds(delay);

            delay += 0.5f;
        }
    }

    private void EnableLeftWeapons()
    {
        var rocketWeapons = transform.GetChild(_childIndices["LeftGuns"]);

        float delay = 0f;
        foreach (var weapon in rocketWeapons.GetComponentsInChildren<CarrierRocketController>())
        {
            weapon.EnableInSeconds(delay);

            delay += 2f;
        }
    }

    private void EnableBackWeapons()
    {
        AutoScroll.instance.StartScroll();

        _rigidbody.constraints &= ~RigidbodyConstraints.FreezePositionZ;
        _rigidbody.AddForce(Vector3.forward * 2, ForceMode.VelocityChange);
        _rigidbody.angularDrag = 10;

        var engineParent = transform.GetChild(_childIndices["Engines"]);

        var engines = engineParent.GetComponentsInChildren<CarrierLaserController>();

        engines.First().loopAfter = 2;
        engines.First().InitiateFire();
        engines.Last().loopAfter = 2;
        engines.Last().InitiateFire();

        engines.Skip(1).First().loopAfter = 1;
        engines.Skip(1).First().InitiateFireInSeconds(3);

        var backGuns = transform.GetChild(_childIndices["BackGuns"]);

        float delay = 0f;
        foreach (var weapon in backGuns.GetComponentsInChildren<CarrierHomingMissileController>())
        {
            weapon.EnableInSeconds(delay);

            delay += 0.25f;
        }
    }

    private void DisableBackWeapons()
    {
        var backGuns = transform.GetChild(_childIndices["BackGuns"]);
        foreach (var weapon in backGuns.GetComponentsInChildren<CarrierHomingMissileController>())
        {
            weapon.Disable();
        }
    }

    private void RestartPhases()
    {
        DisableBackWeapons();
        DisableEngines();

        AutoScroll.instance.HaltScroll();
    }

    private void DisableEngines()
    {
        var engines = transform.GetChild(_childIndices["Engines"]).GetComponentsInChildren<CarrierLaserController>();

        foreach (var engine in engines)
        {
            engine.Disable();
        }
    }

    private void ResetSpawn()
    {
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.constraints |= RigidbodyConstraints.FreezePositionZ;
        transform.position = EnemySpawner.instance.transform.position + _spawnPointOffset;
        _spawnPoint = transform.position;
        SetupPhasePositions();
    }

    private void DisableLeftWeapons()
    {
        var rocketWeapons = transform.GetChild(_childIndices["LeftGuns"]);

        foreach (var weapon in rocketWeapons.GetComponentsInChildren<CarrierRocketController>())
        {
            weapon.Disable();
        }

        foreach (var weapon in _nanobotWeapons)
        {
            weapon.Disable(final: true);
        }
    }

    private void DisableFrontWeapons()
    {
        var frontWeapons = transform.GetChild(_childIndices["FrontGuns"]);

        foreach (var weapon in frontWeapons.GetComponentsInChildren<CarrierGunController>())
        {
            weapon.Disable();
        }
    }

    public void OnEnemyDestroyed()
    {
        foreach (var interceptor in _interceptors.Where(i => i != null))
        {
            Destroy(interceptor.gameObject);
        }

    }
}
