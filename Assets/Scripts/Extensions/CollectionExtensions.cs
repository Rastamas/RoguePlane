using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public static class CollectionExtensions
{
    public static T GetRandom<T>(this IList<T> collection) => collection[Random.Range(0, collection.Count)];

    public static T GetRandom<T>(this T[] collection) => collection[Random.Range(0, collection.Length)];

    public static List<T> GetRandomValues<T>(this IEnumerable<T> collection, int count)
    {
        var randomizedCollection = collection.OrderBy(n => Guid.NewGuid());
        return randomizedCollection.Take(count).ToList();
    }
}
