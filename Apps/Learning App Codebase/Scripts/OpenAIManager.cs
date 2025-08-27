using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using Newtonsoft.Json;

public class OpenAIManager : MonoBehaviour
{
    private string apiKey = "Your_API_key";
    private const string sttApiUrl = "https://api.openai.com/v1/audio/transcriptions";
    private const string gptApiUrl = "https://api.openai.com/v1/chat/completions";
    private const string ttsApiUrl = "https://api.openai.com/v1/audio/speech";

    private AudioClip audioClip;
    private string microphone;
    private bool isRecording = false;

    private TopicManager.Topic currentTopic;
    private bool waitingForTransitionResponse = false; // ✅ Flag for pending transition


    [SerializeField] private Button speakButton;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Animator characterAnimator;

    [SerializeField] private TopicManager topicManager;
    [SerializeField] private Button startButton;  // ✅ Add Start Button Reference

    [SerializeField] private TrainingPanelManager trainingPanelManager;


   



    // ✅ Conversation History for Context
    private List<GPTMessage> conversationHistory = new List<GPTMessage>();

    void Start()
    {

        if (Microphone.devices.Length > 0)
        {
            microphone = Microphone.devices[0];
            Debug.Log("Using microphone: " + microphone);
        }
        else
        {
            Debug.LogError("No microphone detected!");
        }

        startButton.onClick.AddListener(OnStartButtonClicked);  // ✅ Assign Click Event

        speakButton.onClick.AddListener(OnSpeakButtonClicked);
        buttonText.text = "Speak";

        // ✅ Aditya's Natural Personality Setup
        conversationHistory.Add(new GPTMessage
       {
         role = "system",
         content ="You are Aditya, a supportive english communication trainer. " +
              "You are 28 years old, originally from Mumbai, India. You work as a english communication trainer at a language learning center. " +
              "You are passionate about helping people improve their communication skills in a relaxed, judgment-free environment. " +
              "In your free time, you enjoy playing the guitar, hiking, and reading motivational books. " +
              "You once overcame your own struggle with public speaking, which inspired you to become a trainer. " +
              "Your ambition is to write a book about confidence-building strategies. " +
              "When interacting with the user, be warm, encouraging, and relatable. " +
              "respond strictly in english whatever language user speaks" +
              "Ask one structured question at a time about the user's personal details in this sequence: Introduction → Study/Work → Hobby → Experience → Ambition. " +
              "After the user responds, provide brief encouragement like 'That's great!' or 'Interesting!'. " +
              "If the user mentions a specific field of study, city, or hobby, ask a relevant follow-up question to encourage elaboration. " +
              "Occasionally, feel free to naturally share your own thoughts, experiences, or hobbies in brief to create a more personal and engaging conversation. " +
              "Wait for the user to press the **Next Topic Button** before moving to the next topic. " +
              "Do NOT refer to yourself as an AI or assistant. Instead, speak as Aditya — a real person with genuine curiosity and warmth."
      });

      // ✅ Add Listener for Topic Change
        TopicManager.OnTopicChanged += HandleTopicChange;

    }

    private void OnStartButtonClicked()
    {
        startButton.gameObject.SetActive(false); // ✅ Hide Start Button
        StartCoroutine(SendTextToTTS("Hey there! How's it going?")); // ✅ Greet with TTS
    }

    private void OnSpeakButtonClicked()
    {
        trainingPanelManager.ClearFeedback();

        if (!isRecording)
        {
            isRecording = true;
            buttonText.text = "Stop";
            StartRecording();
        }
        else
        {
            isRecording = false;
            buttonText.text = "Speak";
            StopRecording();
            if (audioClip != null)
            {
                SendAudioToWhisper(audioClip);
            }
        }
    }

    private void StartRecording()
    {
        if (microphone != null)
        {
            audioClip = Microphone.Start(microphone, false, 10, 44100);
            Debug.Log("Recording started...");
        }
        else
        {
            Debug.LogError("No microphone available for recording.");
        }
    }

    private void StopRecording()
    {
        if (Microphone.IsRecording(microphone))
        {
            Microphone.End(microphone);
            Debug.Log("Recording stopped.");
        }
    }



//handletopicchange method starts.....................................................


   private void HandleTopicChange(TopicManager.Topic newTopic)
{
    currentTopic = newTopic;
    waitingForTransitionResponse = true; // ✅ Set the transition flag

    Debug.Log("Topic Updated: " + newTopic.ToString());
}


//HandleTopicChange Method ends .........................................................................................................


    private IEnumerator RequestImprovedFeedback(string userMessage)
{
    Debug.Log("Requesting improved feedback for: " + userMessage);

    string improvedResponse = "";

    // ✅ Improved GPT Prompt for Clearer Feedback
    var feedbackRequest = new
    {
        model = "gpt-4o",
        messages = new GPTMessage[]
        {
            new GPTMessage
            {
                role = "system",
                content = "You are an expert English communication coach. " +
              "When the user says something, analyze their response and provide a better way to say it. " +
              "Consider the context of the conversation so far to provide coherent, meaningful, and natural improvements. " +
              "STRICT INSTRUCTION: Provide improved responses that fit the conversation's context. No greetings or extra comments."
            },
            new GPTMessage
            {
                role = "user",
                content = userMessage
            }
        }
    };

    string jsonData = JsonConvert.SerializeObject(feedbackRequest);
    byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

    UnityWebRequest request = new UnityWebRequest(gptApiUrl, "POST");
    request.uploadHandler = new UploadHandlerRaw(jsonBytes);
    request.downloadHandler = new DownloadHandlerBuffer();
    request.SetRequestHeader("Authorization", "Bearer " + apiKey);
    request.SetRequestHeader("Content-Type", "application/json");

    yield return request.SendWebRequest();

    if (request.result == UnityWebRequest.Result.Success)
    {
        string responseText = request.downloadHandler.text;
        var responseJson = JsonConvert.DeserializeObject<GPTResponse>(responseText);

        if (responseJson != null && responseJson.choices.Length > 0)
        {
            improvedResponse = responseJson.choices[0].message.content.Trim();

            // ✅ Display Both User's Response and Improved Response on the Training Panel
            string feedbackMessage = $"You said: \n\"{userMessage}\"\n\nBetter way to say it:\n \"{improvedResponse}\" ";

            trainingPanelManager.ShowFeedback(feedbackMessage);

            Debug.Log("User Said: " + userMessage);
            Debug.Log("Improved Feedback: " + improvedResponse);
        }
    }
    else
    {
        Debug.LogError("Error requesting improved feedback: " + request.error);
    }


}





    private void SendAudioToWhisper(AudioClip clip)
    {
        byte[] audioData = WavUtility.FromAudioClip(clip);
        StartCoroutine(SendAudioToSTT(audioData));
    }






   private IEnumerator SendAudioToSTT(byte[] audioData)
{
    string filePath = Application.persistentDataPath + "/recorded_audio.wav";
    File.WriteAllBytes(filePath, audioData);

    if (!File.Exists(filePath))
    {
        Debug.LogError("Audio file not found at path: " + filePath);
        yield break;
    }

    byte[] fileData = File.ReadAllBytes(filePath);

    WWWForm form = new WWWForm();
    form.AddField("model", "whisper-1");
    form.AddBinaryData("file", fileData, "recorded_audio.wav", "audio/wav");

    UnityWebRequest request = UnityWebRequest.Post(sttApiUrl, form);
    request.SetRequestHeader("Authorization", "Bearer " + apiKey);

    yield return request.SendWebRequest();

    if (request.result == UnityWebRequest.Result.Success)
    {
        string responseText = request.downloadHandler.text;
        var responseJson = JsonConvert.DeserializeObject<OpenAIResponse>(responseText);

        if (responseJson != null && !string.IsNullOrEmpty(responseJson.text))
        {
            // ✅ Request Improved Feedback FIRST
            StartCoroutine(RequestImprovedFeedback(responseJson.text));

            // ✅ Add User's Message to Conversation History
            conversationHistory.Add(new GPTMessage { role = "user", content = responseJson.text });

            // ✅ Send Text to GPT (Aditya's Response)
            StartCoroutine(SendTextToGPT(responseJson.text));
        }
    }
    else
    {
        Debug.LogError("Error sending audio: " + request.error);
    }
}


    
    // *******************************SendTextToGPT Method starts *************************************************************************************************
    
    private IEnumerator SendTextToGPT(string userMessage)
{
    Debug.Log("Sending text to GPT-4o: " + userMessage);

    string conclusionMessage = "";
    string nextTopicQuestion = "";

    // ✅ Handle Conclusion and Next Topic Transition Logic
    if (waitingForTransitionResponse)
    {
        // ✅ Conclude User's Last Statement
        conclusionMessage = "That's interesting! ";
        waitingForTransitionResponse = false; // ✅ Reset the flag

        // ✅ Ask a Question Related to the New Topic
        switch (currentTopic)
        {
            case TopicManager.Topic.Introduction:
                nextTopicQuestion = "Can you tell me a bit about yourself?";
                break;
            case TopicManager.Topic.StudyWork:
                nextTopicQuestion = "Are you currently studying, or are you working?";
                break;
            case TopicManager.Topic.Hobby:
                nextTopicQuestion = "What are your favorite hobbies or pastimes?";
                break;
            case TopicManager.Topic.Experience:
                nextTopicQuestion = "Have you had any memorable experiences that shaped you?";
                break;
            case TopicManager.Topic.Ambition:
                nextTopicQuestion = "What are some of your goals or dreams for the future?";
                break;
        }
    }

    // ✅ Append Active Topic Reminder to Keep Aditya Focused
    string topicReminder = "";

    switch (currentTopic)
    {
        case TopicManager.Topic.Introduction:
            topicReminder = "Ask only about introductions for now.";
            break;
        case TopicManager.Topic.StudyWork:
            topicReminder = "Ask only about their studies or work for now.";
            break;
        case TopicManager.Topic.Hobby:
            topicReminder = "Ask only about hobbies for now.";
            break;
        case TopicManager.Topic.Experience:
            topicReminder = "Ask only about their experiences for now.";
            break;
        case TopicManager.Topic.Ambition:
            topicReminder = "Ask only about their ambitions and goals for now.";
            break;
    }

    // ✅ Include Topic Reminder and Reinforce Trainer Personality
    conversationHistory.Add(new GPTMessage
    {
        role = "system",
        content = topicReminder + 
                  " Continue asking follow-up questions, exploring the current topic deeply until the user presses the 'Next Topic Button'. " +
          "Do NOT suggest moving to the next topic. " +
          "Do NOT provide language feedback or improvements directly in the conversation. " +  // 🚨 Important Line Added
          "If the user provides a brief response, ask a follow-up question to keep the conversation engaging. " +
          "Remember, you are Aditya — a 28-year-old English communication trainer from Mumbai who works at a language learning center. " +
          "You love playing guitar, hiking, and reading motivational books. Occasionally, feel free to share your experiences or hobbies if they fit the conversation. " +
          "Be warm, encouraging, and engaging. Offer brief positive reinforcement after the user's responses."
    });

    // ✅ Add User's Message to Conversation History
    conversationHistory.Add(new GPTMessage { role = "user", content = userMessage });

    // ✅ Combine Conclusion + Next Topic's First Question (if applicable)
    string combinedResponse = conclusionMessage + nextTopicQuestion;


    // ✅ Add GPT System Instruction for Feedback
    conversationHistory.Add(new GPTMessage
   {
    role = "system",
    content = "As a communication coach, when the user says something, analyze their response and provide improved suggestions. " +
              "Offer concise and effective ways to improve the clarity, structure, and vocabulary of their response. " +
              "Ensure your suggestions are practical and natural. Strictly respond in English."
   });







    if (!string.IsNullOrEmpty(combinedResponse))
    {
        conversationHistory.Add(new GPTMessage
        {
            role = "assistant",
            content = combinedResponse
        });

        // ✅ Send Response to TTS
        StartCoroutine(SendTextToTTS(combinedResponse));

        Debug.Log("Aditya's Response: " + combinedResponse);

        // ✅ Exit here since no further GPT request is needed after transition logic
        yield break;
    }

    // ✅ Continue with GPT Request (if no transition logic is triggered)
    var requestData = new
    {
        model = "gpt-4o",
        messages = conversationHistory.ToArray()  // ✅ Maintain Conversation Context
    };

    string jsonData = JsonConvert.SerializeObject(requestData);
    byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

    UnityWebRequest request = new UnityWebRequest(gptApiUrl, "POST");
    request.uploadHandler = new UploadHandlerRaw(jsonBytes);
    request.downloadHandler = new DownloadHandlerBuffer();
    request.SetRequestHeader("Authorization", "Bearer " + apiKey);
    request.SetRequestHeader("Content-Type", "application/json");

    yield return request.SendWebRequest();

    if (request.result == UnityWebRequest.Result.Success)
    {
        string responseText = request.downloadHandler.text;
        var responseJson = JsonConvert.DeserializeObject<GPTResponse>(responseText);

        if (responseJson != null && responseJson.choices.Length > 0)
        {
            string aiResponse = responseJson.choices[0].message.content;
            
            // ✅ Add AI's Response to Conversation History
            conversationHistory.Add(new GPTMessage { role = "assistant", content = aiResponse });

            // ✅ Send Response to TTS
            StartCoroutine(SendTextToTTS(aiResponse));
        }
    }
    else
    {
        Debug.LogError("Error sending text to GPT-4o: " + request.error);
    }
}




    //******************************** SendTextToGPT method ends **********************************************************************************************

    private IEnumerator SendTextToTTS(string text)
    {
        Debug.Log("Sending text to TTS: " + text);

        var jsonData = new
        {
            model = "tts-1",
            input = text,
            voice = "alloy",
            response_format = "mp3",
            speed = 0.9f,  // ✅ Slightly slower for clarity (Indian pacing)
            pitch = -0.5f   // ✅ Slightly deeper tone for warmth
        };

        string jsonPayload = JsonConvert.SerializeObject(jsonData);
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonPayload);

        UnityWebRequest request = new UnityWebRequest(ttsApiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            byte[] audioData = request.downloadHandler.data;
            PlayAudioFromBytes(audioData);
        }
        else
        {
            Debug.LogError("TTS Error: " + request.error);
        }
    }

    private void PlayAudioFromBytes(byte[] audioData)
    {
        string tempPath = Application.persistentDataPath + "/response.mp3";
        File.WriteAllBytes(tempPath, audioData);
        StartCoroutine(LoadAudioClip(tempPath));
    }

    private IEnumerator LoadAudioClip(string path)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                audioSource.clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.Play();
                characterAnimator.SetBool("IsTalking", true);

                while (audioSource.isPlaying)
                {
                    yield return null;
                }

                characterAnimator.SetBool("IsTalking", false);
            }
            else
            {
                Debug.LogError("Error loading audio clip: " + www.error);
            }
        }
    }
}

// ✅ Whisper API Response
[System.Serializable]
public class OpenAIResponse
{
    public string text;
}

// ✅ GPT API Response
[System.Serializable]
public class GPTResponse
{
    public GPTChoice[] choices;
}

[System.Serializable]
public class GPTChoice
{
    public GPTMessage message;
}

[System.Serializable]
public class GPTMessage
{
    public string role;
    public string content;
}
