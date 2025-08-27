using System;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    public static byte[] FromAudioClip(AudioClip audioClip)
    {
        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            int sampleRate = audioClip.frequency; 
            int numChannels = 1; // Force Mono for Whisper API

            float[] samples = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(samples, 0);

            // ✅ Convert to Mono if Stereo
            if (audioClip.channels > 1)
            {
                samples = ConvertToMono(samples, audioClip.channels);
            }

            // ✅ Normalize Samples
            Normalize(samples);

            int dataSize = samples.Length * 2; // 16-bit audio
            int fileSize = 44 + dataSize;

            // WAV Header
            writer.Write(new char[] { 'R', 'I', 'F', 'F' });
            writer.Write(fileSize - 8);
            writer.Write(new char[] { 'W', 'A', 'V', 'E' });

            // Format Chunk
            writer.Write(new char[] { 'f', 'm', 't', ' ' });
            writer.Write(16);                   // PCM chunk size
            writer.Write((short)1);             // PCM format
            writer.Write((short)numChannels);   // Channels
            writer.Write(sampleRate);           // Sample rate
            writer.Write(sampleRate * numChannels * 2); // Byte rate
            writer.Write((short)(numChannels * 2));     // Block align
            writer.Write((short)16);            // Bits per sample

            // Data Chunk
            writer.Write(new char[] { 'd', 'a', 't', 'a' });
            writer.Write(dataSize);

            // ✅ Write Audio Data (16-bit PCM)
            foreach (float sample in samples)
            {
                short intSample = (short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
                writer.Write(intSample);
            }

            return stream.ToArray();
        }
    }

    // ✅ Convert Stereo to Mono
    private static float[] ConvertToMono(float[] stereoSamples, int channels)
    {
        int monoLength = stereoSamples.Length / channels;
        float[] monoSamples = new float[monoLength];

        for (int i = 0; i < monoLength; i++)
        {
            float sum = 0f;
            for (int j = 0; j < channels; j++)
            {
                sum += stereoSamples[i * channels + j];
            }
            monoSamples[i] = sum / channels; // Average channels
        }
        return monoSamples;
    }

    // ✅ Normalize Audio Levels
    private static void Normalize(float[] samples)
    {
        float max = 0f;
        foreach (float sample in samples)
        {
            if (Mathf.Abs(sample) > max)
                max = Mathf.Abs(sample);
        }

        if (max > 0f)
        {
            float normalizationFactor = 1f / max;
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= normalizationFactor;
            }
        }
    }
}
