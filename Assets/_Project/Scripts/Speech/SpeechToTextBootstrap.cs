using UnityEngine;

/// <summary>
/// Creates a <see cref="SpeechToTextController"/> at runtime if none exists, so you can press Play without wiring the scene.
/// </summary>
internal static class SpeechToTextBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateControllerIfMissing()
    {
        if (!Application.isPlaying)
            return;

        SpeechToTextController[] existing = Object.FindObjectsByType<SpeechToTextController>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        if (existing.Length > 0)
            return;

        var go = new GameObject("SpeechToText");
        go.AddComponent<SpeechToTextController>();
    }
}
