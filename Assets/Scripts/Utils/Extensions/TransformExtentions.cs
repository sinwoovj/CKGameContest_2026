using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class TransformExpensions
{
    public static T FindDeepChild<T>(this Transform trans, string name) where T : Component
    {
        T[] children = trans.GetComponentsInChildren<T>(true);
        foreach (T child in children)
        {
            if (child.name == name)
            {
                return child;
            }
        }

        return null;
    }

    public static GameObject FindDeepChild(this Transform trans, string name)
    {
        Transform[] children = trans.GetComponentsInChildren<Transform>(true);
        foreach (var child in children)
        {
            if (child.name == name)
            {
                return child.gameObject;
            }
        }

        return null;
    }
}
