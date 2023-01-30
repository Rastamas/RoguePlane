using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public static class SquadExtensions
{
    public static EnemySquad ToSquad(this EnemyType enemyType)
    {
        return enemyType switch
        {
            EnemyType.Swarmy => GetGridSquad(enemyType, 3, 3, 2.5f),
            EnemyType.Normie => GetGridSquad(enemyType, 1, 2, 4f),
            EnemyType.Pez => GetPezSquad(3, 3f),
            _ => GetSingularEnemySquad(enemyType),
        };
    }

    private static EnemySquad GetPezSquad(int count, float distanceBetweenUnits)
    {
        var enemyPrefab = EnemySpawner.instance.enemyPrefabs[EnemyType.Pez];

        var enemies = Enumerable.Range(0, count).Select(y =>
            new EnemySpawn
            {
                positionOffset = new Vector3(0, -y, 2) * distanceBetweenUnits,
                prefab = enemyPrefab
            }
        );

        var navMeshSize = enemyPrefab.GetComponent<NavMeshObstacle>()?.size ?? Vector3.zero;

        var squad = new EnemySquad
        {
            type = EnemyType.Pez,
            enemies = enemies.ToList(),
            size = new Vector3
            {
                x = enemies.Max(e => e.positionOffset.x) - enemies.Min(e => e.positionOffset.x) + navMeshSize.x * enemyPrefab.transform.localScale.x,
                y = 0,
                z = enemies.Max(e => e.positionOffset.z) - enemies.Min(e => e.positionOffset.z) + navMeshSize.z * enemyPrefab.transform.localScale.z
            }
        };

        return squad;
    }

    private static EnemySquad GetGridSquad(EnemyType enemyType, int rows, int columns, float distanceBetweenUnits)
    {
        var enemyPrefab = EnemySpawner.instance.enemyPrefabs[enemyType];
        var squadRows = rows;
        var squadColumns = columns;
        var enemies = Enumerable.Range(0, squadColumns).SelectMany(x => Enumerable.Range(0, squadRows)
             .Select(z => new EnemySpawn { positionOffset = new Vector3(x, 0, z) * distanceBetweenUnits, prefab = enemyPrefab }));

        var navMeshSize = enemyPrefab.GetComponent<NavMeshObstacle>()?.size ?? Vector3.zero;

        var squad = new EnemySquad
        {
            type = enemyType,
            enemies = enemies.ToList(),
            size = new Vector3
            {
                x = enemies.Max(e => e.positionOffset.x) - enemies.Min(e => e.positionOffset.x) + navMeshSize.x * enemyPrefab.transform.localScale.x,
                y = 0,
                z = enemies.Max(e => e.positionOffset.z) - enemies.Min(e => e.positionOffset.z) + navMeshSize.z * enemyPrefab.transform.localScale.z
            }
        };

        return squad;
    }

    private static EnemySquad GetSingularEnemySquad(EnemyType enemyType)
    {
        var enemyPrefab = EnemySpawner.instance.enemyPrefabs[enemyType];
        var enemies = new List<EnemySpawn> {
            new EnemySpawn {
                prefab = enemyPrefab,
                positionOffset = Vector3.zero
                }
            };

        var navMeshSize = enemyPrefab.GetComponent<NavMeshObstacle>()?.size ?? Vector3.zero;

        return new EnemySquad
        {
            type = enemyType,
            enemies = enemies.ToList(),
            size = new Vector3
            {
                x = navMeshSize.x * enemyPrefab.transform.localScale.x,
                y = 0,
                z = navMeshSize.z * enemyPrefab.transform.localScale.z
            }
        };
    }
}
