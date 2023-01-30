using System;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float weaponDamage;
    public float attacksPerSecond;
    public WeaponDirection direction;

    private ParticleWeapon _weapon;
    private AudioSource _audioSource;
    private static readonly Color defaultOutsideColor = new Color(1, 0.3443396f, 0.3570501f, 0.8235294f);
    private static readonly Color defaultInsideColor = new Color(1, 0.1650943f, 0.1650943f, 0.8235294f);
    private static readonly Color increasedDamageColor = new Color(.04f, .98f, .75f, 0.8235f);
    private static readonly Color lifestealColor = new Color(0.035f, 0.7176471f, 0f, 1f);

    public void Awake()
    {
        var savedGame = PermanentProgressionManager.savedGame;

        weaponDamage = savedGame.GetStat(StatType.Damage);
        attacksPerSecond = savedGame.GetStat(StatType.AttackSpeed);
    }

    public void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.volume *= PlayerPrefs.GetFloat(Constants.SfxVolumePlayerPrefKey, 1f);

        _weapon = new ParticleWeapon(parent: gameObject, attacksPerSecond, direction, projectilePrefab, damage: weaponDamage, isEnemy: false);

        _weapon.SetOutsideColorScheme(defaultOutsideColor);
        _weapon.SetInsideColorScheme(defaultInsideColor);
    }

    public void Enable()
    {
        _weapon.StartEmission();
    }

    public void Disable()
    {
        _weapon.StopEmission();
    }

    public void IncreaseAttackSpeed(float percentage)
    {
        var multiplier = percentage / 100 + 1;
        attacksPerSecond *= multiplier;
        _weapon.ChangeAttackSpeed(multiplier);
    }

    public void IncreaseAttackDamage(float percentage)
    {
        var multiplier = percentage / 100 + 1;
        weaponDamage *= multiplier;
        _weapon.ChangeAttackDamage(weaponDamage);
        _weapon.SetOutsideColorScheme(increasedDamageColor);
    }

    internal void AddLifesteal(float percentage)
    {
        _weapon.damagingParticleController.onEnemyHitCallback =
            (damage) => GameController.GetPlayerController().RestoreFlatHealth(damage * percentage);

        _weapon.SetInsideColorScheme(lifestealColor);
    }
}
