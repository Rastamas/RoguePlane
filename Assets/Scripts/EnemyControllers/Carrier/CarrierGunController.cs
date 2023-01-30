using UnityEngine;

public class CarrierGunController : MonoBehaviour
{
    private Projectile _projectile;
    private GameObjectWeapon _weapon;
    private AudioSource _audioSource;
    private bool _isEnabled;

    private const int weaponDamage = 3;
    private const float attacksPerSecond = 0.5f;

    public void Start()
    {
        _projectile = new Projectile(Resources.Load<GameObject>("Prefabs/Projectiles/Bullet"))
        {
            owner = gameObject,
            damage = weaponDamage,
            isEnemy = true
        };

        _weapon = new GameObjectWeapon(parent: gameObject, attacksPerSecond, WeaponDirection.Backward, _projectile);
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.clip = Resources.Load<AudioClip>("Sounds/Explosions/ES_Laser Gun Fire 11");
    }

    public void Update()
    {
        if (!_isEnabled)
        {
            return;
        }

        transform.LookAt(GameController.GetPlayer().transform);

        var projectile = _weapon.FireProjectiles<BulletController>(transform, GameController.GetPlayer().transform);

        if (projectile != null)
        {
            _audioSource.PlayOneShot(_audioSource.clip, .25f * PlayerPrefs.GetFloat(Constants.SfxVolumePlayerPrefKey, 1f));
        }
    }

    public void EnableInSeconds(float seconds)
    {
        Invoke(nameof(Enable), seconds);
    }

    public void Enable()
    {
        _isEnabled = true;
    }

    public void Disable()
    {
        _isEnabled = false;
    }
}
