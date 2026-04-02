using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Sends WAV audio to Wit.ai <c>POST /speech</c> and returns the understood utterance text.
/// Create an app at https://wit.ai — use the Server Access Token (not the Client Token).
/// </summary>
public static class WitSpeechTranscriptionClient
{
    [Serializable]
    private class WitSpeechResponse
    {
        public string text;
    }

    [Serializable]
    private class WitErrorResponse
    {
        public string error;
        public string code;
    }

    public static IEnumerator Transcribe(
        byte[] wavBytes,
        string serverAccessToken,
        string apiVersion,
        int timeoutSeconds,
        Action<string> onSuccess,
        Action<string> onError)
    {
        if (string.IsNullOrEmpty(serverAccessToken))
        {
            onError?.Invoke("Speech To Text: Wit.ai Server Access Token is not set. Assign SpeechToTextSettings.");
            yield break;
        }

        if (wavBytes == null || wavBytes.Length < 44)
        {
            onError?.Invoke("Speech To Text: Recording too short or invalid.");
            yield break;
        }

        string v = string.IsNullOrEmpty(apiVersion) ? "20240304" : apiVersion;
        string url = "https://api.wit.ai/speech?v=" + UnityWebRequest.EscapeURL(v);

        using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(wavBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + serverAccessToken);
            request.SetRequestHeader("Content-Type", "audio/wav");
            request.timeout = Mathf.Max(30, timeoutSeconds);

            yield return request.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                string body = request.downloadHandler?.text ?? "";
                string msg = ParseWitError(body) ?? request.error ?? "Unknown network error";
                onError?.Invoke($"Speech To Text (Wit): {msg}");
                yield break;
            }

            string json = request.downloadHandler.text;
            string text = ExtractUtteranceText(json);
            if (!string.IsNullOrEmpty(text))
                onSuccess?.Invoke(text.Trim());
            else
                onError?.Invoke("Speech To Text (Wit): No text in response. Train utterances in your Wit app or speak more clearly.");
        }
    }

    private static string ExtractUtteranceText(string json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            var parsed = JsonUtility.FromJson<WitSpeechResponse>(json);
            if (parsed != null && !string.IsNullOrEmpty(parsed.text))
                return parsed.text;
        }
        catch
        {
            // JsonUtility may fail on extra fields; fall through
        }

        // Wit returns JSON with a top-level "text" field; simple fallback if JsonUtility chokes
        var m = Regex.Match(json, "\"text\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"", RegexOptions.Singleline);
        if (m.Success)
            return Regex.Unescape(m.Groups[1].Value);

        return null;
    }

    private static string ParseWitError(string json)
    {
        if (string.IsNullOrEmpty(json))
            return null;
        try
        {
            var err = JsonUtility.FromJson<WitErrorResponse>(json);
            if (err != null && !string.IsNullOrEmpty(err.error))
                return err.code != null ? $"{err.error} ({err.code})" : err.error;
        }
        catch
        {
            // ignore
        }

        return json.Length < 400 ? json : json.Substring(0, 400) + "...";
    }
}
