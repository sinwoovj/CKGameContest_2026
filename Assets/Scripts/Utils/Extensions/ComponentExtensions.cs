using System;
using UnityEngine;

public static class ComponentExtensions
{
    public static bool TryGetComponentInParent<T>(this Component obj, out T component)
    {
        component = obj.GetComponentInParent<T>();
        return component != null;
    }

    public static bool TryGetComponentInChildren<T>(this Component obj, out T component)
    {
        component = obj.GetComponentInChildren<T>();
        return component != null;
    }
}
