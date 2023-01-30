using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    private Slider _fill;
    private Transform _dotsParent;
    private float _sumEnemyScore;
    private float _killedEnemiesScore;
    private List<Transform> _dots;
    private int _dotsActive = 0;

    private static readonly Color dotActiveColor = new Color(0, 1, .796875f, .6f);

    public void Awake()
    {
        _fill = GetComponent<Slider>();
        _dotsParent = transform.GetChild(transform.childCount - 1).transform;
        var parentWidth = _dotsParent.GetComponent<RectTransform>().rect.width;

        var dot = _dotsParent.GetChild(0);

        var rect = dot.GetComponent<RectTransform>().rect;
        rect.x = parentWidth / 2f * -1f;

        var encounterSizes = GameController.instance.encounters
            .Where(e => e.size != EncounterSize.Boss)
            .Select(e => (float)EnemySpawner.instance.encounterSizes[e.size]);

        _sumEnemyScore = encounterSizes.Sum();

        var sizesSoFar = 0f;

        _dots = new List<Transform>{
            dot
        };

        foreach (var encounter in encounterSizes)
        {
            sizesSoFar += encounter;
            var newDot = Instantiate(dot, dot.position, dot.rotation, _dotsParent);

            newDot.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(sizesSoFar / _sumEnemyScore * parentWidth, 0, 0);
            _dots.Add(newDot);
        }

        _dots.First().GetComponent<RectTransform>().anchoredPosition3D = new Vector3(20, 0, 0);
        _dots.Last().GetComponent<RectTransform>().anchoredPosition3D = new Vector3(parentWidth - 20, 0, 0);
    }

    public void UpdateProgress(float killedEnemyScore)
    {
        var encounterSizesSoFar = GameController.instance.encounters.Where(e => e.index <= GameController.currentEncounter)
            .Sum(e => (float)EnemySpawner.instance.encounterSizes[e.size]);

        _killedEnemiesScore = Mathf.Min(encounterSizesSoFar, _killedEnemiesScore + killedEnemyScore);

        _fill.value = _killedEnemiesScore / _sumEnemyScore;
    }

    internal void PowerupReached()
    {
        var dotImage = _dots[_dotsActive++].GetComponent<Image>();

        dotImage.color = dotActiveColor;
    }
}
