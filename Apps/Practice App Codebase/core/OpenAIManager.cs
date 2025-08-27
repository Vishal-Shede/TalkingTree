using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

[Serializable]
public class ConversationEntry
{
    public string Speaker; // "User", "Aditya", or "Anjali"
    public string Text;    // The spoken or typed text

    public ConversationEntry(string speaker, string text)
    {
        Speaker = speaker;
        Text = text;
    }
}

public class OpenAIManager : MonoBehaviour
{
    public enum Turn { Aditya, Anjali }
    private Turn currentTurn = Turn.Aditya;

    [Header("OpenAI API Settings")]
    public string apiKey;
    private string sttEndpoint = "https://api.openai.com/v1/audio/transcriptions";
    private string gptEndpoint = "https://api.openai.com/v1/chat/completions";
    private string ttsEndpoint = "https://api.openai.com/v1/audio/speech";

    private TopicManager.Topic currentTopic;
    private bool waitingForTransitionResponse = false;


    [Header("Audio Settings")]
    public AudioSource adityaAudioSource;
    public AudioSource anjaliAudioSource;

    [Header("Debug Settings")]
    public bool debugMode = true;

    public AdityaController adityaController;
    public AnjaliController anjaliController;

    private string lastResponse;
    private bool isProcessing = false;

    // List to store the conversation log
    private List<ConversationEntry> conversationLog = new List<ConversationEntry>();

    //Start 
    private void Start()
    {
       TopicManager.OnTopicChanged += HandleTopicChange;
    }


    
    
    
    
    
    
    // Speech-to-Text (STT) Method
    public IEnumerator ConvertAudioToText(AudioClip clip, Action<string> callback)
    {
        if (debugMode) Debug.Log("🔊 Starting audio-to-text conversion...");

        byte[] audioData = WavUtility.FromAudioClip(clip);

        var formData = new List<IMultipartFormSection>
        {
            new MultipartFormFileSection("file", audioData, "audio.wav", "audio/wav"),
            new MultipartFormDataSection("model", "whisper-1")
        };

        using (UnityWebRequest request = UnityWebRequest.Post(sttEndpoint, formData))
        {
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                if (debugMode) Debug.Log("✅ STT API Request Successful!");
                
                string responseText = request.downloadHandler.text;

if (debugMode) Debug.Log($"📨 Raw STT Response:\n{responseText}");

try
{
    try
{
    var response = JsonConvert.DeserializeObject<WhisperSTTResponse>(responseText);

    if (!string.IsNullOrEmpty(response.text))
    {
        callback(response.text);
    }
    else
    {
        Debug.LogError("STT response did not contain 'text'.");
        callback("Error: No text found.");
    }
}
catch (Exception e)
{
    Debug.LogError($"❌ STT JSON Parsing Error: {e.Message}");
    Debug.LogError($"❌ Raw JSON: {responseText}");
    callback("Error: Failed to parse STT response.");
}

}
catch (Exception e)
{
    Debug.LogError($"❌ STT JSON Parsing Error: {e.Message}");
    Debug.LogError($"❌ Raw JSON: {responseText}");
    callback("Error: Failed to parse STT response.");
}

            }
            else
            {
                Debug.LogError($"STT API Error: {request.error}");
                callback("Error");
            }
        }
    }

    
    
    
    
    
    
    // Generate GPT Response
    public IEnumerator GenerateResponse(string input, string personality, Action<string> callback)
    {
        if (debugMode) Debug.Log($"🤖 Generating GPT response for: {input}");

        string topicInstruction = "";

switch (currentTopic)
{
    case TopicManager.Topic.Introduction:
        topicInstruction = "Focus only on introductions — like name, city, and how the person describes themselves.";
        break;
    case TopicManager.Topic.StudyWork:
        topicInstruction = "Focus only on their studies or work — like what they do, where they work/study, and their routine.";
        break;
    case TopicManager.Topic.Hobby:
        topicInstruction = "Focus only on hobbies — ask about what they enjoy doing, any skills, or recent activities.";
        break;
    case TopicManager.Topic.Ambition:
        topicInstruction = "Focus only on their future goals, dreams, and what they aspire to do.";
        break;
    case TopicManager.Topic.End:
        topicInstruction = "Wrap up the discussion with polite and encouraging closing remarks.";
        break;
}

var payload = new
{
    model = "gpt-4",
    messages = new[]
    {
        new { role = "system", content = $"{personality} Always respond in English. Keep responses short (1–2 sentences) and end with a question. {topicInstruction}" },
        new { role = "user", content = input }
    }
};


        string jsonPayload = JsonConvert.SerializeObject(payload);

        using (UnityWebRequest request = new UnityWebRequest(gptEndpoint, "POST"))
        {
            byte[] jsonToSend = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
{
    if (debugMode) Debug.Log("✅ GPT API Request Successful!");

    string responseText = request.downloadHandler.text;
    if (debugMode) Debug.Log($"📨 Raw GPT API Response:\n{responseText}");

    try
    {
        var response = JsonConvert.DeserializeObject<GPTResponse>(responseText);

        if (response != null && response.choices != null && response.choices.Length > 0)
        {
            callback(response.choices[0].message.content);
        }
        else
        {
            Debug.LogError("🛑 GPT response format invalid or empty.");
            callback("Error: Unexpected response format.");
        }
    }
    catch (Exception e)
    {
        Debug.LogError($"❌ JSON Parsing Error: {e.Message}");
        Debug.LogError($"❌ Raw JSON: {responseText}");
        callback("Error: Failed to parse GPT response.");
    }
}

            else
            {
                Debug.LogError($"GPT API Error: {request.error}");
                callback("Error generating response.");
            }
        }
    }

    
    
    
    
    
    
    
    
    
    // Text-to-Speech (TTS) Method
    public IEnumerator TextToSpeech(string text, string voice, AudioSource targetAudioSource, Action onComplete = null)
    {
        if (debugMode) Debug.Log($"🔊 Sending text to TTS: {text}");

        var payload = new
        {
            model = "tts-1",
            input = text,
            voice = MapVoiceName(voice)
        };

        string jsonPayload = JsonConvert.SerializeObject(payload);

        using (UnityWebRequest request = new UnityWebRequest(ttsEndpoint, "POST"))
        {
            byte[] jsonToSend = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                if (debugMode) Debug.Log("✅ TTS API Request Successful!");
                byte[] audioData = request.downloadHandler.data;
                PlayAudioFromBytes(audioData, targetAudioSource, onComplete);
            }
            else
            {
                Debug.LogError($"TTS API Error: {request.error}");
            }
        }
    }

    
    
    
    
    
    
    private string MapVoiceName(string voice)
    {
        switch (voice.ToLower())
        {
            case "aditya": return "alloy";
            case "anjali": return "sage";
            default: return voice;
        }
    }

   
    
    
    
    
    
    
    private void PlayAudioFromBytes(byte[] audioData, AudioSource targetAudioSource, Action onComplete)
    {
        string tempPath = Application.persistentDataPath + "/response.mp3";
        File.WriteAllBytes(tempPath, audioData);
        StartCoroutine(LoadAudioClip(tempPath, targetAudioSource, onComplete));
    }

    
    
    
    
    
    
    private IEnumerator LoadAudioClip(string path, AudioSource targetAudioSource, Action onComplete)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                targetAudioSource.clip = audioClip;
                targetAudioSource.Play();

                yield return new WaitForSeconds(audioClip.length); // Wait for playback to finish
                onComplete?.Invoke();
            }
            else
            {
                Debug.LogError($"Error loading audio clip: {www.error}");
            }
        }
    }

    private void LogConversation(string speaker, string text)
    {
        conversationLog.Add(new ConversationEntry(speaker, text));
        if (debugMode) Debug.Log($"Logged Conversation - {speaker}: {text}");
    }

    
    
    
    
    
    
    // Format conversation log for grammar analysis
    private string FormatForGrammarAnalysis()
    {
        string formatted = "Conversation Analysis:\n\n";
        foreach (var entry in conversationLog)
        {
            if (entry.Speaker == "User") // Only analyzing user input
                formatted += $"User: {entry.Text}\n";
        }
        return formatted;
    }

    
    
    
    
    
    
    // Analyze Grammar
    public IEnumerator AnalyzeGrammar(Action<string> callback)
    {
        if (debugMode) Debug.Log("📋 Preparing grammar analysis request...");

        string conversationData = FormatForGrammarAnalysis();

        var payload = new
        {
            model = "gpt-4",
            messages = new[]
            {
                new { role = "system", content = "You are a grammar expert. Analyze the following sentences for grammatical correctness. Highlight errors and suggest improvements." },
                new { role = "user", content = conversationData }
            }
        };

        string jsonPayload = JsonConvert.SerializeObject(payload);

        using (UnityWebRequest request = new UnityWebRequest(gptEndpoint, "POST"))
        {
            byte[] jsonToSend = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            request.SetRequestHeader("Content-Type", "application/json");

            if (debugMode) Debug.Log("📤 Sending grammar analysis request...");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
{
    if (debugMode) Debug.Log("✅ Grammar analysis request successful!");

    string responseText = request.downloadHandler.text;
    if (debugMode) Debug.Log($"📨 Raw GPT API Response:\n{responseText}");

    try
    {
        var response = JsonConvert.DeserializeObject<GPTResponse>(responseText);

        if (response != null && response.choices != null && response.choices.Length > 0)
        {
            callback(response.choices[0].message.content);
        }
        else
        {
            Debug.LogError("🛑 GPT grammar response format invalid or empty.");
            callback("Error: Unexpected grammar analysis response format.");
        }
    }
    catch (Exception e)
    {
        Debug.LogError($"❌ JSON Parsing Error (Grammar): {e.Message}");
        Debug.LogError($"❌ Raw JSON: {responseText}");
        callback("Error: Failed to parse grammar analysis response.");
    }
}

            else
            {
                Debug.LogError($"Grammar Analysis API Error: {request.error}");
                callback("Error analyzing grammar.");
            }
        }
    }

    
    
    
    
    
    public void HandleTurn(string userInput)
    {
        if (isProcessing) return;

        isProcessing = true;

        // Log the user's input
        LogConversation("User", userInput);

        if (currentTurn == Turn.Aditya)
        {
            StartCoroutine(GenerateResponse(userInput, "Aditya is gentle with dry humour", response =>
            {
                lastResponse = response;

                // Log Aditya's response
                LogConversation("Aditya", response);

                StartCoroutine(TextToSpeech(response, "aditya", adityaAudioSource, () =>
                {
                    isProcessing = false;
                    currentTurn = Turn.Anjali; // Switch turn
                }));
            }));
        }
        else
        {
            StartCoroutine(GenerateResponse(userInput, "Anjali is empathetic and warm", response =>
            {
                lastResponse = response;

                // Log Anjali's response
                LogConversation("Anjali", response);

                StartCoroutine(TextToSpeech(response, "anjali", anjaliAudioSource, () =>
                {
                    isProcessing = false;
                    currentTurn = Turn.Aditya; // Switch turn
                }));
            }));
        }
    }

    
    
    
    
    
    
    // Stop Interaction Method
    public void StopInteraction()
    {
        // Stop all coroutines
        StopAllCoroutines();

        // Stop audio playback
        if (adityaAudioSource.isPlaying) adityaAudioSource.Stop();
        if (anjaliAudioSource.isPlaying) anjaliAudioSource.Stop();

        // Reset internal state
        isProcessing = false;
        currentTurn = Turn.Aditya;

        Debug.Log("🛑 Interaction stopped.");
    }

    //new Method
    private void HandleTopicChange(TopicManager.Topic newTopic)
    {
       currentTopic = newTopic;
       waitingForTransitionResponse = true;
    }

    
    
    
    
    
    public void AdvanceToNextTopic()
{
    // Move to next topic
    if (currentTopic < TopicManager.Topic.Ambition)
    {
        currentTopic++;
        Debug.Log("🔄 Topic changed to: " + currentTopic.ToString());

        // Transition line
        string transitionMessage = "That's Interesting!.";

        AudioSource speaker = (currentTurn == Turn.Aditya) ? adityaAudioSource : anjaliAudioSource;
        string voice = (currentTurn == Turn.Aditya) ? "aditya" : "anjali";

        // 👉 Play transition, then trigger the first question
        StartCoroutine(TextToSpeech(transitionMessage, voice, speaker, () =>
        {
            TriggerTopicIntroLine();  // ✅ Call to ask topic-specific question
        }));
    }
    else
    {
        Debug.Log("✅ All topics completed.");
    }
}






private void TriggerTopicIntroLine()
{
    string prompt = "";

    switch (currentTopic)
    {
        case TopicManager.Topic.Introduction:
        prompt = "Let's start by getting to know each other. What's your name and where are you from?";
        break;
    case TopicManager.Topic.StudyWork:
        prompt = "So what do you do? Are you studying or working these days?";
        break;
    case TopicManager.Topic.Hobby:
        prompt = "What do you enjoy doing in your free time?";
        break;
    case TopicManager.Topic.Ambition:
        prompt = "What are your goals or dreams for the future?";
        break;
    case TopicManager.Topic.End:
        prompt = "It was nice talking to you. Thank you for the conversation!";
        break;
    }

    AudioSource speaker = (currentTurn == Turn.Aditya) ? adityaAudioSource : anjaliAudioSource;
    string voice = (currentTurn == Turn.Aditya) ? "aditya" : "anjali";

    StartCoroutine(TextToSpeech(prompt, voice, speaker));
}



   



}

[Serializable]
public class GPTResponse
{
    public GPTChoice[] choices;

    [Serializable]
    public class GPTChoice
    {
        public GPTMessage message;
    }

    [Serializable]
    public class GPTMessage
    {
        public string content;
    }
}

[Serializable]
public class WhisperSTTResponse
{
    public string text;
    
    public Usage usage;

    [Serializable]
    public class Usage
    {
        public string type;
        public float seconds;
    }
}
