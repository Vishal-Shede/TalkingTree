using UnityEngine;

public class EndButtonHandler : MonoBehaviour
{
    public OpenAIManager openAIManager; // Reference to OpenAIManager
    public StartManager startManager;   // Reference to StartManager
    public GameObject startButton;      // The Start button
    public GameObject speakButton;      // The Speak button
    public GameObject endButton;        // The End button
    public GameObject popupFrame;       // The popup frame with Feedback and Self-Reflection options

    public void OnEndButtonPress()
    {
        // Debug to confirm button press
        Debug.Log("End button pressed!");

        // Check references
        if (openAIManager == null)
        {
            Debug.LogError("OpenAIManager is not assigned!");
            return;
        }
        if (startManager == null)
        {
            Debug.LogError("StartManager is not assigned!");
            return;
        }
        if (startButton == null)
        {
            Debug.LogError("Start button is not assigned!");
            return;
        }
        if (speakButton == null)
        {
            Debug.LogError("Speak button is not assigned!");
            return;
        }
        if (endButton == null)
        {
            Debug.LogError("End button is not assigned!");
            return;
        }
        if (popupFrame == null)
        {
            Debug.LogError("PopupFrame is not assigned!");
            return;
        }

        // Debugging to confirm all references are valid
        Debug.Log("All references are valid. Proceeding with End button logic.");

        // Stop all ongoing interactions
        openAIManager.StopInteraction();
        Debug.Log("🛑 Interaction stopped.");

        // Reset relevant managers for the next session
        startManager.ResetManager();
        Debug.Log("🔄 Managers reset for the next session.");

        // Hide Start, Speak, and End buttons
        startButton.SetActive(false);
        speakButton.SetActive(false);
        endButton.SetActive(false);
        Debug.Log("📴 Start, Speak, and End buttons hidden.");

        // Show the PopupFrame
        popupFrame.SetActive(true);
        Debug.Log("✅ Popup frame displayed.");
    }
}
