using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySquad
{
    public EnemyType type;
    public List<EnemySpawn> enemies;
    public Vector3 size;

    public int Value => (int)enemies.Sum(e => e.prefab.GetComponent<EnemyController>()?.maxHealth ?? 0);
}

public class EnemySpawn
{
    public GameObject prefab;
    public Vector3 positionOffset;
}
