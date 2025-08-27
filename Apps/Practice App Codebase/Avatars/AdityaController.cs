using UnityEngine;

public class AdityaController : MonoBehaviour
{
    public OpenAIManager openAIManager;
    public AudioSource adityaAudioSource;
    public string LastResponse { get; private set; }

    // Static introductory line for Aditya
    public void RespondToStaticLine(System.Action onComplete)
    {
        string staticLine = "Alright, since we're stuck here together, let's at least make it interesting. I'm Aditya, graphic designer.";
        Debug.Log("Aditya (Static): " + staticLine);

        // Play TTS for the static line
        openAIManager.StartCoroutine(openAIManager.TextToSpeech(staticLine, "aditya", adityaAudioSource, onComplete));
    }

    public void RespondToUser(string userInput, System.Action callback)
    {
        string personality = "You are Aditya, a 25-year-old freelance graphic designer from Delhi. You have a gentle personality. " +
                             "You enjoy engaging in casual conversations about everyday topics, asking questions to get to know people better. " +
                             "Speak casually, (sometimes after getting comfortable) with dry humor. Keep the conversation flowing naturally, like you're talking to a friend or stranger. " +
                             "Limit your responses to a maximum of two sentences, and always end with a question to keep the conversation going.";

        openAIManager.StartCoroutine(openAIManager.GenerateResponse(userInput, personality, response =>
        {
            LastResponse = response;
            HandleAdityaResponse(response, callback);
        }));
    }

    private void HandleAdityaResponse(string response, System.Action onComplete)
    {
        Debug.Log("Aditya: " + response);
        LastResponse = response;

        openAIManager.StartCoroutine(openAIManager.TextToSpeech(response, "aditya", adityaAudioSource, onComplete));
    }
}
