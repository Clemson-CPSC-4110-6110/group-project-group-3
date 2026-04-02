using System.Collections;
using UnityEngine;

/// <summary>
/// Requests microphone permission on Android/iOS. Editor/desktop always allowed.
/// </summary>
public static class MicrophonePermissionHelper
{
    public static bool HasMicrophonePermission()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone);
#elif UNITY_IOS && !UNITY_EDITOR
        return Application.HasUserAuthorization(UserAuthorization.Microphone);
#else
        return true;
#endif
    }

    /// <summary>
    /// Yields until permission is granted or denied (iOS may need multiple frames).
    /// </summary>
    public static IEnumerator EnsureMicrophonePermissionCoroutine()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
            float t = 0f;
            while (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone) && t < 30f)
            {
                t += Time.deltaTime;
                yield return null;
            }
        }
#elif UNITY_IOS && !UNITY_EDITOR
        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
        }
#endif
        yield break;
    }
}
