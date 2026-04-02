using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Converts Float PCM AudioClip data to WAV bytes (e.g. for Wit.ai speech API).
/// </summary>
public static class WavUtility
{
    public static byte[] FromAudioClip(AudioClip clip)
    {
        if (clip == null)
            throw new ArgumentNullException(nameof(clip));

        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        MemoryStream stream = new MemoryStream();
        byte[] header = new byte[44];
        int hz = clip.frequency;
        ushort channels = (ushort)clip.channels;
        WriteWavHeader(header, samples.Length * sizeof(short), hz, channels);
        stream.Write(header, 0, header.Length);

        for (int i = 0; i < samples.Length; i++)
        {
            short s = (short)(Mathf.Clamp(samples[i], -1f, 1f) * short.MaxValue);
            stream.WriteByte((byte)(s & 0xff));
            stream.WriteByte((byte)((s >> 8) & 0xff));
        }

        return stream.ToArray();
    }

    private static void WriteWavHeader(byte[] header, int dataLength, int hz, ushort channels)
    {
        int byteRate = hz * channels * 2;
        int blockAlign = channels * 2;

        using (var ms = new MemoryStream(header))
        using (var writer = new BinaryWriter(ms))
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataLength);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((ushort)1);
            writer.Write(channels);
            writer.Write(hz);
            writer.Write(byteRate);
            writer.Write((ushort)blockAlign);
            writer.Write((ushort)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(dataLength);
        }
    }
}
