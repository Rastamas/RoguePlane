using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ScreenExtensions
{
    /// input x and y must be between 0 and 1, z must be 0
    /// (0,1)----(1,1)
    ///   |        |
    ///   |        |
    /// (0,0)----(1,0)
    public static Vector3 ViewportPointToSpawnPoint(this Vector3 viewportPoint)
    {
        var ray = Camera.main.ViewportPointToRay(viewportPoint);

        EnemySpawner.instance.spawnPlane.Raycast(ray, out var hit, 100);

        return hit.point;
    }
}
