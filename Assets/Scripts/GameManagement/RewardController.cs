using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RewardController : MonoBehaviour
{
    private static RewardController _instance;
    public static RewardController instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<RewardController>();
            }

            return _instance;
        }
    }

    public float coinDropChanceMultiplier;
    public List<PowerUpType> chosenPowerups;
    private PlayerController _player;
    private PlayerMovementController _playerMovement;
    private bool _spawningInProgress;
    private const int rewardCount = 4;

    public void Start()
    {
        coinDropChanceMultiplier = 1;
        chosenPowerups = new List<PowerUpType>();
        _player = GameController.GetPlayerController();
        _playerMovement = GameController.GetPlayer().GetComponent<PlayerMovementController>();
    }

    private bool IsPowerupAvailable(PowerUpType powerup)
    {
        return powerup switch
        {
            PowerUpType.Health => _player.health < _player.maxHealth,
            _ => !chosenPowerups.Contains(powerup)
        };
    }

    public void Update()
    {
        if (!_spawningInProgress)
        {
            return;
        }

        if (_playerMovement.IsInStartingPosition())
        {
            SpawnPowerups();

            _spawningInProgress = false;
        }

        if (!_playerMovement.resetInProgress)
        {
            _playerMovement.ResetPosition();
        }
    }

    public void SpawnPowerups()
    {
        GameController.GetPlayerController().ToggleSuper(forceOff: true);
        UIController.instance.powerupPanel.gameObject.SetActive(true);

        var powerups = Enum.GetValues(typeof(PowerUpType)).Cast<PowerUpType>()
            .Where(IsPowerupAvailable)
            .ToList()
            .GetRandomValues(rewardCount);

        UIController.instance.powerupPanel.powerupController.AddPowerupOptions(powerups);
    }

    public void InitiatePowerupSpawn()
    {
        UIController.instance.progressBar.PowerupReached();
        _spawningInProgress = true;
    }
}
