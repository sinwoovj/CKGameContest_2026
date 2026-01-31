using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

public static class DictionaryExtensions
{
    [CanBeNull]
    public static TV Get<TK, TV>(this IDictionary<TK, TV> dict, [CanBeNull] TK key, TV defaultValue = default)
    {
        if (key == null)
        {
            return defaultValue;
        }

        return (!dict.TryGetValue(key, out TV tv)) ? defaultValue : tv;
    }

    public static bool TryGetValueWithSubStringKey<T>(this Dictionary<string, T> source, [NotNull] string key, out T value)
    {
        if (source.TryGetValue(key, out value))
        {
            return true;
        }

        foreach (KeyValuePair<string, T> keyValuePair in source)
        {
            bool ignoreCase = key.IndexOf(keyValuePair.Key, StringComparison.OrdinalIgnoreCase) >= 0;
            if (keyValuePair.Key.Length > 0 && ignoreCase)
            {
                value = keyValuePair.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> target)
    {
        if (source == null)
        {
            throw new Exception("source is null");
        }

        if (target == null)
        {
            return;
        }

        foreach (KeyValuePair<TKey, TValue> keyValuePair in target)
        {
            source[keyValuePair.Key] = keyValuePair.Value;
        }
    }

    public static TValue GetRandomValue<TKey, TValue>(this IDictionary<TKey, TValue> source, System.Random random = null)
    {
        if (source.Count == 0)
        {
            throw new Exception("source count is zero.");
        }

        return source.Values.ToArray()[(random != null) ? random.Next(0, source.Count) : UnityEngine.Random.Range(0, source.Count)];
    }


    /// <summary>
    /// Dictionary를 깊은 복사합니다.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this Dictionary<TKey, TValue> source)
    {
        Dictionary<TKey, TValue> clone = new Dictionary<TKey, TValue>();
        foreach (KeyValuePair<TKey, TValue> item in source)
        {
            clone[item.Key] = item.Value;
        }

        return clone;
    }
}
