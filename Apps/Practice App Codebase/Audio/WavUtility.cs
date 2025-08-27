using System;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    public static byte[] FromAudioClip(AudioClip clip)
    {
        MemoryStream stream = new MemoryStream();
        int headerSize = 44;

        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        short[] intData = new short[samples.Length];
        byte[] bytesData = new byte[samples.Length * 2];

        int rescaleFactor = 32767; // Convert float to Int16
        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
            byte[] byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
            writer.Write(headerSize + bytesData.Length - 8);
            writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
            writer.Write(new char[4] { 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)clip.channels);
            writer.Write(clip.frequency);
            writer.Write(clip.frequency * clip.channels * 2);
            writer.Write((short)(clip.channels * 2));
            writer.Write((short)16);
            writer.Write(new char[4] { 'd', 'a', 't', 'a' });
            writer.Write(bytesData.Length);
            writer.Write(bytesData);
        }

        return stream.ToArray();
    }

    public static AudioClip ToAudioClip(byte[] wavData)
    {
        using (MemoryStream stream = new MemoryStream(wavData))
        using (BinaryReader reader = new BinaryReader(stream))
        {
            reader.ReadBytes(22);
            int channels = reader.ReadInt16();
            int sampleRate = reader.ReadInt32();

            reader.ReadBytes(6);
            int dataSize = reader.ReadInt32();
            byte[] data = reader.ReadBytes(dataSize);

            float[] samples = new float[dataSize / 2];
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = BitConverter.ToInt16(data, i * 2) / 32768f;
            }

            AudioClip clip = AudioClip.Create("TTS_Audio", samples.Length, channels, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}




