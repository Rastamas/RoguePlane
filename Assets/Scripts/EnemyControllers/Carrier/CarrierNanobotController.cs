using UnityEngine;

public class CarrierNanobotController : MonoBehaviour
{
    public bool isFiring;
    private ParticleWeapon _weapon;
    private AudioSource _audioSource;
    private bool _isEnabled;
    private bool _shouldUnpauseAudio;

    public void Awake()
    {
        var nanobotPrefab = Resources.Load<GameObject>("Prefabs/Projectiles/Nanobot");

        _weapon = new ParticleWeapon(parent: gameObject, float.MaxValue, WeaponDirection.Forward, nanobotPrefab, damage: 0.1f);

        transform.LookAt(transform.position + Vector3.left);

        _isEnabled = true;
        _audioSource = GetComponent<AudioSource>();

        if (_audioSource != null)
        {
            PausePanel.instance.OnPauseChanged += PauseAudio;
        }
    }

    private void PauseAudio(object sender, bool pausing)
    {
        if (pausing && _audioSource.isPlaying)
        {
            _audioSource.Pause();
            _shouldUnpauseAudio = true;
        }

        if (!pausing && _shouldUnpauseAudio)
        {
            _audioSource.Play();
        }
    }

    public void Update()
    {
        if (!_isEnabled || !isFiring)
        {
            return;
        }

        var playerPosition = GameController.GetPlayer().transform.position;

        _weapon.particleSystemGameObject.transform.LookAt(new Vector3(transform.position.x, playerPosition.y, playerPosition.z));
    }

    public void Enable()
    {
        if (_weapon.particleSystemGameObject == null)
        {
            _weapon.Reinstantiate();
            _isEnabled = true;
        }

        if (!_isEnabled)
        {
            return;
        }

        isFiring = true;
        _weapon.StartEmission();
        if (_audioSource != null)
        {
            _audioSource?.Play();
        }
    }

    public void Disable(bool final = false)
    {
        if (_weapon.particleSystemGameObject == null)
        {
            _weapon.Reinstantiate();
            _isEnabled = true;
        }

        if (final)
        {
            _weapon.DetachParticleSystem();
            _isEnabled = false;
        }

        isFiring = false;
        _weapon.StopEmission();
        if (_audioSource != null)
        {
            _audioSource?.Stop();
        }
    }
}
