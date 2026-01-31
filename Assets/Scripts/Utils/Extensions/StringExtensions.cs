using System;
using UnityEngine;

public static class StringExtensions
{
    public static bool TryParseTime(this string text, out int seconds)
    {
        seconds = 0;

        if (string.IsNullOrEmpty(text) || text.Length < 4)
            return false;

        if (text[2] != ':')
            return false;

        if (!int.TryParse(text[..2], out int m)) return false;
        if (!int.TryParse(text.Substring(3, 2), out int s)) return false;

        if (m < 0 || m > 59 || s < 0 || s > 59)
            return false;

        seconds = m * 60 + s;
        return true;
    }
}
