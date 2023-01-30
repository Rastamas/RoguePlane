using UnityEngine;

public static class GameObjectExtensions
{
    public static T GetComponentInParentOrObject<T>(this GameObject gameObject) => gameObject.GetComponent<T>() ?? gameObject.GetComponentInParent<T>();
}
