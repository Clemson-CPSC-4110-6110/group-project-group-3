using UnityEngine;

/// <summary>
/// Wit.ai speech settings. Get a Server Access Token from your Wit app (Settings → API Details).
/// Create asset: Assets → Create → Project → Speech To Text Settings
/// </summary>
[CreateAssetMenu(fileName = "SpeechToTextSettings", menuName = "Project/Speech To Text Settings", order = 0)]
public class SpeechToTextSettings : ScriptableObject
{
    [Tooltip("Wit.ai Server Access Token (keep secret; do not ship in public builds without protection).")]
    [SerializeField] private string serverAccessToken = "";

    [Tooltip("Wit HTTP API version query parameter (see https://wit.ai/docs/http).")]
    [SerializeField] private string witApiVersion = "20240304";

    [Tooltip("Max length of mic capture in seconds.")]
    [SerializeField] private int maxRecordingSeconds = 30;

    [Tooltip("Sample rate for recording. 44100 Hz is reliable in macOS Editor; Wit accepts the resulting WAV.")]
    [SerializeField] private int sampleRate = 44100;

    [Tooltip("HTTP timeout for the speech request.")]
    [SerializeField] private int requestTimeoutSeconds = 120;

    public string ServerAccessToken => serverAccessToken;
    public string WitApiVersion => witApiVersion;
    public int MaxRecordingSeconds => maxRecordingSeconds;
    public int SampleRate => sampleRate;
    public int RequestTimeoutSeconds => requestTimeoutSeconds;
}
