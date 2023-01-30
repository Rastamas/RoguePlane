using System.Collections.Generic;
using System.Linq;
using GooglePlayGames;
using UnityEngine;
using UnityEngine.VFX;

public class TeslaCoil : MonoBehaviour
{
    private bool _enabled;
    private float _lastFireTime;
    private float _damage;
    private const float attacksPerSecond = 1;
    private const float duration = 0.25f;
    private const float range = 10f;
    private VisualEffect _vfx;
    private AudioSource _audioSource;
    private List<(float time, GameObject gameObject)> _lightnings = new List<(float, GameObject)>();

    public void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _vfx = GetComponentInChildren<VisualEffect>();
        _vfx.gameObject.SetActive(false);
    }

    public void Update()
    {
        if (!_enabled || Time.time < _lastFireTime + 1 / attacksPerSecond)
        {
            return;
        }

        var filterActiveEnemies = false;

        var enemiesHit = 0;

        foreach (var enemy in EnemySpawner.instance.activeEnemies)
        {
            if (enemy.gameObject == null)
            {
                filterActiveEnemies = true;
                continue;
            }

            var vectorToEnemy = enemy.gameObject.transform.position - transform.position;

            if (vectorToEnemy.magnitude > range)
            {
                continue;
            }

            enemy.GetComponent<EnemyController>().Hit(_damage);

            if (enemy.GetComponent<CarrierController>() != null)
            {
                Social.ReportProgress(GPGSIds.achievement_i_feel_something_a_slight_tingle_in_my_generator
                    , 100.0d, (_) => { });
            }

            enemiesHit++;
            PlayLightningVfx(vectorToEnemy);
        }

        if (enemiesHit > 0)
        {
            _audioSource.PlayOneShot(_audioSource.clip,
             volumeScale: PlayerPrefs.GetFloat(Constants.SfxVolumePlayerPrefKey, 1f) * (.5f + enemiesHit / 10));
        }

        if (enemiesHit >= 10)
        {
            Social.ReportProgress(GPGSIds.achievement_shocking, 100.0d, (_) => { });
        }

        if (filterActiveEnemies)
        {
            EnemySpawner.instance.activeEnemies = EnemySpawner.instance.activeEnemies.Where(x => x != null).ToList();
        }

        _lastFireTime = Time.time;
        CleanupOldLightnings();
    }

    private void PlayLightningVfx(Vector3 vectorToEnemy)
    {
        var lightning = Instantiate(_vfx.gameObject, _vfx.transform.position + vectorToEnemy / 2, Quaternion.LookRotation(vectorToEnemy), transform);

        _lightnings.Add((Time.time, lightning));

        var vfx = lightning.GetComponent<VisualEffect>();

        vfx.SetFloat("Duration", duration);
        vfx.SetFloat("Length", vectorToEnemy.magnitude / 2);
        vfx.SendEvent("OnPlay");
    }

    private void CleanupOldLightnings()
    {
        foreach (var lightning in _lightnings)
        {
            if (lightning.time < Time.time - duration)
            {
                Destroy(lightning.gameObject);
            }
            else
            {
                break;
            }
        }

        _lightnings = _lightnings.Where(l => l.gameObject != null).ToList();
    }

    public void Enable(float damage)
    {
        _enabled = true;
        _damage = damage;
        _lastFireTime = Time.time - 1 / attacksPerSecond;
        _vfx.gameObject.SetActive(true);
    }
}
