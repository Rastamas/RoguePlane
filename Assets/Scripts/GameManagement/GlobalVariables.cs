using System.Collections.Generic;

public static class GlobalVariables
{

    private static readonly object lockObject = new object();
    private static Dictionary<string, object> variablesDictionary = new Dictionary<string, object>();

    public static bool TryGet<T>(string key, out T value)
    {
        if (variablesDictionary == null || !variablesDictionary.ContainsKey(key))
        {
            value = default;
            return false;
        }

        value = (T)variablesDictionary[key];

        return true;
    }

    public static void Set(string key, object value)
    {
        lock (lockObject)
        {
            if (variablesDictionary == null)
            {
                variablesDictionary = new Dictionary<string, object>();
            }
            variablesDictionary[key] = value;
        }
    }

}
