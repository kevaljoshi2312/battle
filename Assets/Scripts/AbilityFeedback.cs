using UnityEngine;

public static class AbilityFeedback
{
    static string message;
    static float expireTime;

    public static string Message => Time.time < expireTime ? message : string.Empty;

    public static void Show(string text, float duration = 2f)
    {
        message = text;
        expireTime = Time.time + duration;
    }
}
