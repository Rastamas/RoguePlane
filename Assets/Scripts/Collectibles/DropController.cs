using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropController : MonoBehaviour
{
    [SerializeField]
    private float _dropChancePercentage;

    private GameObject _coinPrefab;
    private bool _isQuitting;
    private bool _isEnabled;

    public void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    public void Awake()
    {
        _isEnabled = true;
        _coinPrefab = Resources.Load<GameObject>("Prefabs/Collectibles/Coin");
    }

    public void OnDestroy()
    {
        if (ShouldNotDrop)
        {
            return;
        }

        var dropPosition = new Vector3(
            gameObject.transform.position.x,
            EnemySpawner.playingLevel, // Make sure coin is dropped on a pickable level (i.e. for Pez)
            gameObject.transform.position.z
        );

        Instantiate(_coinPrefab, dropPosition, Quaternion.identity);
    }

    public void Disable()
    {
        _isEnabled = false;
    }

    private bool ShouldNotDrop => _isQuitting ||
        !_isEnabled ||
        !gameObject.scene.isLoaded ||
        Random.Range(0, 100) >= DropChangePercentage;

    private float DropChangePercentage => _dropChancePercentage *
        RewardController.instance.coinDropChanceMultiplier;
}
