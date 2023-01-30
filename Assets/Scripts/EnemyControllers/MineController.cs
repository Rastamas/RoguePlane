using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineController : MonoBehaviour
{
    public float damage = 30;

    private ParticleSystem _explosion;
    private ParticleSystem _electricity;
    private MeshRenderer _renderer;
    private CapsuleCollider _collider;

    public void Awake()
    {
        _collider = GetComponent<CapsuleCollider>();
        _renderer = GetComponent<MeshRenderer>();
        _electricity = transform.GetChild(0).GetComponent<ParticleSystem>();
        _explosion = transform.GetChild(1).GetComponent<ParticleSystem>();
    }

    public void Start()
    {
        GetComponent<EnemyController>().canCollideWithPlayer = false;
    }

    public void OnCollisionEnter(Collision collision)
    {
        _collider.enabled = false;
        _renderer.enabled = false;
        _electricity.Stop();
        _explosion.Play();
        SFXPlayer.PlayOneshot(SfxOneshot.Mine);

        var player = collision.collider.GetComponent<PlayerController>();

        if (player != null)
        {
            player.Hit(damage);
        }

        Destroy(gameObject, _explosion.main.duration);
    }
}
