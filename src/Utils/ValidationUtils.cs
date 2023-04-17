using ModName.src;
using Kitchen;
using KitchenData;
using System;

namespace ModName.src.Utils
{
    public static class ValidationUtils
    {
        public static bool IsNonNegative(int value, string parameterName = "value", bool warnIfFail = true)
        {
            if (value < 0 && warnIfFail)
                Main.LogError($"{parameterName} must not be negative.");
            return value >= 0;
        }

        public static bool IsNonNegative(float value, string parameterName = "value", bool warnIfFail = true)
        {
            if (value < 0f && warnIfFail)
                Main.LogError($"{parameterName} must not be negative.");
            return value >= 0f;
        }

        public static bool EnumTryParse<T>(string value, out T result, bool ignoreCase = true, bool warnIfFail = true) where T : struct, Enum
        {
            if (!Enum.TryParse(value, ignoreCase, out result))
            {
                if (warnIfFail)
                    Main.LogError($"Failed to parse {typeof(T).Name}.");
                result = default;
                return false;
            }
            return true;
        }
    }
}
