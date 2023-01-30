using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GooglePlayGames;

public class PowerupOption : MonoBehaviour
{
    private static readonly Dictionary<PowerUpType, string> _powerupDescriptions = new Dictionary<PowerUpType, string>
    {
        {PowerUpType.AttackSpeed, "Increase your attack speed by {0}%"},
        {PowerUpType.Health, "Restore {0}% health"},
        {PowerUpType.Damage, "Increase your attack damage by {0}%"},
        {PowerUpType.Shield, "Gain a shield that protects from one attack every {0} second"},
        {PowerUpType.Lifesteal, "Gain a small % of damage dealt as health"},
        {PowerUpType.Armor, "Reduce incoming damage by {0} (to a minimum of 1)"},
        {PowerUpType.TeslaCoil, "Deal {0} damage to nearby enemies"},
        {PowerUpType.Minter, "Increase the chance of enemies dropping coins by {0}%"},
        {PowerUpType.Size, "Increase your max health by {0}% but also increase your size"},
    };

    private static readonly Dictionary<PowerUpType, float> _powerupVariables = new Dictionary<PowerUpType, float>
    {
        {PowerUpType.AttackSpeed, 50},
        {PowerUpType.Damage, 50},
        {PowerUpType.Health, 50},
        {PowerUpType.Shield, 10f},
        {PowerUpType.Lifesteal, 0.05f},
        {PowerUpType.Armor, 5},
        {PowerUpType.TeslaCoil, 8},
        {PowerUpType.Minter, 100},
        {PowerUpType.Size, 100},
    };

    private static readonly Dictionary<PowerUpType, string> _powerupNames = new Dictionary<PowerUpType, string>
    {
        {PowerUpType.AttackSpeed, "Weapon Pre-Igniter"},
        {PowerUpType.Damage, "Superheated Particles"},
        {PowerUpType.Health, "Emergency Repairs"},
        {PowerUpType.Shield, "Deflector Shield"},
        {PowerUpType.Lifesteal, "Scrap Repurposing"},
        {PowerUpType.Armor, "Neosteel Plating"},
        {PowerUpType.TeslaCoil, "Unstable Tesla Coil"},
        {PowerUpType.Minter, "Rare Mineral Extractor"},
        {PowerUpType.Size, "Material Replicator"},
    };

    private TextMeshProUGUI title;
    private TextMeshProUGUI description;
    private Image icon;
    private Button iconButton;
    private Button overlayButton;

    public void Awake()
    {
        title = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        description = title.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        icon = transform.GetChild(2).GetComponent<Image>();
        iconButton = icon.GetComponent<Button>();
        overlayButton = GetComponent<Button>();
    }

    public void Setup(PowerUpType powerUpType)
    {
        title.text = _powerupNames[powerUpType];
        description.text = string.Format(_powerupDescriptions[powerUpType], _powerupVariables[powerUpType]);

        overlayButton.onClick.AddListener(delegate { OnPowerupChosen(powerUpType); });
        iconButton.onClick.AddListener(delegate { OnPowerupChosen(powerUpType); });

        icon.sprite = Resources.Load<Sprite>("Images/" + powerUpType.ToString());
    }

    private void OnPowerupChosen(PowerUpType powerUpType)
    {
        switch (powerUpType)
        {
            case PowerUpType.AttackSpeed:
                GameController.GetPlayerWeaponController().IncreaseAttackSpeed(percentage: _powerupVariables[powerUpType]);
                break;
            case PowerUpType.Damage:
                GameController.GetPlayerWeaponController().IncreaseAttackDamage(percentage: _powerupVariables[powerUpType]);
                break;
            case PowerUpType.Lifesteal:
                GameController.GetPlayerWeaponController().AddLifesteal(percentage: _powerupVariables[powerUpType]);
                break;
            case PowerUpType.Health:
                GameController.GetPlayerController().RestoreHealth(percentage: _powerupVariables[powerUpType]);
                break;
            case PowerUpType.Shield:
                GameController.GetPlayerController().EnableShield(cooldown: _powerupVariables[powerUpType]);
                break;
            case PowerUpType.TeslaCoil:
                GameController.GetPlayerController().EnableTeslaCoil(damage: _powerupVariables[powerUpType]);
                break;
            case PowerUpType.Armor:
                GameController.GetPlayerController().AddArmor(_powerupVariables[powerUpType]);
                break;
            case PowerUpType.Minter:
                RewardController.instance.coinDropChanceMultiplier += _powerupVariables[powerUpType] / 100;
                break;
            case PowerUpType.Size:
                GameController.GetPlayerController().IncreaseMaxHealth(_powerupVariables[powerUpType]);
                break;
            default: throw new NotImplementedException($"Unknown PowerupType: {powerUpType}");
        }

        RewardController.instance.chosenPowerups.Add(powerUpType);

        SFXPlayer.PlayOneshot(SfxOneshot.Powerup);

        UIController.instance.powerupPanel.powerupController.Clear();

        Time.timeScale = 1;

        GooglePlay.SafeIncrementEvent(GPGSIds.event_powerup_chosen, 1);
    }
}
