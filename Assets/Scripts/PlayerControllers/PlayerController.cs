using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float health;
    public float maxHealth;
    public Material armoredMaterial;
    public Material transparentMaterial;
    public ParticleSystem missiles;
    public float lastHitTime;
    public bool damageTaken;
    public bool isDead;

    private bool _isImmune;
    private float _armor;
    private Material _originalMaterial;
    private MeshRenderer _renderer;
    private ShieldController _shieldController;
    private TeslaCoil _teslaCoil;

    public void Awake()
    {
        Vibration.Init();
        var savedGame = PermanentProgressionManager.savedGame;

        maxHealth = health = (int)savedGame.GetStat(StatType.Health);

        var missilePrefab = Resources.Load<GameObject>("Prefabs/Projectiles/Missiles");

        var missiles = Instantiate(missilePrefab, transform.position + Vector3.forward * 2, Quaternion.identity, parent: transform);
        health = maxHealth;

        this.missiles = missiles.GetComponent<ParticleSystem>();
        _renderer = GetComponent<MeshRenderer>();
        _shieldController = GetComponentInChildren<ShieldController>();
        _shieldController.enabled = false;
        _teslaCoil = GetComponentInChildren<TeslaCoil>();

        _originalMaterial = _renderer.material;
    }

    public void Start()
    {
        UIController.instance.ChangeHealth(health / maxHealth);
        lastHitTime = -1;
    }

    public void ToggleSuper(bool forceOff = false)
    {
        if (Time.timeScale == 0 || UIController.instance.powerupPanel.gameObject.activeInHierarchy)
        {
            return;
        }

        if (missiles.isEmitting || forceOff)
        {
            missiles.Stop();
        }
        else
        {
            missiles.Play();
        }
    }

    public void Hit(float damage)
    {
        if (_isImmune || health <= 0)
        {
            return;
        }

        if (_shieldController.shieldIsOn)
        {
            _shieldController.TurnOffShield();
            return;
        }

        var finalDamage = Math.Max(1, damage - _armor);

        damageTaken = true;

        if (PlayerPrefExtensions.GetBool(Constants.VibratePlayerPrefKey, defaultValue: true))
        {
            Vibration.Vibrate(GetVibrationTime(finalDamage));
        }

        ChangeHealth(health - finalDamage);
        lastHitTime = Time.time;

        if (health <= 0)
        {
            isDead = true;
            GameController.GetPlayerMovementController().DisableMovement();
            UIController.instance.ShowEndGamePopupInSeconds();
        }
    }

    private long GetVibrationTime(float finalDamage) => Math.Max(Math.Min(25, (long)finalDamage), 10) * 10;

    public float Collide(float damage)
    {
        if (_isImmune)
        {
            return 0;
        }

        var clampedDamage = Math.Min(damage, maxHealth / 4);

        Hit(clampedDamage);

        if (health > 0)
        {
            SetTemporaryImmunity();
        }


        return clampedDamage;
    }

    public void SetTemporaryImmunity(int duration = 1)
    {
        SetImmunity();

        Invoke(nameof(RemoveImmunity), duration);
    }

    public void SetImmunity()
    {
        _isImmune = true;
        _renderer.material = transparentMaterial;
        gameObject.layer = Constants.ImmuneLayer;
    }

    public void RemoveImmunity()
    {
        _isImmune = false;
        _renderer.material = _originalMaterial;
        gameObject.layer = Constants.PlayerLayer;
    }

    internal void Revive()
    {
        isDead = false;
        RestoreHealth(percentage: 100);
        SetTemporaryImmunity(duration: 2);
    }

    public void RestoreFlatHealth(float amount)
    {
        ChangeHealth(health + amount);
    }

    public void RestoreHealth(float percentage)
    {
        ChangeHealth(Math.Max(0, health) + maxHealth * percentage / 100);
    }

    private void ChangeHealth(float newHealth)
    {
        if (isDead)
        {
            return;
        }

        health = Math.Min(newHealth, maxHealth);
        UIController.instance.ChangeHealth(health / maxHealth);
    }

    public void EnableShield(float cooldown)
    {
        _shieldController.enabled = true;
        _shieldController.Enable(cooldown);
    }

    internal void AddArmor(float armor)
    {
        this._armor = armor;

        _renderer.material = armoredMaterial;
        _originalMaterial = _renderer.material;

    }

    internal void EnableTeslaCoil(float damage)
    {
        _teslaCoil.Enable(damage);
    }

    internal void IncreaseMaxHealth(float percentage)
    {
        var healthIncrease = maxHealth * percentage / 100;

        maxHealth += healthIncrease;
        health += healthIncrease;

        transform.localScale *= 1 + percentage / 500;
    }
}
