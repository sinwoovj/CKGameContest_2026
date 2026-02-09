using System;
using System.Linq;

namespace Shurub
{
    public static class CharUtils
    {
        public static char ValidatePasswordChar(string text, int charIndex, char addedChar)
        {
            if (char.IsWhiteSpace(addedChar)) return '\0';                                          // 공백
            if (addedChar >= 0xAC00 && addedChar <= 0xD7A3) return '\0';                            // 한글
            if (char.IsSurrogate(addedChar)) return '\0';                                           // 이모지
            if (!(char.IsLetterOrDigit(addedChar) || "!@#$%^&*".Contains(addedChar))) return '\0';  // 일부 특수문자

            return addedChar;
        }
    }
}
