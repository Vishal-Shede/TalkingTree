using UnityEngine;
using TMPro;

public class VRButtonHandler : MonoBehaviour
{
    public MicrophoneCapture micCapture;
    public TMP_Text buttonText;

    private bool isRecording = false;

    public void OnButtonPress()
    {
        isRecording = !isRecording;
        micCapture.ToggleRecording();
        buttonText.text = isRecording ? "Stop" : "Speak";
    }

    // Reset Button Method
    public void ResetButton()
    {
        // Ensure recording is stopped
        if (isRecording)
        {
            micCapture.ToggleRecording();
            isRecording = false;
        }

        // Reset button text to "Speak"
        buttonText.text = "Speak";

        Debug.Log("🔄 Speak button reset.");
    }
}
