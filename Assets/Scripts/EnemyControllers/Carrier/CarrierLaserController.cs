using System;
using UnityEngine;

public class CarrierLaserController : MonoBehaviour
{
    private ParticleWeapon _weapon;
    private bool _isFiring;
    private float _beamshotDelay;
    private float _beamFiringTime;
    private float _beamLifetime;
    private Rigidbody _carrierBody;
    private AudioSource _audioSource;
    public int loopAfter;
    private int _fireCount;

    public void Awake()
    {
        var laserPrefab = Resources.Load<GameObject>("Prefabs/Projectiles/EngineLaser");
        var laserBeam = laserPrefab.transform.GetChild(laserPrefab.transform.childCount - 1).GetComponent<ParticleSystem>().main;

        _beamshotDelay = laserBeam.startDelay.constant;
        _beamFiringTime = laserBeam.duration;
        _beamLifetime = laserBeam.startLifetime.constant;

        _weapon = new ParticleWeapon(parent: gameObject, 1, WeaponDirection.Backward, laserPrefab, damage: 0.2f, isSingleShot: true);
        _carrierBody = transform.parent.parent.GetComponent<Rigidbody>();
        _audioSource = GetComponent<AudioSource>();
        _audioSource.volume *= PlayerPrefs.GetFloat(Constants.SfxVolumePlayerPrefKey, 1f);
    }

    public void FixedUpdate()
    {
        if (_isFiring)
        {
            _carrierBody.AddForce(Vector3.forward * _carrierBody.mass * 18f, ForceMode.Force);
        }
    }

    public void InitiateFireInSeconds(float seconds)
    {
        Invoke(nameof(InitiateFire), seconds);
    }

    public void InitiateFire()
    {
        var isLooping = _fireCount >= loopAfter;

        if (isLooping && !_weapon.particleSystem.main.loop)
        {
            foreach (var child in _weapon.particleSystemGameObject.GetComponentsInChildren<ParticleSystem>())
            {
                var particleSystem = child.main;
                particleSystem.loop = true;
            }
        }

        _weapon.StartEmission();
        _audioSource.Play();

        Invoke(nameof(AddVelocityToCarrier), _beamshotDelay);
    }

    private void AddVelocityToCarrier()
    {
        _isFiring = true;

        if (_fireCount < loopAfter)
        {
            Invoke(nameof(Restart), _beamLifetime);
        }

        _fireCount++;
    }

    public void Restart()
    {
        _isFiring = false;
        _weapon.StopEmission();
        _audioSource.Stop();

        Invoke(nameof(InitiateFire), _beamLifetime);
    }

    public void Disable()
    {
        _isFiring = false;
        _weapon.StopEmission();
        _fireCount = 0;
        CancelInvoke();
    }
}
