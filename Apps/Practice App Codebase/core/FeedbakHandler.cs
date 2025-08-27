using System.Collections;
using UnityEngine;
using TMPro;

public class FeedbackHandler : MonoBehaviour
{
    public OpenAIManager openAIManager; // Reference to OpenAIManager
    public TextMeshProUGUI feedbackText; // Reference to the UI text for feedback

    // Method triggered when Feedback button is clicked
    

    public void OnFeedbackButtonClick()
{
    Debug.Log("Feedback Button Clicked!"); // Add this line for debugging
    if (openAIManager != null)
    {
        StartCoroutine(openAIManager.AnalyzeGrammar((feedback) =>
        {
            feedbackText.text = feedback;
        }));
    }
    else
    {
        Debug.LogError("OpenAIManager reference is not set in FeedbackHandler.");
    }
}
}
