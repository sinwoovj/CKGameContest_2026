using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ListExtensions
{
    /// <summary>
    /// List에 존재하는 값의 인덱스를 가져옵니다. 값이 없을 경우 -1을 반환합니다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int IndexOf<T>(this IList<T> source, Predicate<T> predicate)
    {
        for (int i = 0; i < source.Count; i++)
        {
            if (predicate(source[i]))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// List의 모든 요소를 바꿔줍니다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="value"></param>
    public static void SetAll<T>(this IList<T> source, T value)
    {
        for (int i = 0; i < source.Count; i++)
        {
            source[i] = value;
        }
    }

    /// <summary>
    /// List의 값 중 랜덤한 값을 가져옵니다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="random"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static T Random<T>(this IList<T> source, System.Random random = null)
    {
        if (source.Count == 0)
        {
            throw new Exception("source count is zero.");
        }

        return source[(random != null) ? random.Next(0, source.Count) : UnityEngine.Random.Range(0, source.Count)];
    }

    public static T Last<T>(this IList<T> source)
    {
        return source[source.Count - 1];
    }

    /// <summary>
    /// List를 깊은 복사합니다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static List<T> Clone<T>(this List<T> source)
    {
        List<T> clone = new List<T>();
        foreach (T item in source)
        {
            clone.Add(item);
        }

        return clone;
    }
}
