using System;
using UnityEngine;

/// <summary>
/// Records from UnityEngine.Microphone into a Float PCM AudioClip.
/// </summary>
public class MicrophoneRecorder
{
    private string _deviceName;
    private AudioClip _recordingClip;
    private int _lastSamplePosition;

    public bool IsRecording { get; private set; }

    public void StartRecording(int maxSeconds, int sampleRate)
    {
        if (IsRecording)
            return;

        if (Microphone.devices.Length == 0)
            throw new InvalidOperationException("No microphone available.");

        _deviceName = Microphone.devices[0];

        // macOS Editor often rejects some rates; try a short list (preferred first).
        _recordingClip = null;
        TryStart(maxSeconds, sampleRate);
        if (_recordingClip == null)
            TryStart(maxSeconds, 44100);
        if (_recordingClip == null)
            TryStart(maxSeconds, 48000);
        if (_recordingClip == null)
            TryStart(maxSeconds, 16000);

        if (_recordingClip == null)
        {
            throw new InvalidOperationException(
                "Microphone.Start returned null for all sample rates. Check macOS Privacy → Microphone for Unity.");
        }

        _lastSamplePosition = 0;
        IsRecording = true;

        void TryStart(int seconds, int hz)
        {
            if (_recordingClip != null || hz <= 0)
                return;
            var clip = Microphone.Start(_deviceName, false, seconds, hz);
            if (clip != null)
                _recordingClip = clip;
        }
    }

    public void StopRecording()
    {
        if (!IsRecording)
            return;

        if (!string.IsNullOrEmpty(_deviceName) && _recordingClip != null)
            _lastSamplePosition = Microphone.GetPosition(_deviceName);

        if (!string.IsNullOrEmpty(_deviceName))
            Microphone.End(_deviceName);

        IsRecording = false;
    }

    /// <summary>
    /// Returns a trimmed copy of the recorded clip (only samples actually captured so far).
    /// </summary>
    public AudioClip FinalizeClip()
    {
        if (_recordingClip == null)
            return null;

        int position = _lastSamplePosition;
        if (position <= 0)
            position = _recordingClip.samples;
        position = Mathf.Clamp(position, 1, _recordingClip.samples);

        float[] soundData = new float[_recordingClip.samples * _recordingClip.channels];
        _recordingClip.GetData(soundData, 0);

        int copyLength = Mathf.Min(position, _recordingClip.samples);
        if (copyLength < 1)
            return null;

        int channels = _recordingClip.channels;
        float[] trimmed = new float[copyLength * channels];
        Array.Copy(soundData, trimmed, trimmed.Length);

        var trimmedClip = AudioClip.Create(
            "speech_recording",
            copyLength,
            channels,
            _recordingClip.frequency,
            false);
        trimmedClip.SetData(trimmed, 0);

        UnityEngine.Object.Destroy(_recordingClip);
        _recordingClip = null;
        _deviceName = null;

        return trimmedClip;
    }

    public void CancelRecording()
    {
        if (!IsRecording)
            return;

        if (!string.IsNullOrEmpty(_deviceName))
            Microphone.End(_deviceName);

        IsRecording = false;
        _lastSamplePosition = 0;
        if (_recordingClip != null)
        {
            UnityEngine.Object.Destroy(_recordingClip);
            _recordingClip = null;
        }

        _deviceName = null;
    }
}
