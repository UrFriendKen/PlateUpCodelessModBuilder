using CodelessModBuilder.src;

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
    }
}
