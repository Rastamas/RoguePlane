using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class Bezier
{
    public static Vector3 GetQuadraticBezierPoint(float time, Vector3 point1, Vector3 point2, Vector3 point3)
    {
        return Mathf.Pow(1 - time, 2) * point1 + 2 * (1 - time) * time * point2 + Mathf.Pow(time, 2) * point3;
    }

    public static Vector3 GetQuadraticBezierTangent(float time, Vector3 point1, Vector3 point2, Vector3 point3)
    {
        return 2 * (1 - time) * (point2 - point1) + 2 * time * (point3 - point2);
    }

    public static Vector3 GetCubicBezierPoint(float time, Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4)
    {
        return Mathf.Pow(1 - time, 3) * point1 + 3 * Mathf.Pow(1 - time, 2) * time * point2 + 3 * (1 - time) * Mathf.Pow(time, 2) * point3 + Mathf.Pow(time, 3) * point4;
    }

    public static Vector3 GetCubicBezierTangent(float time, Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4)
    {
        return 3 * Mathf.Pow(1 - time, 2) * (point2 - point1) + 6 * (1 - time) * time * (point3 - point2) + 3 * Mathf.Pow(time, 2) * (point4 - point3);
    }
}
