using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CoinController : MonoBehaviour
{
    /// First one is the lowest value, last one is the highest value
    public List<Material> materials;
    private int _value;
    private bool _isConsumed;
    private MeshRenderer _renderer;
    private static readonly Dictionary<EncounterSize, List<int?>> _coinValuePercentages = new Dictionary<EncounterSize, List<int?>>
    {
        {EncounterSize.Test, new List<int?> {100}},
        {EncounterSize.Small, new List<int?> {70, 90, 100}},
        {EncounterSize.Medium, new List<int?> {40, 80, 100}},
        {EncounterSize.Large, new List<int?> {20, 50, 100}},
        {EncounterSize.Boss, new List<int?> {100}},
    };

    private static readonly Dictionary<int, int> _coinIndexValues = new Dictionary<int, int>
    {
        {0, 1},
        {1, 3},
        {2, 5},
    };

    public void Awake()
    {
        _isConsumed = false;
        _renderer = GetComponent<MeshRenderer>();

        _value = GetValue();

        _renderer.material = materials[_coinIndexValues.First(kvp => kvp.Value == _value).Key];
    }

    private int GetValue()
    {
        var randomValue = UnityEngine.Random.value * 100f;
        var percentages = _coinValuePercentages[EnemySpawner.instance.currentEncounterSize];

        var index = percentages.IndexOf(percentages.FirstOrDefault(p => p > randomValue) ?? 100);

        return _coinIndexValues[index];
    }

    public void OnTriggerEnter(Collider collider)
    {
        if (_isConsumed || collider.tag != Constants.PlayerTag)
        {
            return;
        }

        _isConsumed = true;

        PermanentProgressionManager.AddCoin(_value);

        GameController.instance.coinCollected += _value;

        SFXPlayer.PlayOneshot(SfxOneshot.Coin);

        Destroy(transform.parent.gameObject);
    }
}
