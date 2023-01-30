using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityRandom = UnityEngine.Random;
using UnityEngine.AI;
using Unity.Mathematics;

public class EnemySpawner : MonoBehaviour
{
    public float initialSpawnFrequencyInMs;
    private static EnemySpawner _instance;
    public static EnemySpawner instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<EnemySpawner>();
            }

            return _instance;
        }
    }

    public static float playingLevel => instance.spawnPlane.transform.position.y;

    public BoxCollider spawnPlane;
    public List<GameObject> activeEnemies;
    public EncounterSize currentEncounterSize;
    public float spawnZOffset;
    public Dictionary<EnemyType, GameObject> enemyPrefabs;
    private EnemyType[] _enemyTypes;
    private Queue<EnemySquad> _spawnQueue;
    private int _killedEnemyScore;
    public readonly Dictionary<EncounterSize, int> encounterSizes = new Dictionary<EncounterSize, int>()
    {
        { EncounterSize.Test, 1 },
        { EncounterSize.Small, 500 },
        { EncounterSize.Medium, 1000 },
        { EncounterSize.Large, 1500 },
        { EncounterSize.Boss, 1 },
    };
    private float _spawnFrequencyInMs;
    private float _lastSpawnTime;
    private float _spawnXStart;
    private float _spawnXEnd;
    private float _laneWidth;
    private bool _spawnMines;
    private GameObject _minePrefab;
    private Dictionary<Corner, Vector3> _cornerOffsets;
    private List<EnemyType> _allowedEnemies;
    private Dictionary<int, Lane> _lanes;
    private AudioSource _audioSource;
    private AudioClip _explosionAduio;
    private float _lastHitSoundTime = 0;
    private const int SpawnLaneCount = 7;
    private const int EncounterSafetyMultiplier = 100;
    private const float hitSoundCooldown = .2f;

    public void Awake()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.volume = .2f * PlayerPrefs.GetFloat(Constants.SfxVolumePlayerPrefKey, 1f);
        _explosionAduio = Resources.Load<AudioClip>("Sounds/Explosions/ES_Firework Boom");
        _spawnFrequencyInMs = initialSpawnFrequencyInMs;
        _spawnMines = PermanentProgressionManager.IsChallengeEnabed(Challenge.Explosive);
        spawnPlane = GetComponent<BoxCollider>();

        activeEnemies = new List<GameObject>();
        _spawnQueue = new Queue<EnemySquad>();

        _allowedEnemies = new List<EnemyType> {
            EnemyType.Assasin,
            EnemyType.Swarmy,
            EnemyType.Normie,
            EnemyType.Tilter,
            EnemyType.Pez,
            EnemyType.Bulky,
        };

        _enemyTypes = EnumExtensions.GetValues<EnemyType>().Where(e => _allowedEnemies.Contains(e)).ToArray();
        _cornerOffsets = new Dictionary<Corner, Vector3>();

        enemyPrefabs = Resources.LoadAll<GameObject>("Prefabs/Enemies").ToDictionary(
            p => Enum.TryParse<EnemyType>(p.name, out var parsedType)
                ? parsedType
                : throw new NotImplementedException($"EnemyType at path {p.name} is not among the known enum values"),
            p => p);

        _minePrefab = Resources.Load<GameObject>("Prefabs/Explosions/Mine");
    }

    public void Start()
    {
        DetermineSpawnBounds();
    }

    public void Update()
    {
        if (Time.time - _lastSpawnTime < _spawnFrequencyInMs / 1000 ||
            currentEncounterSize == EncounterSize.None ||
            _killedEnemyScore >= encounterSizes[currentEncounterSize] ||
            _spawnQueue.Count == 0)
        {
            return;
        }

        bool spawnSuccessful;
        var attempts = 0;
        do
        {
            var enemyToSpawn = _spawnQueue.Dequeue();

            spawnSuccessful = SpawnEnemy(enemyToSpawn, attempts++);
        } while (!spawnSuccessful);

        _lastSpawnTime = Time.time;
    }

    public void InitializeEncounter(EncounterSize encounterSize)
    {
        currentEncounterSize = encounterSize;
        _killedEnemyScore = 0;
        _spawnQueue = encounterSize == EncounterSize.Boss
            ? GetBoss()
            : GetRandomEnemies(encounterSize);

        _lastSpawnTime = Time.time;
    }

    public Vector3 GetCorner(Corner corner)
    {
        return _cornerOffsets[corner] + transform.position;
    }

    private Queue<EnemySquad> GetRandomEnemies(EncounterSize encounterSize)
    {
        var enemyQueue = new Queue<EnemySquad>();
        var sumHealthOfEnemiesInEncounter = encounterSizes[encounterSize];
        var amountOfEnemiesToPrepare = sumHealthOfEnemiesInEncounter * EncounterSafetyMultiplier;

        do
        {
            var enemyType = GetRandomEnemyType();
            var squad = enemyType.ToSquad();

            enemyQueue.Enqueue(squad);

            amountOfEnemiesToPrepare -= squad.Value;
        } while (amountOfEnemiesToPrepare > 0);

        return enemyQueue;
    }

    private EnemyType GetRandomEnemyType()
    {
        return _enemyTypes[UnityRandom.Range(0, _enemyTypes.Length)];
    }

    private void DetermineSpawnBounds()
    {
        // Cast rays in the corners of the screen to determine the X bounds where the enemies can spawn
        var topLeftCorner = new Vector3(0, 1, 0).ViewportPointToSpawnPoint();
        var topRightCorner = new Vector3(1, 1, 0).ViewportPointToSpawnPoint();
        var bottomLeftCorner = new Vector3(0, 0, 0).ViewportPointToSpawnPoint();
        var bottomRightCorner = new Vector3(1, 0, 0).ViewportPointToSpawnPoint();

        _cornerOffsets[Corner.BottomLeft] = bottomLeftCorner - transform.position;
        _cornerOffsets[Corner.TopLeft] = topLeftCorner - transform.position;
        _cornerOffsets[Corner.TopRight] = topRightCorner - transform.position;
        _cornerOffsets[Corner.BottomRight] = bottomRightCorner - transform.position;

        _spawnXStart = topLeftCorner.x;
        _spawnXEnd = topRightCorner.x;
        spawnZOffset = topLeftCorner.z - transform.position.z;

        _laneWidth = (_spawnXEnd - _spawnXStart) / SpawnLaneCount;

        _lanes = Enumerable.Range(0, SpawnLaneCount).ToDictionary(x => x, x => new Lane(_spawnXStart + (0.5f + x) * _laneWidth));

        _lanes.Add(_lanes.Count, new Lane(-1, LaneType.Assasin));
        _lanes.Add(_lanes.Count, new Lane(-2, LaneType.Tilter));
    }

    private Queue<EnemySquad> GetBoss()
    {
        var bossPrefab = Resources.Load<GameObject>("Prefabs/Bosses/Carrier");

        return new Queue<EnemySquad>(new EnemySquad[1] { new EnemySquad {
            type = EnemyType.Boss,
            size = bossPrefab.GetComponent<NavMeshObstacle>().size * bossPrefab.transform.localScale.z,
            enemies = new List<EnemySpawn>{
                new EnemySpawn {
                    prefab = bossPrefab,
                    positionOffset = Vector3.zero
                }
        }}});
    }

    private bool SpawnEnemy(EnemySquad enemySquad, int attempt)
    {
        var enemySize = enemySquad.size;

        var chosenLane = enemySquad.type switch
        {
            EnemyType.Assasin => _lanes.First(l => l.Value.laneType == LaneType.Assasin).Value,
            EnemyType.Tilter => _lanes.First(l => l.Value.laneType == LaneType.Tilter).Value,
            _ => GetRandomLane(enemySquad)
        };

        if (chosenLane.isOccupied && attempt < 4)
        {
            return false;
        }

        var spawnXCoord = enemySquad.type == EnemyType.Boss
            ? transform.position.x
            : chosenLane.xCoordinate;

        var spawnPoint = new Vector3(spawnXCoord, spawnPlane.transform.position.y, transform.position.z + spawnZOffset + enemySize.z / 2);

        var spawnedEnemies = enemySquad.enemies.Select(e => Instantiate(e.prefab, spawnPoint + e.positionOffset, Quaternion.AngleAxis(180, Vector3.up)));

        foreach (var spawnedEnemy in spawnedEnemies)
        {
            spawnedEnemy.GetComponent<EnemyController>().onDestroyCallback = EnemyDestroyed;

            var enemyWeapon = spawnedEnemy.GetComponent<WeaponController>();

            if (enemyWeapon != null)
            {
                enemyWeapon.Enable();
            }
            activeEnemies.Add(spawnedEnemy);

            var occupiedLane = enemySquad.type switch
            {
                EnemyType.Assasin => chosenLane,
                EnemyType.Tilter => chosenLane,
                _ => _lanes.FirstOrDefault(lane => lane.Value.xCoordinate - _laneWidth / 2 > spawnedEnemy.transform.position.x
                || lane.Key == _lanes.Last(l => l.Value.laneType == LaneType.Generic).Key - 1).Value
            };

            occupiedLane.enemies.Add(spawnedEnemy);
        }

        _spawnFrequencyInMs *= 0.99f;

        if (enemySquad.type == EnemyType.Boss)
        {
            BackgroundMusicController.instance.SwitchToMusic(MusicType.Boss);
        }

        return true;
    }

    private Lane GetRandomLane(EnemySquad enemySquad)
    {
        var widthInLanes = Mathf.CeilToInt(enemySquad.size.x / _laneWidth);

        var availableLanes = _lanes.Where(l => !l.Value.isOccupied && l.Value.laneType == LaneType.Generic).ToList();

        availableLanes = availableLanes
            .Where(lane => Enumerable.Range(lane.Key, widthInLanes).All(laneKey => availableLanes.Any(a => a.Key == laneKey))).ToList();


        if (!availableLanes.Any())
        {
            availableLanes = _lanes.Where(l => l.Value.laneType == LaneType.Generic && l.Key < SpawnLaneCount - widthInLanes).ToList();
        }

        return availableLanes.Any()
            ? availableLanes.GetRandom().Value
            : _lanes[UnityRandom.Range(0, SpawnLaneCount)];
    }

    private void EnemyDestroyed(GameObject gameObject, int scoreToAward)
    {
        _killedEnemyScore += scoreToAward;

        PlayExplosionSFX(gameObject);

        if (_spawnMines && gameObject.GetComponent<MineController>() == null)
        {
            var mine = Instantiate(_minePrefab, gameObject.transform.position, Quaternion.identity);

            mine.GetComponent<EnemyController>().onDestroyCallback = EnemyDestroyed;
            activeEnemies.Add(mine);
        }

        UIController.instance.progressBar.UpdateProgress(scoreToAward);
        UIController.instance.superActivator.IncreaseFill((float)scoreToAward / encounterSizes[EncounterSize.Medium]);

        if (gameObject == null)
        {
            activeEnemies = activeEnemies.Where(x => x != null).ToList();
        }
        else
        {
            activeEnemies.Remove(gameObject);
        }

        if (activeEnemies.Count == 0 && (_killedEnemyScore >= encounterSizes[currentEncounterSize] || _spawnQueue.Count == 0))
        {
            GameController.instance.FinishEncounter();
        }
    }

    private void PlayExplosionSFX(GameObject gameObject)
    {
        var playExplosion = (!_spawnMines || gameObject.GetComponent<MineController>() == null) &&
            Time.time - _lastHitSoundTime > hitSoundCooldown;
        if (playExplosion)
        {
            _audioSource.PlayOneShot(_explosionAduio);
            _lastHitSoundTime = Time.time;
        }
    }
}
