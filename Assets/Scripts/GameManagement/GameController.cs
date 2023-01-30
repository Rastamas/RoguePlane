using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private static GameController _instance;

    public static GameController instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameController>();
            }

            return _instance;
        }
    }
    public GameObject playerPrefab;
    public readonly Vector3 playerSpawnOffset = new Vector3(0, 0, -8);
    public int killCount;
    public float timeStarted;
    public int coinCollected = 0;
    private AutoScroll _autoScroll;
    private EnemySpawner _enemySpawner;
    private RewardController _rewardController;
    private GameObject _player;
    private PlayerController _playerController;
    private PlayerWeaponController _playerWeaponController;
    private PlayerMovementController _playerMovementController;
    public static int currentEncounter = -1;

    public readonly List<(int index, EncounterSize size)> encounters = new List<(int index, EncounterSize size)>
    {
        (0, EncounterSize.Small),
        (1, EncounterSize.Medium),
        (2, EncounterSize.Medium),
        (3, EncounterSize.Large),
        (4, EncounterSize.Boss)
    };

    private Queue<(int index, EncounterSize size)> _encounterQueue;

    public void Awake()
    {
        Time.timeScale = 1;

        killCount = 0;
        _encounterQueue = new Queue<(int index, EncounterSize size)>(encounters);
        _autoScroll = AutoScroll.instance;
        _enemySpawner = EnemySpawner.instance;
        _rewardController = GetComponentInChildren<RewardController>();

        _player = Instantiate(playerPrefab, playerSpawnOffset + transform.position, Quaternion.identity, parent: transform);
        _playerController = _player.GetComponent<PlayerController>();
        _playerWeaponController = _player.GetComponent<PlayerWeaponController>();
        _playerMovementController = _player.GetComponent<PlayerMovementController>();
    }

    public void Start()
    {
        Application.targetFrameRate = Math.Min(60, Screen.currentResolution.refreshRate * 2);
        MobileAds.Initialize(initStatus =>
        {
            AdManager.Instance.Initialize();
        });
        Invoke(nameof(SpawnPowerup), 1);
        timeStarted = Time.time - 1;
    }

    private void SpawnPowerup()
    {
        _rewardController.InitiatePowerupSpawn();
    }

    public void FinishEncounter()
    {
        if (_encounterQueue.Count == 0)
        {
            return;
        }

        _autoScroll.HaltScroll();
        GetPlayerWeaponController().Disable();
        _rewardController.InitiatePowerupSpawn();
    }

    public void StartNewEncounter()
    {
        var (encounter, encounterSize) = _encounterQueue.Dequeue();
        GetPlayerWeaponController().Enable();

        if (encounterSize != EncounterSize.Boss)
        {
            _autoScroll.StartScroll();
        }
        else
        {
            _autoScroll.HaltScroll();
        }
        _enemySpawner.InitializeEncounter(encounterSize);
        currentEncounter = encounter;
    }

    public static GameObject GetPlayer()
    {
        return instance._player;
    }

    public static PlayerController GetPlayerController()
    {
        return instance._playerController;
    }

    public static PlayerWeaponController GetPlayerWeaponController()
    {
        return instance._playerWeaponController;
    }

    public static PlayerMovementController GetPlayerMovementController()
    {
        return instance._playerMovementController;
    }

    public void OnDestroy()
    {
        PermanentProgressionManager.IncreaseKillCount(killCount);
    }
}
