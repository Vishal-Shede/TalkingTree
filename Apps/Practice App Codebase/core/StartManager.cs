using UnityEngine;
using UnityEngine.UI;

public class StartManager : MonoBehaviour
{
    public Button startButton;
    public Button speakButton;

    public AdityaController adityaController;
    public AnjaliController anjaliController;

    private void Start()
    {
        // Set initial button states
        speakButton.interactable = false;

        // Add a listener to the start button
        startButton.onClick.AddListener(StartConversation);
    }

    private void StartConversation()
    {
        // Hide the start button by disabling its GameObject
        startButton.gameObject.SetActive(false);

        // Aditya starts the introduction
        adityaController.RespondToStaticLine(() =>
        {
            // After Aditya finishes, Anjali introduces herself
            anjaliController.RespondToStaticLine(() =>
            {
                // Enable the speak button after introductions
                speakButton.interactable = true;
            });
        });
    }

    // Reset Manager Method
    public void ResetManager()
    {
        // Reactivate the start button
        startButton.gameObject.SetActive(true);

        // Disable the speak button
        speakButton.interactable = false;

        Debug.Log("🔄 StartManager reset for the next session.");
    }
}
