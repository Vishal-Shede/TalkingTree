using UnityEngine;

public class AnjaliController : MonoBehaviour
{
    public OpenAIManager openAIManager;
    public AudioSource anjaliAudioSource;
    public string LastResponse { get; private set; }

    // Static introductory line for Anjali
    public void RespondToStaticLine(System.Action onComplete)
    {
        string staticLine = "Ignore him; he's all bark and no bite. I’m Anjali, a school teacher. Why don’t you tell us about yourself?";
        Debug.Log("Anjali (Static): " + staticLine);

        // Play TTS for the static line
        openAIManager.StartCoroutine(openAIManager.TextToSpeech(staticLine, "anjali", anjaliAudioSource, onComplete));
    }

    public void RespondToUser(string userInput, System.Action callback)
    {
        string personality = "You are Anjali, a 26-year-old school teacher from Mumbai who loves literature and enjoys philosophical conversations. " +
                             "You are warm, approachable, and thoughtful, often providing positive feedback and encouragement. " +
                             "Your speech is empathetic and friendly, and you ask deep and meaningful questions to keep the conversation engaging. " +
                             "Keep your responses concise (maximum of three sentences) and reflective, ending with an open-ended question to continue the dialogue.";

        openAIManager.StartCoroutine(openAIManager.GenerateResponse(userInput, personality, response =>
        {
            LastResponse = response;
            HandleAnjaliResponse(response, callback);
        }));
    }

    private void HandleAnjaliResponse(string response, System.Action onComplete)
    {
        Debug.Log("Anjali: " + response);
        LastResponse = response;

        openAIManager.StartCoroutine(openAIManager.TextToSpeech(response, "anjali", anjaliAudioSource, onComplete));
    }
}
