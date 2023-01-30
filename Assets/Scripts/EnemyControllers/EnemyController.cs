using System;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public int score;
    public float maxHealth;
    public float health;
    public Action<GameObject, int> onDestroyCallback;
    public bool isImmune;
    public bool canCollideWithPlayer = true;

    private bool _destroyedByPlayer;
    private FlashController _flashController;
    private CarrierController _carrierController;

    public void Awake()
    {
        if (maxHealth == 0)
        {
            throw new ArgumentException(nameof(maxHealth) + " can not be null");
        }

        if (PermanentProgressionManager.IsChallengeEnabed(Challenge.Beefy))
        {
            maxHealth *= 2;
        }
        _destroyedByPlayer = false;
        health = maxHealth;
        _flashController = GetComponent<FlashController>();
        _carrierController = GetComponent<CarrierController>();
    }

    public float Hit(float damage)
    {
        if (isImmune || damage == 0)
        {
            return 0;
        }

        health -= damage;
        if (health <= 0)
        {
            var onDestroyController = GetComponent<IEnemyDestroyed>();

            if (onDestroyController != null)
            {
                onDestroyController.OnEnemyDestroyed();
            }

            _destroyedByPlayer = true;

            if (_carrierController == null)
            {
                Destroy(gameObject);
            }
            else
            {
                isImmune = true;

                _carrierController.PlayDeathAnimation();
            }

            return damage + health;
        }

        _flashController?.Flash(health / maxHealth);

        return damage;
    }

    public void OnCollisionEnter(Collision collision)
    {
        var collider = collision.collider;

        if (collider.tag == Constants.PlayerTag && canCollideWithPlayer)
        {
            canCollideWithPlayer = false;
            _destroyedByPlayer = false;

            var dropController = GetComponent<DropController>();
            if (dropController != null)
            {
                dropController.Disable();
            }

            var damage = GameController.GetPlayerController().Collide(damage: maxHealth);
            this.Hit(damage);
            Invoke(nameof(ResetCollision), 0.1f);
        }
    }

    private void ResetCollision()
    {
        canCollideWithPlayer = true;
    }

    public void OnDestroy()
    {
        if (!gameObject.scene.isLoaded)
        {
            return;
        }

        if (_destroyedByPlayer)
        {
            GameController.instance.killCount++;
        }

        var scoreToAward = _destroyedByPlayer
            ? score
            : 0;

        onDestroyCallback?.Invoke(gameObject ?? null, scoreToAward);
    }
}
