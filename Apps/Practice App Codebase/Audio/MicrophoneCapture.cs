using UnityEngine;

public class MicrophoneCapture : MonoBehaviour
{
    public OpenAIManager openAIManager;
    private AudioClip recordedClip;
    private float recordingStartTime;
    private int maxRecordingLength = 60; // Max recording length in seconds

    public void ToggleRecording()
    {
        if (Microphone.IsRecording(null))
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }

    private void StartRecording()
    {
        Debug.Log("🎙️ Recording started...");
        recordingStartTime = Time.time; // Record the start time
        recordedClip = Microphone.Start(null, false, maxRecordingLength, 44100);
    }

    private void StopRecording()
    {
        if (!Microphone.IsRecording(null))
        {
            Debug.LogWarning("No recording to stop.");
            return;
        }

        Debug.Log("🎙️ Recording stopped.");
        int recordingPosition = Microphone.GetPosition(null); // Get the current position
        float recordingLength = Time.time - recordingStartTime; // Calculate the actual recording duration

        Microphone.End(null);

        // Trim the clip to the actual recorded length
        if (recordingPosition > 0 && recordedClip != null)
        {
            AudioClip trimmedClip = TrimAudioClip(recordedClip, recordingPosition);
            recordedClip = trimmedClip;
        }
        else
        {
            Debug.LogError("Recording failed. No valid audio data.");
            return;
        }

        // Pass the audio to OpenAIManager for transcription
        if (recordedClip != null)
        {
            Debug.Log($"🎧 Recorded Audio Length: {recordingLength} seconds");
            openAIManager.StartCoroutine(openAIManager.ConvertAudioToText(recordedClip, HandleUserInput));
        }
    }

    private AudioClip TrimAudioClip(AudioClip clip, int endPosition)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        float[] trimmedSamples = new float[endPosition * clip.channels];
        System.Array.Copy(samples, trimmedSamples, endPosition * clip.channels);

        AudioClip trimmedClip = AudioClip.Create(clip.name + "_trimmed", endPosition, clip.channels, clip.frequency, false);
        trimmedClip.SetData(trimmedSamples, 0);

        return trimmedClip;
    }

    private void HandleUserInput(string transcribedText)
    {
        if (!string.IsNullOrEmpty(transcribedText))
        {
            Debug.Log("User Input: " + transcribedText);
            openAIManager.HandleTurn(transcribedText); // Pass transcribed text to OpenAIManager
        }
        else
        {
            Debug.LogError("No text was transcribed.");
        }
    }
}