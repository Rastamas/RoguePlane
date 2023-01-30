using System;
using System.Collections.Generic;
using System.Linq;

public static class EnumExtensions
{
    public static List<T> GetValues<T>() => Enum.GetValues(typeof(T)).Cast<T>().ToList();
}
